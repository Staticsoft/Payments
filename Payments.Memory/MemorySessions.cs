using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Memory;

public class MemorySessions : Sessions
{
    public Task<string> CreateSubscription(NewSubscriptionSession session)
    {
        var sessionId = Guid.NewGuid().ToString();
        var url = $"https://checkout.example.com/session/{sessionId}";
        return Task.FromResult(url);
    }

    public Task<string> CreateManagement(NewManagementSession session)
    {
        var sessionId = Guid.NewGuid().ToString();
        var url = $"https://billing.example.com/portal/{sessionId}";
        return Task.FromResult(url);
    }
}
