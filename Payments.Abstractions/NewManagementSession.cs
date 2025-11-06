namespace Staticsoft.Payments.Abstractions;

public record NewManagementSession
{
    public required string CustomerId { get; init; }
    public required string ReturnUrl { get; init; }
}
