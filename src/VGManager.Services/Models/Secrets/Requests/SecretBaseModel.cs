namespace VGManager.Services.Models.Secrets.Requests;
public abstract class SecretBaseModel
{
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string UserName { get; set; } = null!;
}
