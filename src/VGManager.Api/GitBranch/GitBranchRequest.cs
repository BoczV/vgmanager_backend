using System.ComponentModel.DataAnnotations;

namespace VGManager.Api.Branch;

public class GitBranchRequest
{
    [Required]
    public string Organization { get; set; } = null!;

    [Required]
    public string GitProject { get; set; } = null!;

    [Required]
    public string PAT { get; set; } = null!;
}
