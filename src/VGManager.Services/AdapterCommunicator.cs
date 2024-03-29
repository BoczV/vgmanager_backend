using System.Text.Json;
using VGManager.Adapter.Client.Interfaces;
using VGManager.Services.Interfaces;

namespace VGManager.Services;

public class AdapterCommunicator(IVGManagerAdapterClientService clientService) : IAdapterCommunicator
{
    public async Task<(bool, string)> CommunicateWithAdapterAsync<T>(
        T request,
        string commandTypes,
        CancellationToken cancellationToken = default
        )
    {
        (bool isSuccess, string response) = await clientService.SendAndReceiveMessageAsync(
            commandTypes,
            JsonSerializer.Serialize(request),
            cancellationToken);
        return (isSuccess, response);
    }
}
