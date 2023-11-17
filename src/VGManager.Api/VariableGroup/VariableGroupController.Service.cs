using VGManager.Api.VariableGroup.Response;
using VGManager.Api.VariableGroups.Request;
using VGManager.Api.VariableGroups.Response;
using VGManager.AzureAdapter.Entities;
using VGManager.Services.Models.Projects;
using VGManager.Services.Models.VariableGroups.Requests;

namespace VGManager.Api.Controllers;

public partial class VariableGroupController
{
    private static VariableResponses GetEmptyVariableGroupGetResponses()
    {
        return new VariableResponses
        {
            Status = AdapterStatus.Success,
            Variables = new List<VariableResponse>()
        };
    }

    private async Task<ProjectResult> GetProjectsAsync(VariableGroupRequest request, CancellationToken cancellationToken)
    {
        var projectModel = new ProjectModel
        {
            Organization = request.Organization,
            PAT = request.PAT
        };

        var projectResponse = await _projectService.GetProjectsAsync(projectModel, cancellationToken);
        return projectResponse;
    }

    private async Task<VariableResponses> GetResultAfterDeleteAsync(
        string userName,
        VariableGroupRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.DeleteVariablesAsync(userName, vgServiceModel, true, cancellationToken);
        var variableGroupResultModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableResponses>(variableGroupResultModel);

        if (status != AdapterStatus.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<VariableResponses> GetBaseResultAsync(VariableGroupRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var variableGroupResultsModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableResponses>(variableGroupResultsModel);
        return result;
    }

    private async Task<VariableResponses> GetAddResultAsync(string userName, VariableGroupAddRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupAddModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.AddVariablesAsync(userName, vgServiceModel, cancellationToken);
        vgServiceModel.KeyFilter = vgServiceModel.Key;
        vgServiceModel.ValueFilter = vgServiceModel.Value;
        var variableGroupResultModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableResponses>(variableGroupResultModel);

        if (status != AdapterStatus.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<VariableResponses> GetUpdateResultAsync(
        string userName,
        VariableGroupUpdateRequest request,
        CancellationToken cancellationToken
        )
    {
        var vgServiceModel = _mapper.Map<VariableGroupUpdateModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.UpdateVariableGroupsAsync(userName, vgServiceModel, true, cancellationToken);

        vgServiceModel.ValueFilter = vgServiceModel.NewValue;
        var variableGroupResultModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableResponses>(variableGroupResultModel);

        if (status != AdapterStatus.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<VariableResponses> GetVariableGroupResponsesAsync<T>(
        string userName,
        T request,
        CancellationToken cancellationToken
        )
    {
        VariableResponses? result;
        var vgRequest = request as VariableGroupRequest ?? new VariableGroupRequest();
        if (vgRequest.Project == "All")
        {
            result = GetEmptyVariableGroupGetResponses();
            var projectResponse = await GetProjectsAsync(vgRequest, cancellationToken);

            foreach (var project in projectResponse.Projects)
            {
                vgRequest.Project = project.Name;
                var subResult = await GetResultAsync(userName, request, vgRequest, cancellationToken);
                result.Variables.ToList().AddRange(subResult.Variables);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
        }
        else
        {
            result = await GetResultAsync(userName, request, vgRequest, cancellationToken);
        }
        return result;
    }

    private async Task<VariableResponses> GetResultAsync<T>(
        string userName,
        T request,
        VariableGroupRequest vgRequest,
        CancellationToken cancellationToken
        )
    {
        if (request is VariableGroupUpdateRequest updateRequest)
        {
            return await GetUpdateResultAsync(userName, updateRequest, cancellationToken);
        }
        else if (request is VariableGroupAddRequest addRequest)
        {
            return await GetAddResultAsync(userName, addRequest, cancellationToken);
        }
        else
        {
            return await GetBaseResultAsync(vgRequest, cancellationToken);
        }
    }

    private static VariableGroupResponses GetResult(VariableResponses variableResponses)
    {
        var listResult = new List<VariableGroupResponse>();
        var result = new VariableGroupResponses
        {
            Status = variableResponses.Status
        };

        foreach (var variableResponse in variableResponses.Variables)
        {
            if (!listResult.Exists(
                item => item.VariableGroupName == variableResponse.VariableGroupName && item.Project == variableResponse.Project
                ))
            {
                listResult.Add(new()
                {
                    VariableGroupName = variableResponse.VariableGroupName,
                    Project = variableResponse.Project
                });
            }
        }

        result.VariableGroups = listResult;
        return result;
    }
}
