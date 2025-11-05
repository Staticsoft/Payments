# Payments - Desired State

## Overview
A Stripe payment integration library that provides a simplified interface for managing subscriptions and customers. The library focuses on subscription lifecycle management and customer operations, with support for testing various subscription statuses.

## Aggregators

### Billing
Manages payment-related operations including subscriptions and customers.

**Composed Interfaces**:
- `Subscriptions` - Manages subscription lifecycle and status
- `Customers` - Manages customer records

## Interfaces

### Subscriptions
Manages subscription lifecycle including creation and cancellation.

**Methods**:
```csharp
public interface Subscriptions
{
    /// <summary>
    /// Lists all subscriptions for a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A collection of subscriptions belonging to the customer.</returns>
    Task<IReadOnlyCollection<Subscription>> List(string customerId);
    
    /// <summary>
    /// Retrieves a specific subscription by its identifier.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription.</param>
    /// <returns>The subscription with the specified identifier.</returns>
    /// <exception cref="Subscriptions.NotFoundException">Thrown when the subscription does not exist.</exception>
    Task<Subscription> Get(string subscriptionId);
    
    /// <summary>
    /// Creates a new subscription for a customer.
    /// </summary>
    /// <param name="newSubscription">The subscription details.</param>
    /// <returns>The created subscription.</returns>
    Task<Subscription> Create(NewSubscription newSubscription);
    
    /// <summary>
    /// Cancels an active subscription.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription to cancel.</param>
    /// <returns>The canceled subscription.</returns>
    /// <exception cref="Subscriptions.NotFoundException">Thrown when the subscription does not exist.</exception>
    Task<Subscription> Cancel(string subscriptionId);
    
    /// <summary>
    /// Creates a checkout session for a customer to set up payment and start a subscription.
    /// </summary>
    /// <param name="newSession">The checkout session details.</param>
    /// <returns>The URL where the customer should be redirected to complete checkout.</returns>
    Task<string> CreateSession(NewSession newSession);
}
```

**Exceptions**:
- `Subscriptions.NotFoundException` - Thrown when a subscription with the specified ID does not exist

### Customers
Manages customer records including creation, retrieval, and deletion.

**Methods**:
```csharp
public interface Customers
{
    /// <summary>
    /// Lists all customers.
    /// </summary>
    /// <returns>A collection of all customers.</returns>
    Task<IReadOnlyCollection<Customer>> List();
    
    /// <summary>
    /// Retrieves a specific customer by their identifier.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>The customer with the specified identifier.</returns>
    /// <exception cref="Customers.NotFoundException">Thrown when the customer does not exist.</exception>
    Task<Customer> Get(string customerId);
    
    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="newCustomer">The customer details.</param>
    /// <returns>The created customer.</returns>
    Task<Customer> Create(NewCustomer newCustomer);
    
    /// <summary>
    /// Deletes a customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer to delete.</param>
    /// <exception cref="Customers.NotFoundException">Thrown when the customer does not exist.</exception>
    Task Delete(string customerId);
    
    /// <summary>
    /// Sets up payment methods for a customer, enabling active subscriptions.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <exception cref="Customers.NotFoundException">Thrown when the customer does not exist.</exception>
    Task SetupPayments(string customerId);
}
```

**Exceptions**:
- `Customers.NotFoundException` - Thrown when a customer with the specified ID does not exist

## Data Types

### Subscription
Represents a subscription with its current status.

```csharp
public record Subscription
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required SubscriptionStatus Status { get; init; }
}
```

### NewSubscription
Input model for creating or updating subscriptions.

```csharp
public record NewSubscription
{
    public required string CustomerId { get; init; }
    public TimeSpan TrialPeriod { get; init; } = TimeSpan.Zero;
}
```

### SubscriptionStatus
Enumeration of possible subscription statuses.

```csharp
public enum SubscriptionStatus
{
    Active,
    Canceled,
    Incomplete,
    IncompleteExpired,
    Trialing,
    Unpaid
}
```

