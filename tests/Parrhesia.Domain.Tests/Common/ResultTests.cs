using FluentAssertions;
using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void GenericSuccess_ShouldCreateSuccessResultWithValue()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void GenericFailure_ShouldCreateFailureResultWithoutValue()
    {
        // Arrange
        var errorMessage = "Value not found";

        // Act
        var result = Result.Failure<int>(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.Value.Should().Be(default(int));
    }
}