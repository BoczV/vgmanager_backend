using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Services.Common;
using VGManager.Api.VariableGroup.Response;
using VGManager.Api.VariableGroups.Request;
using VGManager.Api.VariableGroups.Response;
using VGManager.AzureAdapter.Entities;
using VGManager.Services.Interfaces;
using VGManager.Services.Models.VariableGroups.Requests;

namespace VGManager.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public partial class VariableGroupController : ControllerBase
{
    private readonly IVariableGroupService _vgService;
    private readonly IProjectService _projectService;
    private readonly IMapper _mapper;

    public VariableGroupController(
        IVariableGroupService vgService,
        IProjectService projectService,
        IMapper mapper
        )
    {
        _vgService = vgService;
        _projectService = projectService;
        _mapper = mapper;
    }

    [HttpPost("Get", Name = "GetVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VariableResponses>> GetAsync(
        [FromBody] VariableGroupRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request.UserName, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("GetVariableGroups", Name = "GetVariableGroups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VariableGroupResponses>> GetVariableGroupsAsync(
        [FromBody] VariableGroupRequest request,
        CancellationToken cancellationToken
    )
    {
        var subResult = await GetVariableGroupResponsesAsync(request.UserName, request, cancellationToken);
        var result = GetResult(subResult);
        return Ok(result);
    }

    [HttpPost("Update", Name = "UpdateVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VariableResponses>> UpdateAsync(
        [FromBody] VariableGroupUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request.UserName, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("UpdateInline", Name = "UpdateVariableInline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> UpdateInlineAsync(
        [FromBody] VariableGroupUpdateRequest request,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = _mapper.Map<VariableGroupUpdateModel>(request);
        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.UpdateVariableGroupsAsync(request.UserName, vgServiceModel, false, cancellationToken);

        return Ok(status);
    }

    [HttpPost("Add", Name = "AddVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VariableResponses>> AddAsync(
        [FromBody] VariableGroupAddRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await GetVariableGroupResponsesAsync(request.UserName, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("Delete", Name = "DeleteVariables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VariableResponses>> DeleteAsync(
        [FromBody] VariableGroupRequest request,
        CancellationToken cancellationToken
    )
    {
        VariableResponses? result;
        if (request.Project == "All")
        {
            result = GetEmptyVariableGroupGetResponses();
            var projectResponse = await GetProjectsAsync(request, cancellationToken);

            foreach (var project in projectResponse.Projects)
            {
                request.Project = project.Name;
                var subResult = await GetResultAfterDeleteAsync(request.UserName, request, cancellationToken);
                result.Variables.ToList().AddRange(subResult.Variables);

                if (subResult.Status != AdapterStatus.Success)
                {
                    result.Status = subResult.Status;
                }
            }
        }
        else
        {
            result = await GetResultAfterDeleteAsync(request.UserName, request, cancellationToken);
        }
        return Ok(result);
    }

    [HttpPost("DeleteInline", Name = "DeleteVariableInline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> DeleteInlineAsync(
        [FromBody] VariableGroupRequest request,
        CancellationToken cancellationToken
    )
    {
        var vgServiceModel = _mapper.Map<VariableGroupModel>(request);
        _vgService.SetupConnectionRepository(vgServiceModel);
        var status = await _vgService.DeleteVariablesAsync(request.UserName, vgServiceModel, false, cancellationToken);

        return Ok(status);
    }
}
