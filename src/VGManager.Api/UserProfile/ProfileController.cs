using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VGManager.Api.Projects.Responses;
using VGManager.Api.Projects;
using VGManager.Services;
using VGManager.Services.Interfaces;
using VGManager.Services.Models.Projects;
using VGManager.AzureAdapter.Entities;

namespace VGManager.Api.UserProfile;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IMapper _mapper;

    public ProfileController(IProfileService profileService, IMapper mapper)
    {
        _profileService = profileService;
        _mapper = mapper;
    }

    [HttpPost("Get", Name = "getprofile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectsResponse>> GetAsync(
        [FromBody] ProfileRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var profile = await _profileService.GetProfileAsync(request.Organization, request.PAT, cancellationToken);
            if(profile is null)
            {
                return Ok(new ProfileResponse()
                {
                    Profile = null!,
                    Status = Status.Unknown
                });
            }
            return Ok(new ProfileResponse()
            {
                Profile = profile,
                Status = Status.Success 
            });
        } 
        catch (Exception)
        {
            return Ok(new ProfileResponse()
            {
                Profile = null!,
                Status = Status.Unknown
            });
        }
        
    }
}
