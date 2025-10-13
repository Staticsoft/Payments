namespace Staticsoft.Payments.Abstractions;

public record Customer
{
    public required string Id { get; init; }
    public required string Email { get; init; }
}
