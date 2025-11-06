using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Stripe;

public class StripeSessions(StripeBillingOptions options) : Sessions
{
	readonly StripeBillingOptions Options = options;

	public async Task<string> CreateSubscription(NewSubscriptionSession session)
	{
		StripeConfiguration.ApiKey = Options.ApiKey;
		var service = new StripeCheckoutSessionService();

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

		var stripeSession = await service.CreateAsync(options);
		return stripeSession.Url;
	}

	public async Task<string> CreateManagement(NewManagementSession session)
	{
		StripeConfiguration.ApiKey = Options.ApiKey;
		var service = new StripeBillingPortalSessionService();

		var options = new StripeBillingPortalSessionCreateOptions
		{
			Customer = session.CustomerId,
			ReturnUrl = session.ReturnUrl
		};

		var stripeSession = await service.CreateAsync(options);
		return stripeSession.Url;
	}
}
