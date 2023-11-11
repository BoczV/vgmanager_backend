namespace VGManager.Entities;

public abstract class OperationEntity
{
    public string Id { get; set; } = null!;
    public string User { get; set; } = null!;
    public string Organization { get; set; } = null!;
    public string Project { get; set; } = null!;
    public string VariableGroupFilter { get; set; } = null!;
    public string Key { get; set; } = null!;
}