using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Payments.Memory;

namespace Staticsoft.Payments.Tests;

public class MemoryBillingTests : BillingTests
{
    protected override IServiceCollection Services
        => base.Services.UseMemoryBilling();
}
