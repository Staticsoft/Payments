namespace Staticsoft.Payments.Abstractions;

public record NewSubscription
{
    public required string CustomerId { get; init; }
    public TimeSpan TrialPeriod { get; init; } = TimeSpan.Zero;
}
