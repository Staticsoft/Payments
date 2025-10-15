namespace Staticsoft.Payments.Abstractions;

public class Billing(
    Subscriptions subscriptions,
    Customers customers
)
{
    public Subscriptions Subscriptions { get; } = subscriptions;
    public Customers Customers { get; } = customers;
}
