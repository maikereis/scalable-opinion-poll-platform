using FluentAssertions;
using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.Tests.Common;

public class AggregateRootTests
{
    private class TestEvent : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public string Message { get; }

        public TestEvent(string message)
        {
            Message = message;
        }
    }

    private class TestAggregate : AggregateRoot
    {
        public string Name { get; private set; }

        public TestAggregate(string name) : base()
        {
            Name = name;
        }

        public TestAggregate(Guid id, string name) : base(id)
        {
            Name = name;
        }

        public void DoSomething(string message)
        {
            AddDomainEvent(new TestEvent(message));
        }
    }

    [Fact]
    public void AggregateRoot_ShouldStartWithNoDomainEvents()
    {
        // Arrange & Act
        var aggregate = new TestAggregate("Test");

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToList()
    {
        // Arrange
        var aggregate = new TestAggregate("Test");

        // Act
        aggregate.DoSomething("Event1");

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddDomainEvent_ShouldAccumulateEvents()
    {
        // Arrange
        var aggregate = new TestAggregate("Test");

        // Act
        aggregate.DoSomething("Event1");
        aggregate.DoSomething("Event2");
        aggregate.DoSomething("Event3");

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate("Test");
        aggregate.DoSomething("Event1");
        aggregate.DoSomething("Event2");

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var aggregate = new TestAggregate("Test");

        // Assert
        aggregate.DomainEvents.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
    }

    [Fact]
    public void AggregateRoot_ShouldInheritEntityBehavior()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate1 = new TestAggregate(id, "Agg1");
        var aggregate2 = new TestAggregate(id, "Agg2");

        // Assert - same ID means equal
        aggregate1.Should().Be(aggregate2);
    }
}