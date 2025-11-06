namespace Staticsoft.Payments.Abstractions;

public class Billing(
    Subscriptions subscriptions,
    Customers customers,
    Sessions sessions
)
{
    public Subscriptions Subscriptions { get; } = subscriptions;
    public Customers Customers { get; } = customers;
    public Sessions Sessions { get; } = sessions;
}
