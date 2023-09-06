﻿using AutoMapper;
using Microsoft.Extensions.Options;
using VGManager.Repository.Interfaces;
using VGManager.Services.Interfaces;
using VGManager.Services.Models.Projects;

namespace VGManager.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public ProjectService(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<ProjectResultModel> GetProjects(ProjectModel projectModel, CancellationToken cancellationToken = default)
    {
        var url = $"https://dev.azure.com/{projectModel.Organization}";
        var projectEntity = await _projectRepository.GetProjects(url, projectModel.PAT, cancellationToken);
        return _mapper.Map<ProjectResultModel>(projectEntity);
    }
}
