using SendSmsApi.Models;

namespace SendSmsApi.Services;

public interface ISmsService
{
    Task<SendSmsResponse> SendSmsAsync(
        SendSmsRequest request,
        CancellationToken cancellationToken = default);
}