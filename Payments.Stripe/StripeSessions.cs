using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Stripe;

public class StripeSessions(
	StripeCheckoutSessionService checkoutSession,
	StripeBillingPortalSessionService billingSession,
	StripeBillingOptions options
) : Sessions
{
	readonly StripeBillingOptions Options = options;
	readonly StripeCheckoutSessionService CheckoutSession = checkoutSession;
	readonly StripeBillingPortalSessionService BillingSession = billingSession;

	public async Task<string> CreateSubscription(NewSubscriptionSession session)
	{
		var options = new StripeCheckoutSessionCreateOptions
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
			options.SubscriptionData = new StripeCheckoutSessionSubscriptionDataOptions
			{
				TrialEnd = DateTime.UtcNow.Add(session.TrialPeriod)
			};
		}

		var stripeSession = await CheckoutSession.CreateAsync(options);
		return stripeSession.Url;
	}

	public async Task<string> CreateManagement(NewManagementSession session)
	{
		var options = new StripeBillingPortalSessionCreateOptions
		{
			Customer = session.CustomerId,
			ReturnUrl = session.ReturnUrl
		};

		var stripeSession = await BillingSession.CreateAsync(options);
		return stripeSession.Url;
	}
}
