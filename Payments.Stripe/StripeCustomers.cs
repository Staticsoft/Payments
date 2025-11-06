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

			if (stripeCustomer.Deleted.HasValue && stripeCustomer.Deleted.Value)
				throw new Customers.NotFoundException(customerId);

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

	public async Task SetupPayments(string customerId)
	{
		StripeConfiguration.ApiKey = Options.ApiKey;

		try
		{
			// Create a test payment method
			var paymentMethodService = new PaymentMethodService();
			var paymentMethodOptions = new PaymentMethodCreateOptions
			{
				Type = "card",
				Card = new PaymentMethodCardOptions
				{
					Token = "tok_visa" // Stripe test token
				}
			};
			var paymentMethod = await paymentMethodService.CreateAsync(paymentMethodOptions);

			// Attach payment method to customer
			var attachOptions = new PaymentMethodAttachOptions
			{
				Customer = customerId
			};
			await paymentMethodService.AttachAsync(paymentMethod.Id, attachOptions);

			// Set as default payment method
			var customerService = new CustomerService();
			var customerUpdateOptions = new CustomerUpdateOptions
			{
				InvoiceSettings = new CustomerInvoiceSettingsOptions
				{
					DefaultPaymentMethod = paymentMethod.Id
				}
			};
			await customerService.UpdateAsync(customerId, customerUpdateOptions);
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
