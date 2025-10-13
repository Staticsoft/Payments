namespace Staticsoft.Payments.Abstractions;

public record NewSubscription
{
    public required string CustomerId { get; init; }
}
