using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Memory;

public class MemorySubscriptions : Subscriptions
{
    readonly Dictionary<string, Subscription> Store = new();

    public Task<IReadOnlyCollection<Subscription>> List(string customerId)
    {
        var subscriptions = Store.Values
            .Where(s => s.CustomerId == customerId)
            .ToArray();
        return Task.FromResult<IReadOnlyCollection<Subscription>>(subscriptions);
    }

    public Task<Subscription> Get(string subscriptionId)
    {
        if (!Store.ContainsKey(subscriptionId))
            throw new Subscriptions.NotFoundException(subscriptionId);

        return Task.FromResult(Store[subscriptionId]);
    }

    public Task<Subscription> Create(NewSubscription newSubscription)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = newSubscription.CustomerId,
            Status = SubscriptionStatus.Incomplete
        };
        Store[subscription.Id] = subscription;
        return Task.FromResult(subscription);
    }

    public Task<Subscription> Cancel(string subscriptionId)
    {
        if (!Store.ContainsKey(subscriptionId))
            throw new Subscriptions.NotFoundException(subscriptionId);

        var subscription = Store[subscriptionId];
        var newStatus = subscription.Status == SubscriptionStatus.Incomplete 
            ? SubscriptionStatus.IncompleteExpired 
            : SubscriptionStatus.Canceled;
        var canceled = subscription with { Status = newStatus };
        Store[subscriptionId] = canceled;
        return Task.FromResult(canceled);
    }

    public Task<Subscription> Pause(string subscriptionId)
    {
        if (!Store.ContainsKey(subscriptionId))
            throw new Subscriptions.NotFoundException(subscriptionId);

        var subscription = Store[subscriptionId];
        var newStatus = subscription.Status == SubscriptionStatus.Incomplete 
            ? SubscriptionStatus.Incomplete 
            : SubscriptionStatus.Paused;
        var paused = subscription with { Status = newStatus };
        Store[subscriptionId] = paused;
        return Task.FromResult(paused);
    }

    public Task<Subscription> Resume(string subscriptionId)
    {
        if (!Store.ContainsKey(subscriptionId))
            throw new Subscriptions.NotFoundException(subscriptionId);

        var subscription = Store[subscriptionId];
        var newStatus = subscription.Status == SubscriptionStatus.Incomplete 
            ? SubscriptionStatus.Incomplete 
            : SubscriptionStatus.Active;
        var resumed = subscription with { Status = newStatus };
        Store[subscriptionId] = resumed;
        return Task.FromResult(resumed);
    }
}
