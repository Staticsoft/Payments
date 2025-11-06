namespace Staticsoft.Payments.Abstractions;

public interface Sessions
{
    /// <summary>
    /// Creates a checkout session for a customer to subscribe to a new subscription.
    /// </summary>
    /// <param name="session">The checkout session details.</param>
    /// <returns>The URL where the customer should be redirected to complete checkout.</returns>
    Task<string> CreateSubscription(NewSubscriptionSession session);

    /// <summary>
    /// Creates a portal session for a customer to manage their existing subscriptions.
    /// </summary>
    /// <param name="session">The portal session details.</param>
    /// <returns>The URL where the customer should be redirected to manage subscriptions.</returns>
    Task<string> CreateManagement(NewManagementSession session);
}
