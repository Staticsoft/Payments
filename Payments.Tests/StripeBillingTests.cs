using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Payments.Stripe;

namespace Staticsoft.Payments.Tests;

public class StripeBillingTests : BillingTests
{
    protected override IServiceCollection Services
        => base.Services
            .UseStripeBilling(_ => new()
            {
                ApiKey = GetApiKey(),
                PriceId = GetPriceId()
            });

    static string GetApiKey()
        => EnvVariable("StripeApiKey");

    static string GetPriceId()
        => EnvVariable("StripePriceId");

    static string EnvVariable(string name)
        => Environment.GetEnvironmentVariable(name)
        ?? throw new ArgumentNullException($"Environment variable {name} is not set");
}
