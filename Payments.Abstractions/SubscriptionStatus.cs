namespace Staticsoft.Payments.Abstractions;

public enum SubscriptionStatus
{
    Active,
    Canceled,
    Paused,
    Incomplete,
    IncompleteExpired,
    Trialing,
    PastDue,
    Unpaid
}
