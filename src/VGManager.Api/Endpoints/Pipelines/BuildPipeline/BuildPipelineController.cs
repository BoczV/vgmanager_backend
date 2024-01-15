using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using VGManager.Api.Endpoints.Pipelines.BuildPipeline;
using VGManager.Models.Models;
using VGManager.Models.StatusEnums;
using VGManager.Services.Interfaces;
using static Microsoft.Azure.Pipelines.WebApi.PipelinesResources;

namespace VGManager.Api.Endpoints.Pipelines.Build;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public class BuildPipelineController : ControllerBase
{
    private readonly IBuildPipelineService _buildPipelineService;

    public BuildPipelineController(IBuildPipelineService buildPipelineService)
    {
        _buildPipelineService = buildPipelineService;
    }

    [HttpPost("GetAll", Name = "getall")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterResponseModel<IEnumerable<(string, string)>>>> GetAll(
        [FromBody] BuildPipelineRequest request,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var pipelines = await _buildPipelineService.GetBuildPipelinesAsync(
                request.Organization,
                request.PAT,
                request.Project,
                cancellationToken
                );
            return Ok(new AdapterResponseModel<IEnumerable<(string, string)>>()
            {
                Status = AdapterStatus.Success,
                Data = pipelines
            });
        }
        catch (Exception)
        {
            return Ok(new AdapterResponseModel<IEnumerable<(string, string)>>()
            {
                Status = AdapterStatus.Unknown,
                Data = Enumerable.Empty<(string, string)>()
            });
        }
    }

    [HttpPost("Run", Name = "run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdapterStatus>> RunBuildPipelineAsync(
        [FromBody] BuildPipelineRequest request,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var status = await _buildPipelineService.RunBuildPipelineAsync(
                request.Organization,
                request.PAT,
                request.Project,
                request.DefinitionId,
                request.SourceBranch,
                cancellationToken
                );
            return Ok(status);
        }
        catch (Exception)
        {
            return Ok(AdapterStatus.Unknown);
        }
    }
}