### Customer
Represents a customer record.

```csharp
public record Customer
{
    public required string Id { get; init; }
    public required string Email { get; init; }
}
```

### NewCustomer
Input model for creating customers.

```csharp
public record NewCustomer
{
    public required string Email { get; init; }
}
```

### NewSession
Input model for creating checkout sessions.

```csharp
public record NewSession
{
    public required string CustomerId { get; init; }
    public TimeSpan TrialPeriod { get; init; } = TimeSpan.Zero;
    public required string SuccessUrl { get; init; }
}
```

## Providers

### Memory Implementation
**Package**: `Payments.Memory`

**Purpose**: In-memory implementation for testing and development

**Characteristics**:
- Stores data in memory using dictionaries
- No persistence between application restarts
- Supports all subscription status transitions
- Ideal for unit testing and development

**Options**: None required

**Environment Variables**: None required

**Registration**:
```csharp
services.UseMemoryBilling();
```

### Stripe Implementation
**Package**: `Payments.Stripe`

**Purpose**: Integration with Stripe payment platform

**Characteristics**:
- Uses Stripe .NET SDK
- Requires API authentication
- Supports all CRUD operations for subscriptions and customers
- Maps Stripe subscription statuses to library enum

**Options**:
```csharp
public class StripeBillingOptions
{
    public required string ApiKey { get; init; }
    public required string PriceId { get; init; }
}
```

**Environment Variables**:
- `StripeApiKey` - Stripe API key for authentication
- `StripePriceId` - Stripe price ID for subscription creation

**Registration**:
```csharp
services
    .UseStripeBilling(_ => new()
    {
        ApiKey = EnvVariable("StripeApiKey"),
        PriceId = EnvVariable("StripePriceId")
    });

static string EnvVariable(string key)
    => Environment.GetEnvironmentVariable(key)
    ?? throw new ArgumentNullException($"Environment variable '{key}' is not set");
```

## Test Scenarios

Test scenarios are ordered by increasing complexity, following the test ordering strategy.

### Level 1: Exception Tests (No State Changes)

#### Scenario: Get non-existing subscription throws NotFoundException
**Given** the system is empty  
**When** I try to get a subscription with ID "non-existing-id"  
**Then** a `Subscriptions.NotFoundException` is thrown

#### Scenario: Get non-existing customer throws NotFoundException
**Given** the system is empty  
**When** I try to get a customer with ID "non-existing-id"  
**Then** a `Customers.NotFoundException` is thrown

#### Scenario: Cancel non-existing subscription throws NotFoundException
**Given** the system is empty  
**When** I try to cancel a subscription with ID "non-existing-id"  
**Then** a `Subscriptions.NotFoundException` is thrown

#### Scenario: Delete non-existing customer throws NotFoundException
**Given** the system is empty  
**When** I try to delete a customer with ID "non-existing-id"  
**Then** a `Customers.NotFoundException` is thrown

### Level 2: Read-Only Operations on Empty System

#### Scenario: List customers returns empty collection when no customers exist
**Given** the system is empty  
**When** I list all customers  
**Then** an empty collection is returned

#### Scenario: List subscriptions returns empty collection when customer has no subscriptions
**Given** I have created a customer  
**And** the customer has no subscriptions  
**When** I list subscriptions for the customer  
**Then** an empty collection is returned

### Level 3: Single Create + Verify

#### Scenario: Create customer and verify it exists
**Given** the system is empty  
**When** I create a customer with email "test@example.com"  
**Then** the customer is created with a unique ID  
**And** I can retrieve the customer by its ID  
**And** the customer has the correct email

#### Scenario: Create subscription and verify it exists
**Given** I have created a customer  
**When** I create a subscription for the customer  
**Then** the subscription is created with a unique ID  
**And** I can retrieve the subscription by its ID  
**And** the subscription has status "Incomplete"  
**And** the subscription is associated with the correct customer

