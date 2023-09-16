using AutoMapper;
using VGManager.Api.Secret.Request;
using VGManager.Api.Secrets.Response;
using VGManager.Services.Models.Secrets;

namespace VGManager.Api.MapperProfiles;

public class SecretProfile : Profile
{
    public SecretProfile()
    { 
        CreateMap<SecretRequest, SecretModel>();
        CreateMap<SecretCopyRequest, SecretCopyModel>();

        CreateMap<SecretResultModel, SecretGetResponse>();
        CreateMap<SecretResultsModel, SecretsGetResponse>();
        CreateMap<DeletedSecretResultModel, DeletedSecretGetResponse>();
        CreateMap<DeletedSecretResultsModel, DeletedSecretsGetResponse>();
    }
}
