using VGManager.AzureAdapter.Entities;

namespace VGManager.Services.Models.VariableGroups.Results;

public class VariableResults
{
    public Status Status { get; set; }
    public List<VariableResult> VariableGroups { get; set; } = null!;
}
