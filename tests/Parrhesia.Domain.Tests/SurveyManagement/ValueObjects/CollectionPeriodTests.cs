using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.ValueObjects;

public class CollectionPeriodTests
{
    [Fact]
    public void Create_WithValidDates_ShouldSucceed()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(7);

        var period = CollectionPeriod.Create(start, end);

        period.StartDate.Should().Be(start);
        period.EndDate.Should().Be(end);
    }

    [Fact]
    public void Create_WithEndBeforeStart_ShouldThrow()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(-1);

        Action act = () => CollectionPeriod.Create(start, end);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*End date must be after start date*");
    }

    [Fact]
    public void Create_WithSameStartAndEnd_ShouldThrow()
    {
        var date = DateTime.UtcNow;

        Action act = () => CollectionPeriod.Create(date, date);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*End date must be after start date*");
    }

    [Fact]
    public void Create_WithDurationLessThanMinimum_ShouldThrow()
    {
        var start = DateTime.UtcNow;
        var end = start.AddHours(12); // Less than 1 day

        Action act = () => CollectionPeriod.Create(start, end);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*at least {CollectionPeriod.MinDurationDays} day*");
    }

    [Fact]
    public void Create_WithDurationExceedingMaximum_ShouldThrow()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(CollectionPeriod.MaxDurationDays + 1);

        Action act = () => CollectionPeriod.Create(start, end);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*cannot exceed {CollectionPeriod.MaxDurationDays} days*");
    }

    [Fact]
    public void Create_AtMinimumDuration_ShouldSucceed()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(CollectionPeriod.MinDurationDays);

        var period = CollectionPeriod.Create(start, end);

        period.Duration.TotalDays.Should().Be(CollectionPeriod.MinDurationDays);
    }

    [Fact]
    public void Create_AtMaximumDuration_ShouldSucceed()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(CollectionPeriod.MaxDurationDays);

        var period = CollectionPeriod.Create(start, end);

        period.Duration.TotalDays.Should().Be(CollectionPeriod.MaxDurationDays);
    }

    [Fact]
    public void IsActive_BeforeStart_ShouldReturnFalse()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddDays(7);
        var period = CollectionPeriod.Create(start, end);

        period.IsActive(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsActive_DuringPeriod_ShouldReturnTrue()
    {
        var start = DateTime.UtcNow.AddDays(-1);
        var end = start.AddDays(7);
        var period = CollectionPeriod.Create(start, end);

        period.IsActive(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsActive_AfterEnd_ShouldReturnFalse()
    {
        var start = DateTime.UtcNow.AddDays(-10);
        var end = start.AddDays(7);
        var period = CollectionPeriod.Create(start, end);

        period.IsActive(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void HasEnded_BeforeEnd_ShouldReturnFalse()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(7);
        var period = CollectionPeriod.Create(start, end);

        period.HasEnded(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void HasEnded_AfterEnd_ShouldReturnTrue()
    {
        var start = DateTime.UtcNow.AddDays(-10);
        var end = start.AddDays(7);
        var period = CollectionPeriod.Create(start, end);

        period.HasEnded(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void TwoPeriods_WithSameDates_ShouldBeEqual()
    {
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 1, 15);

        var period1 = CollectionPeriod.Create(start, end);
        var period2 = CollectionPeriod.Create(start, end);

        period1.Should().Be(period2);
    }
}
