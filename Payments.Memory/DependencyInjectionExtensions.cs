using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Memory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseMemoryBilling(this IServiceCollection services)
        => services
            .AddSingleton<Billing>()
            .AddSingleton<MemoryCustomers>()
            .AddSingleton<Customers>(sp => sp.GetRequiredService<MemoryCustomers>())
            .AddSingleton<Subscriptions, MemorySubscriptions>();
}
