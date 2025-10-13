using Staticsoft.Payments.Abstractions;
using Staticsoft.Testing;
using Xunit;

namespace Staticsoft.Payments.Tests;

public abstract class BillingTests : TestBase<Billing>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Clean up all customers (which will also clean up their subscriptions)
        var customers = await SUT.Customers.List();
        await Task.WhenAll(customers.Select(customer => SUT.Customers.Delete(customer.Id)));
    }

    public Task DisposeAsync()
        => Task.CompletedTask;

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenGettingNonExistingSubscription()
    {
        await Assert.ThrowsAsync<Subscriptions.NotFoundException>(
            () => SUT.Subscriptions.Get("non-existing-id")
        );
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenGettingNonExistingCustomer()
    {
        await Assert.ThrowsAsync<Customers.NotFoundException>(
            () => SUT.Customers.Get("non-existing-id")
        );
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenCancelingNonExistingSubscription()
    {
        await Assert.ThrowsAsync<Subscriptions.NotFoundException>(
            () => SUT.Subscriptions.Cancel("non-existing-id")
        );
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenDeletingNonExistingCustomer()
    {
        await Assert.ThrowsAsync<Customers.NotFoundException>(
            () => SUT.Customers.Delete("non-existing-id")
        );
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenPausingNonExistingSubscription()
    {
        await Assert.ThrowsAsync<Subscriptions.NotFoundException>(
            () => SUT.Subscriptions.Pause("non-existing-id")
        );
    }

    [Fact]
    public async Task ThrowsNotFoundExceptionWhenResumingNonExistingSubscription()
    {
        await Assert.ThrowsAsync<Subscriptions.NotFoundException>(
            () => SUT.Subscriptions.Resume("non-existing-id")
        );
    }

    [Fact]
    public async Task ReturnsEmptyCollectionWhenNoCustomersExist()
    {
        var customers = await SUT.Customers.List();

        Assert.Empty(customers);
    }

    [Fact]
    public async Task ReturnsEmptyCollectionWhenCustomerHasNoSubscriptions()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscriptions = await SUT.Subscriptions.List(customer.Id);

        Assert.Empty(subscriptions);
    }

    [Fact]
    public async Task CreatesCustomerAndVerifiesItExists()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });

        Assert.NotNull(customer.Id);
        Assert.NotEmpty(customer.Id);

        var retrieved = await SUT.Customers.Get(customer.Id);

        Assert.Equal(customer.Id, retrieved.Id);
        Assert.Equal("test@example.com", retrieved.Email);
    }

    [Fact]
    public async Task CreatesSubscriptionAndVerifiesItExists()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });

        Assert.NotNull(subscription.Id);
        Assert.NotEmpty(subscription.Id);

        var retrieved = await SUT.Subscriptions.Get(subscription.Id);

        Assert.Equal(subscription.Id, retrieved.Id);
        Assert.True(retrieved.Status == SubscriptionStatus.Active || retrieved.Status == SubscriptionStatus.Incomplete);
        Assert.Equal(customer.Id, retrieved.CustomerId);
    }

    [Fact]
    public async Task ReturnsSingleCustomerAfterCreation()
    {
        var createdCustomer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var customers = await SUT.Customers.List();

        var customer = Assert.Single(createdCustomer);
        Assert.Equal(createdCustomer.Id, customer.Id);
    }

    [Fact]
    public async Task ReturnsSingleSubscriptionAfterCreation()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var createdSubscription = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });
        var subscriptions = await SUT.Subscriptions.List(customer.Id);

        var subscription = Assert.Single(subscriptions);
        Assert.Equal(createdSubscription.Id, subscription.Id);
    }
}
