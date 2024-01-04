using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using VGManager.AzureAdapter.Entities;
using VGManager.AzureAdapter.Interfaces;

namespace VGManager.AzureAdapter;

public class ReleasePipelineAdapter: IReleasePipelineAdapter
{
    private VssConnection _connection = null!;
    private readonly ILogger _logger;

    public ReleasePipelineAdapter(ILogger<ReleasePipelineAdapter> logger)
    {
        _logger = logger;
    }

    public async Task<(AdapterStatus, IEnumerable<string>)> GetEnvironmentsAsync(
        string organization,
        string pat,
        string project,
        string repositoryName,
        CancellationToken cancellationToken = default
        )
    {
        try
        {
            _logger.LogInformation("Request git branches from {project} azure project.", project);
            var definition = await GetReleaseDefinitionAsync(organization, pat, project, repositoryName, cancellationToken);
            return (
                definition is null ? AdapterStatus.Unknown: AdapterStatus.Success, 
                definition?.Environments.Select(env => env.Name).ToList() ?? Enumerable.Empty<string>()
                );
        }
        catch (ProjectDoesNotExistWithNameException ex)
        {
            _logger.LogError(ex, "{project} azure project is not found.", project);
            return (AdapterStatus.ProjectDoesNotExist, Enumerable.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting git branches from {project} azure project.", project);
            return (AdapterStatus.Unknown, Enumerable.Empty<string>());
        }
    }

    public async Task<(AdapterStatus, IEnumerable<string>)> GetVariableGroupsAsync(
        string organization,
        string pat,
        string project,
        string repositoryName,
        CancellationToken cancellationToken = default
        )
    {
        try
        {
            _logger.LogInformation("Request git branches from {project} azure project.", project);
            var definition = await GetReleaseDefinitionAsync(organization, pat, project, repositoryName, cancellationToken);

            if (definition is null)
            {
                return (AdapterStatus.Unknown, Enumerable.Empty<string>());
            }

            var variableGroupNames = await GetVariableGroupNames(project, definition, cancellationToken);

            return (AdapterStatus.Success, variableGroupNames);
        }
        catch (ProjectDoesNotExistWithNameException ex)
        {
            _logger.LogError(ex, "{project} azure project is not found.", project);
            return (AdapterStatus.ProjectDoesNotExist, Enumerable.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting git branches from {project} azure project.", project);
            return (AdapterStatus.Unknown, Enumerable.Empty<string>());
        }
    }

    private async Task<List<string>> GetVariableGroupNames(string project, ReleaseDefinition definition, CancellationToken cancellationToken)
    {
        var taskAgentClient = await _connection.GetClientAsync<TaskAgentHttpClient>(cancellationToken: cancellationToken);
        var variableGroupNames = new List<string>();

        foreach (var env in definition.Environments)
        {
            foreach (var id in env.VariableGroups)
            {
                var vg = await taskAgentClient.GetVariableGroupAsync(project, id, cancellationToken: cancellationToken);
                variableGroupNames.Add(vg.Name);
            }
        }

        return variableGroupNames;
    }

    private async Task<ReleaseDefinition?> GetReleaseDefinitionAsync(
        string organization, 
        string pat, 
        string project, 
        string repositoryName, 
        CancellationToken cancellationToken
        )
    {
        Setup(organization, pat);
        var releaseClient = await _connection.GetClientAsync<ReleaseHttpClient>(cancellationToken);
        var expand = ReleaseDefinitionExpands.Artifacts;
        var releaseDefinitions = await releaseClient.GetReleaseDefinitionsAsync(
            project,
            expand: expand,
            cancellationToken: cancellationToken
            );

        var result = releaseDefinitions.FirstOrDefault(
            definition => definition.Artifacts.Any(artifact => artifact.Alias.Contains(repositoryName))
        );

        return await releaseClient.GetReleaseDefinitionAsync(project, result?.Id ?? 0, cancellationToken: cancellationToken);
    }

    private void Setup(string organization, string pat)
    {
        var uriString = $"https://dev.azure.com/{organization}";
        Uri uri;
        Uri.TryCreate(uriString, UriKind.Absolute, out uri!);

        var credentials = new VssBasicCredential(string.Empty, pat);
        _connection = new VssConnection(uri, credentials);
    }
}
