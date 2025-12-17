using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class CollectionPeriod : ValueObject
{
    public const int MinDurationDays = 1;
    public const int MaxDurationDays = 30;

    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private CollectionPeriod(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static CollectionPeriod Create(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        var duration = endDate - startDate;

        if (duration.TotalDays < MinDurationDays)
            throw new ArgumentException($"Collection period must be at least {MinDurationDays} day(s)", nameof(endDate));

        if (duration.TotalDays > MaxDurationDays)
            throw new ArgumentException($"Collection period cannot exceed {MaxDurationDays} days", nameof(endDate));

        return new CollectionPeriod(startDate, endDate);
    }

    public bool IsActive(DateTime now) => now >= StartDate && now <= EndDate;

    public bool HasEnded(DateTime now) => now > EndDate;

    public bool HasStarted(DateTime now) => now >= StartDate;

    public TimeSpan Duration => EndDate - StartDate;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
