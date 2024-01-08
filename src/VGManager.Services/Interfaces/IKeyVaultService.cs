using VGManager.Models;
using VGManager.Services.Models.Secrets.Requests;
using VGManager.Services.Models.Secrets.Results;

namespace VGManager.Services.Interfaces;

public interface IKeyVaultService
{
    void SetupConnectionRepository(SecretModel secretModel);
    Task<(string?, IEnumerable<string>)> GetKeyVaultsAsync(
        string tenantId,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken = default
        );
    Task<AdapterResponseModel<IEnumerable<SecretResult>>> GetSecretsAsync(string secretFilter, CancellationToken cancellationToken = default);
    AdapterResponseModel<IEnumerable<DeletedSecretResult>> GetDeletedSecrets(string secretFilter, CancellationToken cancellationToken = default);
    Task<AdapterStatus> RecoverSecretAsync(string secretFilter, string userName, CancellationToken cancellationToken = default);
    Task<AdapterStatus> DeleteAsync(string secretFilter, string userName, CancellationToken cancellationToken = default);
    Task<AdapterStatus> CopySecretsAsync(SecretCopyModel secretCopyModel, CancellationToken cancellationToken = default);
}
