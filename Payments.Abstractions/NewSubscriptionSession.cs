namespace Staticsoft.Payments.Abstractions;

public record NewSubscriptionSession
{
    public required string CustomerId { get; init; }
    public TimeSpan TrialPeriod { get; init; } = TimeSpan.Zero;
    public required string SuccessUrl { get; init; }
}