#### Scenario: List customers returns single customer after creation
**Given** the system is empty  
**When** I create a customer  
**Then** listing all customers returns exactly one customer  
**And** the customer matches the created customer

#### Scenario: List subscriptions returns single subscription after creation
**Given** I have created a customer  
**When** I create a subscription for the customer  
**Then** listing subscriptions for the customer returns exactly one subscription  
**And** the subscription matches the created subscription

#### Scenario: Create checkout session without trial and verify valid URL is returned
**Given** I have created a customer  
**When** I create a checkout session for the customer without a trial period  
**Then** a valid URL is returned  
**And** the URL starts with "http://" or "https://"

#### Scenario: Create checkout session with trial and verify valid URL is returned
**Given** I have created a customer  
**When** I create a checkout session for the customer with a 14-day trial period  
**Then** a valid URL is returned  
**And** the URL starts with "http://" or "https://"

### Level 4: Create + Delete Cycle

#### Scenario: Delete created customer
**Given** I have created a customer  
**When** I delete the customer  
**Then** the customer no longer exists  
**And** attempting to get it throws NotFoundException

#### Scenario: List customers returns empty after deleting all customers
**Given** I have created a customer  
**When** I delete the customer  
**Then** listing all customers returns an empty collection

### Level 5: Multiple Items

#### Scenario: Create and list multiple customers
**Given** the system is empty  
**When** I create three customers with emails "user1@example.com", "user2@example.com", "user3@example.com"  
**Then** listing all customers returns exactly three customers  
**And** all three customers are present in the list

#### Scenario: Create multiple subscriptions for same customer
**Given** I have created a customer  
**When** I create three subscriptions for the customer  
**Then** listing subscriptions for the customer returns exactly three subscriptions  
**And** all subscriptions are associated with the correct customer

#### Scenario: Get each created customer individually
**Given** I have created three customers  
**When** I retrieve each customer by its ID  
**Then** each customer is returned with the correct data

#### Scenario: Get each created subscription individually
**Given** I have created a customer with three subscriptions  
**When** I retrieve each subscription by its ID  
**Then** each subscription is returned with the correct data

### Level 6: Update Operations

#### Scenario: Setup payments for customer
**Given** I have created a customer  
**When** I setup payments for the customer  
**Then** the customer is ready to have active subscriptions

#### Scenario: Create active subscription with payment setup
**Given** I have created a customer  
**And** I have setup payments for the customer  
**When** I create a subscription for the customer  
**Then** the subscription is created with status "Active"  
**And** the subscription is associated with the correct customer

#### Scenario: Cancel active subscription
**Given** I have created a customer with payment setup  
**And** I have created an active subscription  
**When** I cancel the subscription  
**Then** retrieving the subscription shows status "Canceled"  
**And** the subscription ID remains unchanged

#### Scenario: Cancel incomplete subscription
**Given** I have created a customer without payment setup  
**And** I have created an incomplete subscription  
**When** I cancel the subscription  
**Then** retrieving the subscription shows status "IncompleteExpired"  
**And** the subscription ID remains unchanged

#### Scenario: Canceled active subscription appears in list with correct status
**Given** I have created a customer with payment setup  
**And** I have created an active subscription  
**When** I cancel the subscription  
**Then** listing subscriptions for the customer shows the subscription with status "Canceled"

#### Scenario: Create subscription with trial period
**Given** I have created a customer with payment setup  
**When** I create a subscription with a 14-day trial period  
**Then** the subscription is created with status "Trialing"

#### Scenario: Cancel trialing subscription
**Given** I have created a customer with payment setup  
**And** I have created a subscription with a trial period  
**When** I cancel the subscription  
**Then** retrieving the subscription shows status "Canceled"  
**And** the subscription ID remains unchanged

#### Scenario: Create subscription with trial period without payment setup
**Given** I have created a customer without payment setup  
**When** I create a subscription with a 14-day trial period  
**Then** the subscription is created with status "Trialing"  
**And** the subscription is associated with the correct customer
