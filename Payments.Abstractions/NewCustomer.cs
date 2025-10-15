namespace Staticsoft.Payments.Abstractions;

public record NewCustomer
{
    public required string Email { get; init; }
}
