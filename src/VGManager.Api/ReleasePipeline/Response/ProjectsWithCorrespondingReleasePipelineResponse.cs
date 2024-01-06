using VGManager.AzureAdapter.Entities;

namespace VGManager.Api.ReleasePipeline.Response;

public class ProjectsWithCorrespondingReleasePipelineResponse
{
    public AdapterStatus Status { get; set; }

    public IEnumerable<string> Projects { get; set; } = null!;
}