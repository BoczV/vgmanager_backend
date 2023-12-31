using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.RegularExpressions;
using VGManager.AzureAdapter.Entities;
using VGManager.Entities.VGEntities;
using VGManager.Services.Models.VariableGroups.Requests;

namespace VGManager.Services;

public partial class VariableService
{
    public async Task<AdapterStatus> UpdateVariableGroupsAsync(
        VariableGroupUpdateModel variableGroupUpdateModel,
        bool filterAsRegex,
        CancellationToken cancellationToken = default
        )
    {
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == AdapterStatus.Success)
        {
            var variableGroupFilter = variableGroupUpdateModel.VariableGroupFilter;
            var filteredVariableGroups = FilterWithoutSecrets(filterAsRegex, variableGroupFilter, vgEntity.VariableGroups);
            var keyFilter = variableGroupUpdateModel.KeyFilter;
            var valueFilter = variableGroupUpdateModel.ValueFilter;
            var newValue = variableGroupUpdateModel.NewValue;
            Regex? valueRegex = null;

            if (valueFilter is not null)
            {
                try
                {
                    valueRegex = new Regex(valueFilter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
                }
                catch (RegexParseException ex)
                {
                    _logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", valueFilter);
                }
            }

            var finalStatus = await UpdateVariableGroupsAsync(newValue, filteredVariableGroups, keyFilter, valueRegex, cancellationToken);

            if (finalStatus == AdapterStatus.Success)
            {
                var org = variableGroupUpdateModel.Organization;

                var entity = new VGUpdateEntity
                {
                    VariableGroupFilter = variableGroupFilter,
                    Key = keyFilter,
                    Project = _project,
                    Organization = org,
                    User = variableGroupUpdateModel.UserName,
                    Date = DateTime.UtcNow,
                    NewValue = variableGroupUpdateModel.NewValue
                };

                if (_organizationSettings.Organizations.Contains(org))
                {
                    await _editionColdRepository.AddEntityAsync(entity, cancellationToken);
                }
            }

            return finalStatus;
        }
        return status;
    }

    private async Task<AdapterStatus> UpdateVariableGroupsAsync(
        string newValue,
        IEnumerable<VariableGroup> filteredVariableGroups,
        string keyFilter,
        Regex? valueRegex,
        CancellationToken cancellationToken
        )
    {
        var updateCounter1 = 0;
        var updateCounter2 = 0;

        foreach (var filteredVariableGroup in filteredVariableGroups)
        {
            var variableGroupName = filteredVariableGroup.Name;
            var updateIsNeeded = UpdateVariables(newValue, keyFilter, valueRegex, filteredVariableGroup);

            if (updateIsNeeded)
            {
                updateCounter2++;
                var variableGroupParameters = GetVariableGroupParameters(filteredVariableGroup, variableGroupName);

                var updateStatus = await _variableGroupConnectionRepository.UpdateAsync(
                    variableGroupParameters,
                    filteredVariableGroup.Id,
                    cancellationToken
                    );

                if (updateStatus == AdapterStatus.Success)
                {
                    updateCounter1++;
                    _logger.LogDebug("{variableGroupName} updated.", variableGroupName);
                }
            }
        }
        return updateCounter1 == updateCounter2 ? AdapterStatus.Success : AdapterStatus.Unknown;
    }

    private static bool UpdateVariables(
        string newValue,
        string keyFilter,
        Regex? regex,
        VariableGroup filteredVariableGroup
        )
    {
        var filteredVariables = Filter(filteredVariableGroup.Variables, keyFilter);
        var updateIsNeeded = false;

        foreach (var filteredVariable in filteredVariables)
        {
            updateIsNeeded = IsUpdateNeeded(filteredVariable, regex, newValue);
        }

        return updateIsNeeded;
    }

    private static bool IsUpdateNeeded(KeyValuePair<string, VariableValue> filteredVariable, Regex? regex, string newValue)
    {
        var variableValue = filteredVariable.Value.Value;

        if (regex is not null)
        {
            if (regex.IsMatch(variableValue.ToLower()))
            {
                filteredVariable.Value.Value = newValue;
                return true;
            }
        }
        else
        {
            filteredVariable.Value.Value = newValue;
            return true;
        }

        return false;
    }
}
