using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGManager.Services.Models.VariableGroups.Results;
public class SecretVariableGroupResultModel: VariableGroupBaseResultModel
{
    public string KeyVaultName { get; set; } = null!;
}
