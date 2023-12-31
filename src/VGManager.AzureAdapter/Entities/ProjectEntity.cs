using Microsoft.TeamFoundation.Core.WebApi;

namespace VGManager.AzureAdapter.Entities;
public class ProjectEntity
{
    public TeamProjectReference Project { get; set; } = null!;
    public IEnumerable<string> SubscriptionIds { get; set; } = null!;
}
