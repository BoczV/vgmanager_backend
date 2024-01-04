using Microsoft.Extensions.Logging;
using VGManager.AzureAdapter.Entities;
using VGManager.AzureAdapter.Interfaces;
using VGManager.Services.Interfaces;

namespace VGManager.Services;

public class ReleasePipelineService: IReleasePipelineService
{
    private readonly IReleasePipelineAdapter _releasePipelineAdapter;
    private readonly ILogger _logger;

    public ReleasePipelineService(
        IReleasePipelineAdapter releasePipelineAdapter,
        ILogger<ReleasePipelineService> logger
        )
    {
        _releasePipelineAdapter = releasePipelineAdapter;
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
            _logger.LogInformation("Request release environments for {repository} repository.", repositoryName);
            return await _releasePipelineAdapter.GetEnvironmentsAsync(organization, pat, project, repositoryName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting release environments for {repository} repository.", repositoryName);
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
            _logger.LogInformation("Request variable groups connected to release pipeline for {repository} repository.", repositoryName);
            return await _releasePipelineAdapter.GetVariableGroupsAsync(organization, pat, project, repositoryName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting variable groups connected to release pipeline for {repository} repository.", repositoryName);
            return (AdapterStatus.Unknown, Enumerable.Empty<string>());
        }
    }
}
