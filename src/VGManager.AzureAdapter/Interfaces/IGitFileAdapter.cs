using VGManager.Models.StatusEnums;

namespace VGManager.AzureAdapter.Interfaces;

public interface IGitFileAdapter
{
    Task<(AdapterStatus, IEnumerable<string>)> GetFilePathAsync(
        string organization,
        string pat,
        string repositoryId,
        string fileName,
        string branch,
        CancellationToken cancellationToken = default
        );

    Task<(AdapterStatus, IEnumerable<string>)> GetConfigFilesAsync(
        string organization,
        string pat,
        string repositoryId,
        string? extension,
        string branch,
        CancellationToken cancellationToken = default
        );
}
