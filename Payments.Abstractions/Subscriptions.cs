namespace Staticsoft.Payments.Abstractions;

public interface Subscriptions
{
    Task<IReadOnlyCollection<Subscription>> List(string customerId);
    Task<Subscription> Get(string subscriptionId);
    Task<Subscription> Create(NewSubscription newSubscription);
    Task<Subscription> Cancel(string subscriptionId);
    Task<Subscription> Pause(string subscriptionId);
    Task<Subscription> Resume(string subscriptionId);

    public class NotFoundException(string subscriptionId)
        : Exception(ToMessage(subscriptionId))
    {
        static string ToMessage(string subscriptionId)
            => $"Subscription with ID '{subscriptionId}' not found.";
    }
}
