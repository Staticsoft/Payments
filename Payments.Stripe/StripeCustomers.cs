using Staticsoft.Payments.Abstractions;
using Stripe;

namespace Staticsoft.Payments.Stripe;

public class StripeCustomers(StripeBillingOptions options) : Customers
{
    readonly StripeBillingOptions Options = options;

    public async Task<IReadOnlyCollection<Abstractions.Customer>> List()
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new CustomerService();

        var customers = await service.ListAsync();
        return customers.Data.Select(MapToCustomer).ToArray();
    }

    public async Task<Abstractions.Customer> Get(string customerId)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new CustomerService();

        try
        {
            var stripeCustomer = await service.GetAsync(customerId);
            return MapToCustomer(stripeCustomer);
        }
        catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
        {
            throw new Customers.NotFoundException(customerId);
        }
    }

    public async Task<Abstractions.Customer> Create(NewCustomer newCustomer)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new CustomerService();

        var options = new CustomerCreateOptions
        {
            Email = newCustomer.Email
        };

        var stripeCustomer = await service.CreateAsync(options);
        return MapToCustomer(stripeCustomer);
    }

    public async Task Delete(string customerId)
    {
        StripeConfiguration.ApiKey = Options.ApiKey;
        var service = new CustomerService();

        try
        {
            await service.DeleteAsync(customerId);
        }
        catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
        {
            throw new Customers.NotFoundException(customerId);
        }
    }

    static Abstractions.Customer MapToCustomer(global::Stripe.Customer stripeCustomer)
        => new()
        {
            Id = stripeCustomer.Id,
            Email = stripeCustomer.Email
        };
}
