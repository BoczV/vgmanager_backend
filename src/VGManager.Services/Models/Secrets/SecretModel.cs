using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGManager.Services.Models.Secrets;
public class SecretModel
{
    public string KeyVaultName { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string SecretFilter { get; set; } = null!;
}
