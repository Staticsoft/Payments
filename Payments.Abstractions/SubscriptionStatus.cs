namespace Staticsoft.Payments.Abstractions;

public enum SubscriptionStatus
{
    Active,
    Canceled,
    Incomplete,
    IncompleteExpired,
    Trialing,
    PastDue,
    Unpaid
}
