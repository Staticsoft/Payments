using Staticsoft.Payments.Abstractions;
using Stripe;
using Stripe.Checkout;

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
			Customer = customerId,
			Status = "all"
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
		var customerService = new CustomerService();

		// Check if customer has a default payment method
		var customer = await customerService.GetAsync(newSubscription.CustomerId);
		var hasPaymentMethod = customer.InvoiceSettings?.DefaultPaymentMethodId != null;

		var options = new SubscriptionCreateOptions
		{
			Customer = newSubscription.CustomerId,
			Items = new List<SubscriptionItemOptions>
			{
				new() { Price = Options.PriceId }
			},
			CollectionMethod = "charge_automatically"
		};

		if (newSubscription.TrialPeriod > TimeSpan.Zero)
		{
			options.TrialEnd = DateTime.UtcNow.Add(newSubscription.TrialPeriod);
		}

		if (!hasPaymentMethod)
		{
			options.PaymentBehavior = "default_incomplete";
		}
		else
		{
			options.PaymentBehavior = "allow_incomplete";
		}

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

	static SubscriptionStatus MapStatus(string status)
		=> status switch
		{
			"active" => SubscriptionStatus.Active,
			"canceled" => SubscriptionStatus.Canceled,
			"incomplete" => SubscriptionStatus.Incomplete,
			"incomplete_expired" => SubscriptionStatus.IncompleteExpired,
			"trialing" => SubscriptionStatus.Trialing,
			"past_due" => SubscriptionStatus.Unpaid,
			"unpaid" => SubscriptionStatus.Unpaid,
			_ => throw new ArgumentException($"Unknown subscription status: {status}")
		};

	public async Task<string> CreateSession(NewSession newSession)
	{
		StripeConfiguration.ApiKey = Options.ApiKey;
		var service = new SessionService();

		var options = new SessionCreateOptions
		{
			Customer = newSession.CustomerId,
			SuccessUrl = newSession.SuccessUrl,
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

		if (newSession.TrialPeriod > TimeSpan.Zero)
		{
			options.SubscriptionData = new SessionSubscriptionDataOptions
			{
				TrialEnd = DateTime.UtcNow.Add(newSession.TrialPeriod)
			};
		}

		var session = await service.CreateAsync(options);
		return session.Url;
	}
}
