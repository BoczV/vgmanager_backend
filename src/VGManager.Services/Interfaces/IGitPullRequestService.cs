using VGManager.Adapter.Models.Models;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Response;

namespace VGManager.Services.Interfaces;
public interface IGitPullRequestService
{
    Task<AdapterResponseModel<List<GitPRResponse>>> GetPRsAsync(
        GitPRRequest model,
        CancellationToken cancellationToken
        );

    Task<AdapterResponseModel<bool>> CreatePullRequestAsync(
        CreatePRRequest model,
        CancellationToken cancellationToken
        );

    Task<AdapterResponseModel<bool>> CreatePullRequestsAsync(
        CreatePRsRequest model,
        CancellationToken cancellationToken
        );

    Task<AdapterResponseModel<bool>> ApprovePullRequestsAsync(
        ApprovePRsRequest request,
        CancellationToken cancellationToken
        );
}