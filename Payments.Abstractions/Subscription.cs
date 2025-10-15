namespace Staticsoft.Payments.Abstractions;

public record Subscription
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required SubscriptionStatus Status { get; init; }
}
