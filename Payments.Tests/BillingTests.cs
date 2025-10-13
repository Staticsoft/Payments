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
        Assert.Equal(SubscriptionStatus.Incomplete, retrieved.Status);
        Assert.Equal(customer.Id, retrieved.CustomerId);
    }

    [Fact]
    public async Task ReturnsSingleCustomerAfterCreation()
    {
        var createdCustomer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var customers = await SUT.Customers.List();

        var customer = Assert.Single(customers);
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

    [Fact]
    public async Task DeletesCreatedCustomer()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        await SUT.Customers.Delete(customer.Id);

        await Assert.ThrowsAsync<Customers.NotFoundException>(
            () => SUT.Customers.Get(customer.Id)
        );
    }

    [Fact]
    public async Task ReturnsEmptyCollectionAfterDeletingAllCustomers()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        await SUT.Customers.Delete(customer.Id);
        var customers = await SUT.Customers.List();

        Assert.Empty(customers);
    }

    [Fact]
    public async Task CreatesAndListsMultipleCustomers()
    {
        var customer1 = await SUT.Customers.Create(new NewCustomer { Email = "user1@example.com" });
        var customer2 = await SUT.Customers.Create(new NewCustomer { Email = "user2@example.com" });
        var customer3 = await SUT.Customers.Create(new NewCustomer { Email = "user3@example.com" });

        var customers = await SUT.Customers.List();

        Assert.Equal(3, customers.Count);
        Assert.Contains(customers, c => c.Id == customer1.Id);
        Assert.Contains(customers, c => c.Id == customer2.Id);
        Assert.Contains(customers, c => c.Id == customer3.Id);
    }

    [Fact]
    public async Task CreatesMultipleSubscriptionsForSameCustomer()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription1 = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });
        var subscription2 = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });
        var subscription3 = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });

        var subscriptions = await SUT.Subscriptions.List(customer.Id);

        Assert.Equal(3, subscriptions.Count);
        Assert.Contains(subscriptions, s => s.Id == subscription1.Id);
        Assert.Contains(subscriptions, s => s.Id == subscription2.Id);
        Assert.Contains(subscriptions, s => s.Id == subscription3.Id);
    }

    [Fact]
    public async Task GetsEachCreatedCustomerIndividually()
    {
        var customer1 = await SUT.Customers.Create(new NewCustomer { Email = "user1@example.com" });
        var customer2 = await SUT.Customers.Create(new NewCustomer { Email = "user2@example.com" });
        var customer3 = await SUT.Customers.Create(new NewCustomer { Email = "user3@example.com" });

        var retrieved1 = await SUT.Customers.Get(customer1.Id);
        var retrieved2 = await SUT.Customers.Get(customer2.Id);
        var retrieved3 = await SUT.Customers.Get(customer3.Id);

        Assert.Equal(customer1.Id, retrieved1.Id);
        Assert.Equal("user1@example.com", retrieved1.Email);
        Assert.Equal(customer2.Id, retrieved2.Id);
        Assert.Equal("user2@example.com", retrieved2.Email);
        Assert.Equal(customer3.Id, retrieved3.Id);
        Assert.Equal("user3@example.com", retrieved3.Email);
    }

    [Fact]
    public async Task GetsEachCreatedSubscriptionIndividually()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription1 = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });
        var subscription2 = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });
        var subscription3 = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });

        var retrieved1 = await SUT.Subscriptions.Get(subscription1.Id);
        var retrieved2 = await SUT.Subscriptions.Get(subscription2.Id);
        var retrieved3 = await SUT.Subscriptions.Get(subscription3.Id);

        Assert.Equal(subscription1.Id, retrieved1.Id);
        Assert.Equal(customer.Id, retrieved1.CustomerId);
        Assert.Equal(subscription2.Id, retrieved2.Id);
        Assert.Equal(customer.Id, retrieved2.CustomerId);
        Assert.Equal(subscription3.Id, retrieved3.Id);
        Assert.Equal(customer.Id, retrieved3.CustomerId);
    }

    [Fact]
    public async Task CancelsIncompleteSubscription()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });

        var canceled = await SUT.Subscriptions.Cancel(subscription.Id);

        Assert.Equal(subscription.Id, canceled.Id);
        Assert.Equal(SubscriptionStatus.IncompleteExpired, canceled.Status);

        var retrieved = await SUT.Subscriptions.Get(subscription.Id);
        Assert.Equal(SubscriptionStatus.IncompleteExpired, retrieved.Status);
    }

    [Fact]
    public async Task PausesIncompleteSubscription()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });

        var paused = await SUT.Subscriptions.Pause(subscription.Id);

        Assert.Equal(subscription.Id, paused.Id);
        Assert.Equal(SubscriptionStatus.Incomplete, paused.Status);

        var retrieved = await SUT.Subscriptions.Get(subscription.Id);
        Assert.Equal(SubscriptionStatus.Incomplete, retrieved.Status);
    }

    [Fact]
    public async Task ResumesIncompleteSubscription()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });
        await SUT.Subscriptions.Pause(subscription.Id);

        var resumed = await SUT.Subscriptions.Resume(subscription.Id);

        Assert.Equal(subscription.Id, resumed.Id);
        Assert.Equal(SubscriptionStatus.Incomplete, resumed.Status);

        var retrieved = await SUT.Subscriptions.Get(subscription.Id);
        Assert.Equal(SubscriptionStatus.Incomplete, retrieved.Status);
    }

    [Fact]
    public async Task CanceledSubscriptionAppearsInListWithCorrectStatus()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });
        await SUT.Subscriptions.Cancel(subscription.Id);

        var subscriptions = await SUT.Subscriptions.List(customer.Id);

        var canceledSubscription = Assert.Single(subscriptions);
        Assert.Equal(subscription.Id, canceledSubscription.Id);
        Assert.Equal(SubscriptionStatus.IncompleteExpired, canceledSubscription.Status);
    }

    [Fact]
    public async Task MultipleStatusTransitions()
    {
        var customer = await SUT.Customers.Create(new NewCustomer { Email = "test@example.com" });
        var subscription = await SUT.Subscriptions.Create(new NewSubscription { CustomerId = customer.Id });

        var paused = await SUT.Subscriptions.Pause(subscription.Id);
        Assert.Equal(SubscriptionStatus.Incomplete, paused.Status);

        var resumed = await SUT.Subscriptions.Resume(subscription.Id);
        Assert.Equal(SubscriptionStatus.Incomplete, resumed.Status);

        var canceled = await SUT.Subscriptions.Cancel(subscription.Id);
        Assert.Equal(SubscriptionStatus.IncompleteExpired, canceled.Status);

        var final = await SUT.Subscriptions.Get(subscription.Id);
        Assert.Equal(SubscriptionStatus.IncompleteExpired, final.Status);
    }
}
