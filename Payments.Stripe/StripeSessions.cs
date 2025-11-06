using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Stripe;

public class StripeSessions(StripeBillingOptions options) : Sessions
{
    readonly StripeBillingOptions Options = options;

    public async Task<string> CreateSubscription(NewSubscriptionSession session)
    {
        global::Stripe.StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new global::Stripe.Checkout.SessionService();

        var options = new global::Stripe.Checkout.SessionCreateOptions
        {
            Customer = session.CustomerId,
            SuccessUrl = session.SuccessUrl,
            Mode = "subscription",
            LineItems =
            [
                new()
                {
                    Price = Options.PriceId,
                    Quantity = 1
                }
            ]
        };

        if (session.TrialPeriod > TimeSpan.Zero)
        {
            options.SubscriptionData = new global::Stripe.Checkout.SessionSubscriptionDataOptions
            {
                TrialEnd = DateTime.UtcNow.Add(session.TrialPeriod)
            };
        }

        var stripeSession = await service.CreateAsync(options);
        return stripeSession.Url;
    }

    public async Task<string> CreateManagement(NewManagementSession session)
    {
        global::Stripe.StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new global::Stripe.BillingPortal.SessionService();

        var options = new global::Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = session.CustomerId,
            ReturnUrl = session.ReturnUrl
        };

        var stripeSession = await service.CreateAsync(options);
        return stripeSession.Url;
    }
}
