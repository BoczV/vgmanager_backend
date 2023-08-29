﻿
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using VGManager.Repository.Interfaces;

namespace VGManager.Repository;

public class VariableGroupConnectionRepository : IVariableGroupConnectionRepository
{
    private VssConnection _connection = null!;
    private string _project = null!;

    public void Setup(string organization, string project, string pat)
    {
        _project = project;

        var uriString = $"https://dev.azure.com/{organization}";
        Uri uri;
        Uri.TryCreate(uriString, UriKind.Absolute, out uri!);

        var credentials = new VssBasicCredential(string.Empty, pat);
        _connection = new VssConnection(uri, credentials);
    }

    public async Task<IEnumerable<VariableGroup>> GetAll(CancellationToken cancellationToken = default)
    {
        var httpClient = _connection.GetClient<TaskAgentHttpClient>(cancellationToken: cancellationToken);
        var variableGroups = await httpClient.GetVariableGroupsAsync(_project, cancellationToken: cancellationToken);
        return variableGroups;
    }

    public async Task Update(VariableGroupParameters variableGroupParameters, int variableGroupId, CancellationToken cancellationToken = default)
    {
        variableGroupParameters.VariableGroupProjectReferences = new List<VariableGroupProjectReference>()
        {
            new VariableGroupProjectReference()
            {
                Name = variableGroupParameters.Name,
                ProjectReference = new ProjectReference()
                {
                    Name = _project
                }
            }
        };

        var httpClient = _connection.GetClient<TaskAgentHttpClient>(cancellationToken: cancellationToken);
        await httpClient!.UpdateVariableGroupAsync(variableGroupId, variableGroupParameters, cancellationToken: cancellationToken);
    }
}
