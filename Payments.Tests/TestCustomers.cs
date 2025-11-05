using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Tests;

public class TestCustomers(
	Customers customers
) : Customers
{
	readonly Customers Customers = customers;
	const string TestDomain = "@example.com";

	public async Task<IReadOnlyCollection<Customer>> List()
	{
		var customers = await Customers.List();
		return customers
			.Where(customer => customer.Email.EndsWith(TestDomain))
			.ToArray();
	}

	public Task<Customer> Get(string customerId)
		=> Customers.Get(customerId);

	public Task<Customer> Create(NewCustomer newCustomer)
	{
		if (!newCustomer.Email.EndsWith(TestDomain))
		{
			throw new ArgumentException($"Only customers from {TestDomain} can be created in tests");
		}
		return Customers.Create(newCustomer);
	}

	public Task Delete(string customerId)
		=> Customers.Delete(customerId);

	public Task SetupPayments(string customerId)
		=> Customers.SetupPayments(customerId);
}
