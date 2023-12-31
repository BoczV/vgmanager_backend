using Microsoft.TeamFoundation.DistributedTask.WebApi;
using VGManager.AzureAdapter.Entities;
using VGManager.Entities.VGEntities;
using VGManager.Services.Models.VariableGroups.Requests;

namespace VGManager.Services;

public partial class VariableService
{
    public async Task<AdapterStatus> DeleteVariablesAsync(
        VariableGroupModel variableGroupModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        )
    {
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == AdapterStatus.Success)
        {
            var variableGroupFilter = variableGroupModel.VariableGroupFilter;
            var filteredVariableGroups = FilterWithoutSecrets(filterAsRegex, variableGroupFilter, vgEntity.VariableGroups);
            var finalStatus = await DeleteVariablesAsync(variableGroupModel, filteredVariableGroups, cancellationToken);
            if (finalStatus == AdapterStatus.Success)
            {
                var org = variableGroupModel.Organization;
                var entity = new VGDeleteEntity
                {
                    VariableGroupFilter = variableGroupFilter,
                    Key = variableGroupModel.KeyFilter,
                    Project = _project,
                    Organization = org,
                    User = variableGroupModel.UserName,
                    Date = DateTime.UtcNow
                };

                if (_organizationSettings.Organizations.Contains(org))
                {
                    await _deletionColdRepository.AddEntityAsync(entity, cancellationToken);
                }
            }
            return finalStatus;
        }

        return status;
    }

    private async Task<AdapterStatus> DeleteVariablesAsync(
        VariableGroupModel variableGroupModel,
        IEnumerable<VariableGroup> filteredVariableGroups,
        CancellationToken cancellationToken
        )
    {
        var deletionCounter1 = 0;
        var deletionCounter2 = 0;
        var keyFilter = variableGroupModel.KeyFilter;

        foreach (var filteredVariableGroup in filteredVariableGroups)
        {
            var variableGroupName = filteredVariableGroup.Name;

            var deleteIsNeeded = DeleteVariables(
                filteredVariableGroup,
                keyFilter,
                variableGroupModel.ValueFilter
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

                if (updateStatus == AdapterStatus.Success)
                {
                    deletionCounter2++;
                }
            }
        }
        return deletionCounter1 == deletionCounter2 ? AdapterStatus.Success : AdapterStatus.Unknown;
    }

    private static bool DeleteVariables(VariableGroup filteredVariableGroup, string keyFilter, string? valueCondition)
    {
        var deleteIsNeeded = false;
        var filteredVariables = Filter(filteredVariableGroup.Variables, keyFilter);
        foreach (var filteredVariable in filteredVariables)
        {
            var variableValue = filteredVariable.Value.Value;

            if (valueCondition is not null)
            {
                if (valueCondition.Equals(variableValue))
                {
                    filteredVariableGroup.Variables.Remove(filteredVariable.Key);
                    deleteIsNeeded = true;
                }
            }
            else
            {
                filteredVariableGroup.Variables.Remove(filteredVariable.Key);
                deleteIsNeeded = true;
            }
        }

        return deleteIsNeeded;
    }
}
