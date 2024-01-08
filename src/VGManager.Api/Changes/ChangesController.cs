using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using VGManager.Api.Changes.Request;
using VGManager.Models;
using VGManager.Services.Interfaces;
using VGManager.Services.Models.Changes.Requests;
using VGManager.Services.Models.Changes.Responses;

namespace VGManager.Api.Changes;

[Route("api/[controller]")]
[ApiController]
[EnableCors("_allowSpecificOrigins")]
public class ChangesController : ControllerBase
{

    private readonly IChangeService _changesService;
    private readonly IMapper _mapper;

    public ChangesController(IChangeService changesService, IMapper mapper)
    {
        _changesService = changesService;
        _mapper = mapper;
    }

    [HttpPost("Variables", Name = "getvariablechanges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RepositoryResponseModel<VGOperationModel>>> GetVariableChangesAsync(
        [FromBody] VGChangesRequest request,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var result = await _changesService.GetAsync(_mapper.Map<VGRequestModel>(request), cancellationToken);
            return Ok(new RepositoryResponseModel<VGOperationModel>
            {
                Status = RepositoryStatus.Success,
                Data = result
            });
        }
        catch (Exception)
        {
            return Ok(new RepositoryResponseModel<VGOperationModel>
            {
                Status = RepositoryStatus.Unknown,
                Data = Array.Empty<VGOperationModel>()
            });
        }
    }

    [HttpPost("Secrets", Name = "getsecretchanges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RepositoryResponseModel<SecretOperationModel>>> GetSecretChangesAsync(
        [FromBody] SecretChangesRequest request,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var result = await _changesService.GetAsync(_mapper.Map<SecretRequestModel>(request), cancellationToken);
            return Ok(new RepositoryResponseModel<SecretOperationModel>
            {
                Status = RepositoryStatus.Success,
                Data = result
            });
        }
        catch (Exception)
        {
            return Ok(new RepositoryResponseModel<SecretOperationModel>
            {
                Status = RepositoryStatus.Unknown,
                Data = Array.Empty<SecretOperationModel>()
            });
        }
    }

    [HttpPost("KeyVaultCopies", Name = "getkvchanges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RepositoryResponseModel<KVOperationModel>>> GetKVChangesAsync(
        [FromBody] KVChangesRequest request,
        CancellationToken cancellationToken
        )
    {
        try
        {
            var result = await _changesService.GetAsync(_mapper.Map<KVRequestModel>(request), cancellationToken);
            return Ok(new RepositoryResponseModel<KVOperationModel>
            {
                Status = RepositoryStatus.Success,
                Data = result
            });
        }
        catch (Exception)
        {
            return Ok(new RepositoryResponseModel<KVOperationModel>
            {
                Status = RepositoryStatus.Unknown,
                Data = Array.Empty<KVOperationModel>()
            });
        }
    }
}
