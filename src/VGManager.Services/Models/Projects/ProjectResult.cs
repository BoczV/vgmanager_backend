using Microsoft.TeamFoundation.Core.WebApi;

namespace VGManager.Services.Models.Projects;

public class ProjectResult
{
    public TeamProjectReference Project { get; set; } = null!;
    public IEnumerable<string> SubscriptionIds { get; set; } = null!;
}
