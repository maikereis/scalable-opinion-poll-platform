using FluentAssertions;
using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.Tests.Common;

public class ValueObjectTests
{
    private class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string ZipCode { get; }

        public Address(string street, string city, string zipCode)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;
        }
    }

    [Fact]
    public void TwoValueObjects_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("123 Main St", "Springfield", "12345");

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
        address1.Equals(address2).Should().BeTrue();
    }

    [Fact]
    public void TwoValueObjects_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("456 Oak Ave", "Springfield", "12345");

        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void TwoValueObjects_WithSameValues_ShouldHaveSameHashCode()
    {
        // Arrange
        var address1 = new Address("123 Main St", "Springfield", "12345");
        var address2 = new Address("123 Main St", "Springfield", "12345");

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void ValueObject_ShouldNotBeEqual_ToNull()
    {
        // Arrange
        var address = new Address("123 Main St", "Springfield", "12345");

        // Act & Assert
        address.Equals(null).Should().BeFalse();
        (address == null).Should().BeFalse();
    }

    [Fact]
    public void TwoNullValueObjects_ShouldBeEqual()
    {
        // Arrange
        Address? address1 = null;
        Address? address2 = null;

        // Act & Assert
        (address1 == address2).Should().BeTrue();
    }
}
