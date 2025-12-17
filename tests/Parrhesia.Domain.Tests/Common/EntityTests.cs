using FluentAssertions;
using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.Tests.Common;

public class EntityTests
{
    private class TestEntity : Entity
    {
        public string Name { get; set; }

        public TestEntity(string name) : base()
        {
            Name = name;
        }

        public TestEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

    [Fact]
    public void Entity_ShouldGenerateId_WhenCreatedWithDefaultConstructor()
    {
        // Arrange & Act
        var entity = new TestEntity("Test");

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Entity_ShouldUseProvidedId_WhenCreatedWithIdConstructor()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestEntity(id, "Test");

        // Assert
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Entity_ShouldThrowException_WhenCreatedWithEmptyId()
    {
        // Arrange & Act
        Action act = () => new TestEntity(Guid.Empty, "Test");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Entity ID cannot be empty*");
    }

    [Fact]
    public void TwoEntities_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Entity1");
        var entity2 = new TestEntity(id, "Entity2");

        // Act & Assert
        entity1.Should().Be(entity2);
        (entity1 == entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeTrue();
    }

    [Fact]
    public void TwoEntities_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity("Entity1");
        var entity2 = new TestEntity("Entity2");

        // Act & Assert
        entity1.Should().NotBe(entity2);
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void TwoEntities_WithSameId_ShouldHaveSameHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Entity1");
        var entity2 = new TestEntity(id, "Entity2");

        // Act & Assert
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void Entity_ShouldNotBeEqual_ToNull()
    {
        // Arrange
        var entity = new TestEntity("Test");

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
        (entity == null).Should().BeFalse();
    }

    [Fact]
    public void TwoNullEntities_ShouldBeEqual()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act & Assert
        (entity1 == entity2).Should().BeTrue();
    }
}
