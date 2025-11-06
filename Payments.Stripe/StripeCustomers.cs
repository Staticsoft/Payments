using Staticsoft.Payments.Abstractions;

namespace Staticsoft.Payments.Stripe;

public class StripeCustomers(
	StripeCustomerService customerService,
	StripePaymentMethodService paymentService
) : Customers
{
	readonly StripeCustomerService CustomerService = customerService;
	readonly StripePaymentMethodService PaymentService = paymentService;

	public async Task<IReadOnlyCollection<Customer>> List()
	{
		var customers = await CustomerService.ListAsync();
		return customers.Data.Select(MapToCustomer).ToArray();
	}

	public async Task<Customer> Get(string customerId)
	{
		try
		{
			var stripeCustomer = await CustomerService.GetAsync(customerId);

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
		var options = new StripeCustomerCreateOptions
		{
			Email = newCustomer.Email
		};

		var stripeCustomer = await CustomerService.CreateAsync(options);
		return MapToCustomer(stripeCustomer);
	}

	public async Task Delete(string customerId)
	{
		try
		{
			await CustomerService.DeleteAsync(customerId);
		}
		catch (StripeException ex) when (ex.StripeError?.Type == "invalid_request_error")
		{
			throw new Customers.NotFoundException(customerId);
		}
	}

	public async Task SetupPayments(string customerId)
	{
		try
		{
			// Create a test payment method
			var paymentMethodOptions = new StripePaymentMethodCreateOptions
			{
				Type = "card",
				Card = new StripePaymentMethodCardOptions
				{
					Token = "tok_visa" // Stripe test token
				}
			};
			var paymentMethod = await PaymentService.CreateAsync(paymentMethodOptions);

			// Attach payment method to customer
			var attachOptions = new StripePaymentMethodAttachOptions
			{
				Customer = customerId
			};
			await PaymentService.AttachAsync(paymentMethod.Id, attachOptions);

			// Set as default payment method
			var customerUpdateOptions = new StripeCustomerUpdateOptions
			{
				InvoiceSettings = new StripeCustomerInvoiceSettingsOptions
				{
					DefaultPaymentMethod = paymentMethod.Id
				}
			};
			await CustomerService.UpdateAsync(customerId, customerUpdateOptions);
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
