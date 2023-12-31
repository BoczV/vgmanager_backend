using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VGManager.Api.Projects.Responses;
using VGManager.Services.Interfaces;
using VGManager.Services.Models.Projects;

namespace VGManager.Api.Projects;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IMapper _mapper;

    public ProjectController(IProjectService projectService, IMapper mapper)
    {
        _projectService = projectService;
        _mapper = mapper;
    }

    [HttpPost("Get", Name = "getprojects")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectsResponse>> GetAsync(
        [FromBody] ProjectRequest request,
        CancellationToken cancellationToken
    )
    {
        var projectModel = _mapper.Map<ProjectModel>(request);
        var projects = await _projectService.GetProjectsAsync(projectModel, cancellationToken);

        var result = _mapper.Map<ProjectsResponse>(projects);
        return Ok(result);
    }
}
