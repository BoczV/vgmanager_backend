using AutoMapper;
using VGManager.Api.VariableGroup.Response;
using VGManager.Api.VariableGroups.Request;
using VGManager.Api.VariableGroups.Response;
using VGManager.Services.Models.VariableGroups.Requests;
using VGManager.Services.Models.VariableGroups.Results;

namespace VGManager.Api.MapperProfiles;

public class VariableGroupProfile : Profile
{
    public VariableGroupProfile()
    {
        CreateMap<VariableGroupUpdateRequest, VariableGroupUpdateModel>();
        CreateMap<VariableGroupAddRequest, VariableGroupAddModel>();
        CreateMap<VariableGroupRequest, VariableGroupModel>();

        CreateMap<VariableGroupResultBaseModel, VariableGroupGetBaseResponse>();
        CreateMap<VariableGroupResultModel, VariableGroupGetResponse>();
        CreateMap<SecretVariableGroupResultModel, SecretVariableGroupGetResponse>();
        CreateMap<VariableGroupResultsModel, VariableGroupGetResponses>();
    }
}
