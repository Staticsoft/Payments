namespace Staticsoft.Payments.Stripe;

public class StripeBillingOptions
{
    public required string ApiKey { get; init; }
    public required string PriceId { get; init; }
}
