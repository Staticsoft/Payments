namespace Staticsoft.Payments.Abstractions;

public interface Customers
{
    Task<IReadOnlyCollection<Customer>> List();
    Task<Customer> Get(string customerId);
    Task<Customer> Create(NewCustomer newCustomer);
    Task Delete(string customerId);

    public class NotFoundException(string customerId)
        : Exception(ToMessage(customerId))
    {
        static string ToMessage(string customerId)
            => $"Customer with ID '{customerId}' not found.";
    }
}
