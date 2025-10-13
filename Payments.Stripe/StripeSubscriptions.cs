using Staticsoft.Payments.Abstractions;
using Stripe;

namespace Staticsoft.Payments.Stripe;

public class StripeSubscriptions(StripeBillingOptions options) : Subscriptions
{
    readonly StripeBillingOptions Options = options;

    public async Task<IReadOnlyCollection<Abstractions.Subscription>> List(string customerId)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new SubscriptionService();

        var options = new SubscriptionListOptions
        {
            Customer = customerId
        };

        var subscriptions = await service.ListAsync(options);
        return subscriptions.Data.Select(MapToSubscription).ToArray();
    }

    public async Task<Abstractions.Subscription> Get(string subscriptionId)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new SubscriptionService();

        try
        {
            var stripeSubscription = await service.GetAsync(subscriptionId);
            return MapToSubscription(stripeSubscription);
        }
        catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
        {
            throw new Subscriptions.NotFoundException(subscriptionId);
        }
    }

    public async Task<Abstractions.Subscription> Create(NewSubscription newSubscription)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new SubscriptionService();

        var options = new SubscriptionCreateOptions
        {
            Customer = newSubscription.CustomerId,
            Items = new List<SubscriptionItemOptions>
            {
                new() { Price = Options.PriceId }
            },
            PaymentBehavior = "default_incomplete"
        };

        var stripeSubscription = await service.CreateAsync(options);
        return MapToSubscription(stripeSubscription);
    }

    static Abstractions.Subscription MapToSubscription(global::Stripe.Subscription stripeSubscription)
        => new()
        {
            Id = stripeSubscription.Id,
            CustomerId = stripeSubscription.CustomerId,
            Status = MapStatus(stripeSubscription.Status)
        };

    public async Task<Abstractions.Subscription> Cancel(string subscriptionId)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new SubscriptionService();

        try
        {
            var stripeSubscription = await service.CancelAsync(subscriptionId);
            return MapToSubscription(stripeSubscription);
        }
        catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
        {
            throw new Subscriptions.NotFoundException(subscriptionId);
        }
    }

    public async Task<Abstractions.Subscription> Pause(string subscriptionId)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new SubscriptionService();

        try
        {
            var options = new SubscriptionUpdateOptions
            {
                PauseCollection = new SubscriptionPauseCollectionOptions
                {
                    Behavior = "void"
                }
            };
            var stripeSubscription = await service.UpdateAsync(subscriptionId, options);
            return MapToSubscription(stripeSubscription);
        }
        catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
        {
            throw new Subscriptions.NotFoundException(subscriptionId);
        }
    }

    public async Task<Abstractions.Subscription> Resume(string subscriptionId)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new SubscriptionService();

        try
        {
            var options = new SubscriptionUpdateOptions
            {
                PauseCollection = null
            };
            var stripeSubscription = await service.UpdateAsync(subscriptionId, options);
            return MapToSubscription(stripeSubscription);
        }
        catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
        {
            throw new Subscriptions.NotFoundException(subscriptionId);
        }
    }

    static SubscriptionStatus MapStatus(string status)
        => status switch
        {
            "active" => SubscriptionStatus.Active,
            "canceled" => SubscriptionStatus.Canceled,
            "paused" => SubscriptionStatus.Paused,
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
            "trialing" => SubscriptionStatus.Trialing,
            "past_due" => SubscriptionStatus.PastDue,
            "unpaid" => SubscriptionStatus.Unpaid,
            _ => throw new ArgumentException($"Unknown subscription status: {status}")
        };
}
