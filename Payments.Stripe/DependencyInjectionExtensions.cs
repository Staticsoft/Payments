using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Stripe;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection UseStripeBilling(
		this IServiceCollection services,
		Func<IServiceProvider, StripeBillingOptions> options
	)
		=> services
			.AddSingleton(options)
			.AddSingleton(StripeClient)
			.AddStripeService<StripeCustomerService>(client => new(client))
			.AddStripeService<StripeSubscriptionService>(client => new(client))
			.AddStripeService<StripePaymentMethodService>(client => new(client))
			.AddStripeService<StripeCheckoutSessionService>(client => new(client))
			.AddStripeService<StripeBillingPortalSessionService>(client => new(client))
			.AddSingleton<Billing>()
			.AddSingleton<Subscriptions, StripeSubscriptions>()
			.AddSingleton<Customers, StripeCustomers>()
			.AddSingleton<Sessions, StripeSessions>();

	static StripeClient StripeClient(this IServiceProvider provider)
		=> new(provider.GetRequiredService<StripeBillingOptions>().ApiKey);

	static IServiceCollection AddStripeService<T>(this IServiceCollection services, Func<StripeClient, T> factory)
		where T : class
		=> services.AddSingleton(provider => factory(provider.GetRequiredService<StripeClient>()));
}
