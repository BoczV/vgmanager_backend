using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text;
using System.Text.Json;
using VGManager.AzureAdapter.Entities;
using VGManager.AzureAdapter.Interfaces;
using YamlDotNet.RepresentationModel;

namespace VGManager.AzureAdapter;

public class GitRepositoryAdapter : IGitRepositoryAdapter
{
    private VssConnection _connection = null!;
    private readonly ILogger _logger;

    private readonly char[] _notAllowedCharacters = { '{', '}', ' ', '(', ')', '$' };
    private readonly char _startingChar = '$';
    private readonly char _endingChar = '}';
    private readonly string _secretYamlKind = "Secret";
    private readonly string _secretYamlElement = "stringData";
    private readonly string _variableYamlKind = "ConfigMap";
    private readonly string _variableYamlElement = "data";

    public GitRepositoryAdapter(ILogger<GitRepositoryAdapter> logger)
    {
        _logger = logger;
    }

    public void Setup(string organization, string pat)
    {
        var uriString = $"https://dev.azure.com/{organization}";
        Uri uri;
        Uri.TryCreate(uriString, UriKind.Absolute, out uri!);

        var credentials = new VssBasicCredential(string.Empty, pat);
        _connection = new VssConnection(uri, credentials);
    }

    public async Task<IEnumerable<GitRepository>> GetAllAsync(
        string organization,
        string project,
        string pat,
        CancellationToken cancellationToken = default
        )
    {
        _logger.LogInformation("Request git repositories from {project} azure project.", project);
        Setup(organization, pat);
        var gitClient = await _connection.GetClientAsync<GitHttpClient>(cancellationToken);
        var repositories = await gitClient.GetRepositoriesAsync(cancellationToken: cancellationToken);
        return repositories.Where(repo => (!repo.IsDisabled ?? false) && repo.ProjectReference.Name == project).ToList();
    }

    public async Task<List<string>> GetVariablesFromConfigAsync(
        GitRepositoryEntity gitRepositoryEntity,
        CancellationToken cancellationToken = default
        )
    {
        var project = gitRepositoryEntity.Project;
        var gitRepositoryId = gitRepositoryEntity.GitRepositoryId;

        _logger.LogInformation(
            "Requesting configurations from {project} azure project, {gitRepositoryId} git repository.",
            project,
            gitRepositoryId
            );

        var gitClient = await _connection.GetClientAsync<GitHttpClient>(cancellationToken);
        var gitVersionDescriptor = new GitVersionDescriptor
        {
            VersionType = GitVersionType.Branch,
            Version = gitRepositoryEntity.Branch
        };

        var item = await gitClient.GetItemTextAsync(
            project: project,
            repositoryId: gitRepositoryId,
            path: gitRepositoryEntity.FilePath,
            versionDescriptor: gitVersionDescriptor,
            cancellationToken: cancellationToken
            );

        if (gitRepositoryEntity.FilePath.EndsWith(".json"))
        {
            var json = await GetJsonObjectAsync(item, cancellationToken);
            var result = GetKeysFromJson(json, gitRepositoryEntity.Exceptions ?? Enumerable.Empty<string>(), gitRepositoryEntity.Delimiter);
            return result;
        }
        else if (gitRepositoryEntity.FilePath.EndsWith(".yaml"))
        {
            return GetKeysFromYaml(item);
        }
        else
        {
            return Enumerable.Empty<string>().ToList();
        }
    }

    private static List<string> GetKeysFromJson(JsonElement jsonObject, IEnumerable<string> exceptions, string delimiter)
    {
        var keys = new List<string>();
        GetKeysFromJsonHelper(jsonObject, delimiter, string.Empty, exceptions, keys);
        return keys;
    }

    private static void GetKeysFromJsonHelper(
        JsonElement jsonObject,
        string delimiter,
        string prefix,
        IEnumerable<string> exceptions,
        List<string> keys
        )
    {
        if (jsonObject.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in jsonObject.EnumerateObject())
            {
                var name = property.Name;
                var key = prefix + name + delimiter;

                if (property.Value.ValueKind != JsonValueKind.Object || exceptions.Contains(property.Name))
                {
                    keys.Add(key.Remove(key.Length - delimiter.Length));
                }

                if (!exceptions.Contains(property.Name))
                {
                    GetKeysFromJsonHelper(property.Value, delimiter, key, exceptions, keys);
                }
            }
        }
    }

    private static async Task<JsonElement> GetJsonObjectAsync(Stream item, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        var buffer = new byte[2048];
        int bytesRead;
        while ((bytesRead = await item.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }
        var result = stream.ToArray();
        var itemText = Encoding.UTF8.GetString(result);
        return JsonSerializer.Deserialize<JsonElement>(itemText);
    }

    private List<string> GetKeysFromYaml(Stream item)
    {
        var yamls = GetYamlDocuments(item);
        var result = new List<string>();
        var counter = 0;
        foreach (var yaml in yamls)
        {
            var subResult = CollectKeysFromYaml(yaml, counter == 0 ? _variableYamlElement : _secretYamlElement);
            result.AddRange(subResult);
            counter++;
            if (counter == 2)
            {
                break;
            }
        }
        return result;
    }

    private IEnumerable<YamlDocument> GetYamlDocuments(Stream item)
    {
        var reader = new StreamReader(item);
        var yaml = new YamlStream();
        yaml.Load(reader);
        return yaml.Documents.Where(
            document => document.AllNodes.Contains(_secretYamlKind) || document.AllNodes.Contains(_variableYamlKind)
            ).ToList();
    }

    private IEnumerable<string> CollectKeysFromYaml(YamlDocument yaml, string nodeKey)
    {
        var data = yaml.AllNodes.FirstOrDefault(node => node.ToString().Contains(nodeKey));
        var strNode = data?.ToString() ?? string.Empty;
        var listNode = strNode.Split($" {nodeKey}").ToList();
        var rawVariables = listNode[1].Split(",");
        return CollectKeysFromYaml(rawVariables);
    }

    private IEnumerable<string> CollectKeysFromYaml(string[] rawVariables)
    {
        var result = new List<string>();
        foreach (var rawVariable in rawVariables)
        {
            var strBuilder = new StringBuilder();
            var startCollecting = false;
            foreach (var character in rawVariable)
            {
                if (character == _startingChar)
                {
                    startCollecting = true;
                }
                if (startCollecting &&
                    !_notAllowedCharacters.Contains(character)
                    )
                {
                    strBuilder.Append(character);
                }
                else if (character == _endingChar)
                {
                    startCollecting = false;
                }
            }
            var strResult = strBuilder.ToString();
            if (!strResult.IsNullOrEmpty())
            {
                result.Add(strResult);
            }
        }
        return result;
    }
}
