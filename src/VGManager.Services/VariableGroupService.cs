using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.RegularExpressions;
using VGManager.AzureAdapter.Entities;
using VGManager.AzureAdapter.Interfaces;
using VGManager.Services.Interfaces;
using VGManager.Services.Models.VariableGroups.Requests;
using VGManager.Services.Models.VariableGroups.Results;

namespace VGManager.Services;

public class VariableGroupService : IVariableGroupService
{
    private readonly IVariableGroupAdapter _variableGroupConnectionRepository;
    private string _project = null!;
    private readonly ILogger _logger;

    public VariableGroupService(IVariableGroupAdapter variableGroupConnectionRepository, ILogger<VariableGroupService> logger)
    {
        _variableGroupConnectionRepository = variableGroupConnectionRepository;
        _logger = logger;
    }

    public void SetupConnectionRepository(VariableGroupModel variableGroupModel)
    {
        var project = variableGroupModel.Project;
        _variableGroupConnectionRepository.Setup(
            variableGroupModel.Organization,
            project,
            variableGroupModel.PAT
            );
        _project = project;
    }

    public async Task<VariableGroupResultsModel> GetVariableGroupsAsync(
        VariableGroupModel variableGroupModel,
        CancellationToken cancellationToken = default
        )
    {
        var matchedVariableGroups = new List<VariableGroupResultBaseModel>();
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == Status.Success)
        {
            var filteredVariableGroups = variableGroupModel.ContainsSecrets ?
                Filter(vgEntity.VariableGroups, variableGroupModel.VariableGroupFilter) :
                FilterWithoutSecrets(vgEntity.VariableGroups, variableGroupModel.VariableGroupFilter);

            var valueFilter = variableGroupModel.ValueFilter;

            foreach (var filteredVariableGroup in filteredVariableGroups)
            {
                matchedVariableGroups.AddRange(GetVariables(variableGroupModel.KeyFilter, valueFilter, filteredVariableGroup));
            }

            return new()
            {
                Status = status,
                VariableGroups = matchedVariableGroups,
            };
        }
        else
        {
            return new()
            {
                Status = status,
                VariableGroups = new List<VariableGroupResultBaseModel>(),
            };
        }
    }

    public async Task<Status> UpdateVariableGroupsAsync(
        VariableGroupUpdateModel variableGroupUpdateModel,
        CancellationToken cancellationToken = default
        )
    {
        _logger.LogInformation("Variable group name, Key, Old value, New value");
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == Status.Success)
        {
            var filteredVariableGroups = FilterWithoutSecrets(vgEntity.VariableGroups, variableGroupUpdateModel.VariableGroupFilter);
            var updateCounter1 = 0;
            var updateCounter2 = 0;

            foreach (var filteredVariableGroup in filteredVariableGroups)
            {
                var variableGroupName = filteredVariableGroup.Name;
                var updateIsNeeded = UpdateVariables(variableGroupUpdateModel, filteredVariableGroup, variableGroupName);

                if (updateIsNeeded)
                {
                    updateCounter2++;
                    var variableGroupParameters = GetVariableGroupParameters(filteredVariableGroup, variableGroupName);

                    var updateStatus = await _variableGroupConnectionRepository.UpdateAsync(
                        variableGroupParameters,
                        filteredVariableGroup.Id,
                        cancellationToken
                        );

                    if (updateStatus == Status.Success)
                    {
                        updateCounter1++;
                        _logger.LogInformation("{variableGroupName} updated.", variableGroupName);
                    }
                }
            }
            return updateCounter1 == updateCounter2 ? Status.Success : Status.Unknown;
        }
        return status;
    }

    public async Task<Status> AddVariablesAsync(VariableGroupAddModel variableGroupAddModel, CancellationToken cancellationToken = default)
    {
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == Status.Success)
        {
            IEnumerable<VariableGroup> filteredVariableGroups = null!;
            var keyFilter = variableGroupAddModel.KeyFilter;
            var variableGroupFilter = variableGroupAddModel.VariableGroupFilter;
            var key = variableGroupAddModel.Key;
            var value = variableGroupAddModel.Value;

            if (keyFilter is not null)
            {
                var regex = new Regex(keyFilter);

                filteredVariableGroups = FilterWithoutSecrets(vgEntity.VariableGroups, variableGroupFilter)
                    .Select(vg => vg)
                    .Where(vg => vg.Variables.Keys.ToList().FindAll(key => regex.IsMatch(key)).Count > 0);
            }
            else
            {
                filteredVariableGroups = FilterWithoutSecrets(vgEntity.VariableGroups, variableGroupFilter);
            }

            var updateCounter = 0;

            foreach (var filteredVariableGroup in filteredVariableGroups)
            {
                try
                {
                    var success = await AddVariableAsync(key, value, filteredVariableGroup, cancellationToken);

                    if (success)
                    {
                        updateCounter++;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Something went wrong during variable addition. Variable group: {variableGroupName}, Key: {key}",
                        filteredVariableGroup.Name,
                        key
                        );
                }
            }
            return updateCounter == filteredVariableGroups.Count() ? Status.Success : Status.Unknown;
        }
        return status;
    }

    public async Task<Status> DeleteVariableAsync(VariableGroupModel variableGroupModel, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Variable group name, Deleted Key, Deleted Value");
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == Status.Success)
        {
            var filteredVariableGroups = FilterWithoutSecrets(vgEntity.VariableGroups, variableGroupModel.VariableGroupFilter);
            var deletionCounter1 = 0;
            var deletionCounter2 = 0;

            foreach (var filteredVariableGroup in filteredVariableGroups)
            {
                var variableGroupName = filteredVariableGroup.Name;

                var deleteIsNeeded = DeleteVariables(
                    filteredVariableGroup,
                    variableGroupModel.ValueFilter,
                    variableGroupModel.KeyFilter,
                    variableGroupName
                    );

                if (deleteIsNeeded)
                {
                    deletionCounter1++;
                    var variableGroupParameters = GetVariableGroupParameters(filteredVariableGroup, variableGroupName);

                    var updateStatus = await _variableGroupConnectionRepository.UpdateAsync(
                        variableGroupParameters,
                        filteredVariableGroup.Id,
                        cancellationToken
                        );

                    if (updateStatus == Status.Success)
                    {
                        deletionCounter2++;
                    }
                }
            }
            return deletionCounter1 == deletionCounter2 ? Status.Success : Status.Unknown;
        }

        return status;
    }

    private static IEnumerable<VariableGroup> FilterWithoutSecrets(IEnumerable<VariableGroup> variableGroups, string filter)
    {
        var regex = new Regex(filter.ToLower());
        return variableGroups.Where(vg => regex.IsMatch(vg.Name.ToLower()) && vg.Type != "AzureKeyVault").ToList();
    }

    private static IEnumerable<VariableGroup> Filter(IEnumerable<VariableGroup> variableGroups, string filter)
    {
        var regex = new Regex(filter.ToLower());
        return variableGroups.Where(vg => regex.IsMatch(vg.Name.ToLower())).ToList();
    }

    private static IEnumerable<KeyValuePair<string, VariableValue>> Filter(IDictionary<string, VariableValue> variables, string filter)
    {
        var regex = new Regex(filter.ToLower());
        return variables.Where(v => regex.IsMatch(v.Key.ToLower())).ToList();
    }

    private static VariableGroupParameters GetVariableGroupParameters(VariableGroup filteredVariableGroup, string variableGroupName)
    {
        return new VariableGroupParameters()
        {
            Name = variableGroupName,
            Variables = filteredVariableGroup.Variables,
            Description = filteredVariableGroup.Description,
            Type = filteredVariableGroup.Type,
        };
    }

    private List<VariableGroupResultBaseModel> GetVariables(
        string keyFilter,
        string? valueFilter,
        VariableGroup filteredVariableGroup
        )
    {
        var result = new List<VariableGroupResultBaseModel>();
        var filteredVariables = Filter(filteredVariableGroup.Variables, keyFilter);

        foreach (var filteredVariable in filteredVariables)
        {
            var variableValue = filteredVariable.Value.Value ?? string.Empty;
            if (valueFilter is not null)
            {
                var regex = new Regex(valueFilter.ToLower());
                if (regex.IsMatch(variableValue.ToLower()))
                {
                    if (filteredVariableGroup.Type == "AzureKeyVault")
                    {
                        result.Add(new SecretVariableGroupResultModel()
                        {
                            Project = _project ?? string.Empty,
                            SecretVariableGroup = true,
                            VariableGroupName = filteredVariableGroup.Name,
                            VariableGroupKey = filteredVariable.Key,
                            KeyVaultName = string.Empty
                        });
                    }
                    else
                    {
                        result.Add(new VariableGroupResultModel()
                        {
                            Project = _project ?? string.Empty,
                            SecretVariableGroup = false,
                            VariableGroupName = filteredVariableGroup.Name,
                            VariableGroupKey = filteredVariable.Key,
                            VariableGroupValue = variableValue
                        });
                    }
                }
            }
            else
            {
                if (filteredVariableGroup.Type == "AzureKeyVault")
                {
                    result.Add(new SecretVariableGroupResultModel()
                    {
                        Project = _project ?? string.Empty,
                        SecretVariableGroup = true,
                        VariableGroupName = filteredVariableGroup.Name,
                        VariableGroupKey = filteredVariable.Key,
                        KeyVaultName = string.Empty
                    });
                }
                else
                {
                    result.Add(new VariableGroupResultModel()
                    {
                        Project = _project ?? string.Empty,
                        SecretVariableGroup = false,
                        VariableGroupName = filteredVariableGroup.Name,
                        VariableGroupKey = filteredVariable.Key,
                        VariableGroupValue = variableValue
                    });
                }
            }
        }
        return result;
    }

    private bool UpdateVariables(
        VariableGroupUpdateModel variableGroupUpdateModel,
        VariableGroup filteredVariableGroup,
        string variableGroupName
        )
    {
        var filteredVariables = Filter(filteredVariableGroup.Variables, variableGroupUpdateModel.KeyFilter);
        var updateIsNeeded = false;

        foreach (var filteredVariable in filteredVariables)
        {
            var variableValue = filteredVariable.Value.Value;
            var variableKey = filteredVariable.Key;
            var newValue = variableGroupUpdateModel.NewValue;
            var valueFilter = variableGroupUpdateModel.ValueFilter;

            if (valueFilter is not null)
            {
                if (valueFilter.Equals(variableValue))
                {
                    _logger.LogInformation(
                    "{variableGroupName}, {variableKey}, {variableValue}, {newValue}",
                    variableGroupName,
                    variableKey,
                    variableValue,
                    newValue
                    );

                    filteredVariable.Value.Value = newValue;
                    updateIsNeeded = true;
                }
            }
            else
            {
                _logger.LogInformation(
                    "{variableGroupName}, {variableKey}, {variableValue}, {newValue}",
                    variableGroupName,
                    variableKey,
                    variableValue,
                    newValue
                    );

                filteredVariable.Value.Value = newValue;
                updateIsNeeded = true;
            }
        }

        return updateIsNeeded;
    }

    private bool DeleteVariables(VariableGroup filteredVariableGroup, string? valueCondition, string keyFilter, string variableGroupName)
    {
        var deleteIsNeeded = false;
        var filteredVariables = Filter(filteredVariableGroup.Variables, keyFilter);
        foreach (var filteredVariable in filteredVariables)
        {
            var variableValue = filteredVariable.Value.Value;
            var variableKey = filteredVariable.Key;

            if (valueCondition is not null)
            {
                if (valueCondition.Equals(variableValue))
                {
                    _logger.LogInformation("{variableGroupName}, {variableKey}, {variableValue}", variableGroupName, variableKey, variableValue);
                    filteredVariableGroup.Variables.Remove(filteredVariable.Key);
                    deleteIsNeeded = true;
                }
            }
            else
            {
                _logger.LogInformation("{variableGroupName}, {variableKey}, {variableValue}", variableGroupName, variableKey, variableValue);
                filteredVariableGroup.Variables.Remove(filteredVariable.Key);
                deleteIsNeeded = true;
            }
        }

        return deleteIsNeeded;
    }

    private async Task<bool> AddVariableAsync(string key, string value, VariableGroup filteredVariableGroup, CancellationToken cancellationToken)
    {
        var variableGroupName = filteredVariableGroup.Name;
        filteredVariableGroup.Variables.Add(key, value);
        var variableGroupParameters = GetVariableGroupParameters(filteredVariableGroup, variableGroupName);

        var updateStatus = await _variableGroupConnectionRepository.UpdateAsync(
            variableGroupParameters,
            filteredVariableGroup.Id,
            cancellationToken
            );

        if (updateStatus == Status.Success)
        {
            _logger.LogInformation("{variableGroupName}, {key}, {value}", variableGroupName, key, value);
            return true;
        }
        return false;
    }
}
