using VGManager.Api.VariableGroups.Request;
using VGManager.Api.VariableGroups.Response;
using VGManager.AzureAdapter.Entities;
using VGManager.Services.Models.Projects;
using VGManager.Services.Models.VariableGroups.Requests;

namespace VGManager.Api.Controllers;

public partial class VariableGroupController
{
    private static VariableGroupResponses GetEmptyVariableGroupGetResponses()
    {
        return new VariableGroupResponses
        {
            Status = Status.Success,
            VariableGroups = new List<VariableGroupResponse>()
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

    private async Task<VariableGroupResponses> GetResultAfterDeleteAsync(VariableGroupRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.DeleteVariablesAsync(vgServiceModel, true, cancellationToken);
        var variableGroupResultModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableGroupResponses>(variableGroupResultModel);

        if (status != Status.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<VariableGroupResponses> GetBaseResultAsync(VariableGroupRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var variableGroupResultsModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableGroupResponses>(variableGroupResultsModel);
        return result;
    }

    private async Task<VariableGroupResponses> GetAddResultAsync(VariableGroupAddRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupAddModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.AddVariablesAsync(vgServiceModel, cancellationToken);
        vgServiceModel.KeyFilter = vgServiceModel.Key;
        vgServiceModel.ValueFilter = vgServiceModel.Value;
        var variableGroupResultModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableGroupResponses>(variableGroupResultModel);

        if (status != Status.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<VariableGroupResponses> GetUpdateResultAsync(VariableGroupUpdateRequest request, CancellationToken cancellationToken)
    {
        var vgServiceModel = _mapper.Map<VariableGroupUpdateModel>(request);

        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.UpdateVariableGroupsAsync(vgServiceModel, true, cancellationToken);

        vgServiceModel.ValueFilter = vgServiceModel.NewValue;
        var variableGroupResultModel = await _vgService.GetVariableGroupsAsync(vgServiceModel, cancellationToken);

        var result = _mapper.Map<VariableGroupResponses>(variableGroupResultModel);

        if (status != Status.Success)
        {
            result.Status = status;
        }

        return result;
    }

    private async Task<VariableGroupResponses> GetVariableGroupResponsesAsync<T>(T request, CancellationToken cancellationToken)
    {
        VariableGroupResponses? result;
        var vgRequest = request as VariableGroupRequest ?? new VariableGroupRequest();
        if (vgRequest.Project == "All")
        {
            result = GetEmptyVariableGroupGetResponses();
            var projectResponse = await GetProjectsAsync(vgRequest, cancellationToken);

            foreach (var project in projectResponse.Projects)
            {
                vgRequest.Project = project.Name;
                var subResult = await GetResultAsync(request, vgRequest, cancellationToken);
                result.VariableGroups.AddRange(subResult.VariableGroups);

                if (subResult.Status != Status.Success)
                {
                    result.Status = subResult.Status;
                }
            }
        }
        else
        {
            result = await GetResultAsync(request, vgRequest, cancellationToken);
        }
        return result;
    }

    private async Task<VariableGroupResponses> GetResultAsync<T>(T request, VariableGroupRequest vgRequest, CancellationToken cancellationToken)
    {
        if (request is VariableGroupUpdateRequest updateRequest)
        {
            return await GetUpdateResultAsync(updateRequest, cancellationToken);
        }
        else if (request is VariableGroupAddRequest addRequest)
        {
            return await GetAddResultAsync(addRequest, cancellationToken);
        }
        else
        {
            return await GetBaseResultAsync(vgRequest, cancellationToken);
        }
    }
}
