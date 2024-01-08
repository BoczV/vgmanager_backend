using AutoMapper;
using VGManager.AzureAdapter.Interfaces;
using VGManager.Models.Models;
using VGManager.Services.Interfaces;
using VGManager.Services.Models.Common;
using VGManager.Services.Models.Projects;

namespace VGManager.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectAdapter _projectRepository;
    private readonly IMapper _mapper;

    public ProjectService(IProjectAdapter projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<AdapterResponseModel<IEnumerable<ProjectResult>>> GetProjectsAsync(BaseModel projectModel, CancellationToken cancellationToken = default)
    {
        var url = $"https://dev.azure.com/{projectModel.Organization}";
        var projectsEntity = await _projectRepository.GetProjectsAsync(url, projectModel.PAT, cancellationToken);

        return new AdapterResponseModel<IEnumerable<ProjectResult>>()
        {
            Status = projectsEntity.Status,
            Data = _mapper.Map<IEnumerable<ProjectResult>>(projectsEntity.Data)
        };
    }
}
