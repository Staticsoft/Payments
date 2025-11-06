using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Stripe;

public class StripeCustomers(StripeBillingOptions options) : Customers
{
	readonly StripeBillingOptions Options = options;

	public async Task<IReadOnlyCollection<Customer>> List()
	{
		StripeConfiguration.ApiKey = Options.ApiKey;
		var service = new StripeCustomerService();

		var customers = await service.ListAsync();
		return customers.Data.Select(MapToCustomer).ToArray();
	}

	public async Task<Customer> Get(string customerId)
	{
		StripeConfiguration.ApiKey = Options.ApiKey;
		var service = new StripeCustomerService();

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

	public async Task<Customer> Create(NewCustomer newCustomer)
	{
		StripeConfiguration.ApiKey = Options.ApiKey;
		var service = new StripeCustomerService();

		var options = new StripeCustomerCreateOptions
		{
			Email = newCustomer.Email
		};

		var stripeCustomer = await service.CreateAsync(options);
		return MapToCustomer(stripeCustomer);
	}

	public async Task Delete(string customerId)
	{
		StripeConfiguration.ApiKey = Options.ApiKey;
		var service = new StripeCustomerService();

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
			var paymentMethodService = new StripePaymentMethodService();
			var paymentMethodOptions = new StripePaymentMethodCreateOptions
			{
				Type = "card",
				Card = new StripePaymentMethodCardOptions
				{
					Token = "tok_visa" // Stripe test token
				}
			};
			var paymentMethod = await paymentMethodService.CreateAsync(paymentMethodOptions);

			// Attach payment method to customer
			var attachOptions = new StripePaymentMethodAttachOptions
			{
				Customer = customerId
			};
			await paymentMethodService.AttachAsync(paymentMethod.Id, attachOptions);

			// Set as default payment method
			var customerService = new StripeCustomerService();
			var customerUpdateOptions = new StripeCustomerUpdateOptions
			{
				InvoiceSettings = new StripeCustomerInvoiceSettingsOptions
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

	static Customer MapToCustomer(StripeCustomer stripeCustomer)
		=> new()
		{
			Id = stripeCustomer.Id,
			Email = stripeCustomer.Email
		};
}
