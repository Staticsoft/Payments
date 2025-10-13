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
            .AddSingleton<Billing>()
            .AddSingleton<Subscriptions, StripeSubscriptions>()
            .AddSingleton<Customers, StripeCustomers>();
}
