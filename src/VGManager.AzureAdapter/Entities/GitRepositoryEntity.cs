namespace VGManager.AzureAdapter.Entities;

public class GitRepositoryEntity
{
    public string Organization { get; set; } = null!;

    public string Project { get; set; } = null!;

    public string PAT { get; set; } = null!;

    public string GitRepositoryId { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string Delimiter { get; set; } = null!;

    public IEnumerable<string>? Exceptions { get; set; } = null!;
}
