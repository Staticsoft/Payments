using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Memory;

public class MemoryCustomers : Customers
{
    readonly Dictionary<string, Customer> Store = new();

    public Task<IReadOnlyCollection<Customer>> List()
    {
        return Task.FromResult<IReadOnlyCollection<Customer>>(Store.Values.ToArray());
    }

    public Task<Customer> Get(string customerId)
    {
        if (!Store.ContainsKey(customerId))
            throw new Customers.NotFoundException(customerId);

        return Task.FromResult(Store[customerId]);
    }

    public Task<Customer> Create(NewCustomer newCustomer)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Email = newCustomer.Email
        };
        Store[customer.Id] = customer;
        return Task.FromResult(customer);
    }

    public Task Delete(string customerId)
    {
        if (!Store.ContainsKey(customerId))
            throw new Customers.NotFoundException(customerId);

        Store.Remove(customerId);
        return Task.CompletedTask;
    }
}
