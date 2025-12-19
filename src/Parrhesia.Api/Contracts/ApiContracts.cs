using System.ComponentModel.DataAnnotations;

namespace Parrhesia.Api.Contracts;

// ==================== Survey Contracts ====================

public record CreateSurveyApiRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime StartDate { get; init; }

    [Required]
    public DateTime EndDate { get; init; }
}

public record UpdateSurveyApiRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? EndDate { get; init; }
}

public record SurveyResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<QuestionResponse> Questions { get; init; } = [];
}

public record SurveyListItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int QuestionCount { get; init; }
}

// ==================== Question Contracts ====================

public record AddQuestionApiRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Text { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Order { get; init; }
}

public record QuestionResponse
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public int Order { get; init; }
    public List<OptionResponse> Options { get; init; } = [];
}

// ==================== Option Contracts ====================

public record AddOptionApiRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Text { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Order { get; init; }
}

public record OptionResponse
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public int Order { get; init; }
}

// ==================== Voting Contracts ====================

public record CastVoteApiRequest
{
    [Required]
    public Guid QuestionId { get; init; }

    [Required]
    public Guid OptionId { get; init; }
}

public record VoteResponse
{
    public Guid BallotId { get; init; }
    public Guid SurveyId { get; init; }
    public Guid QuestionId { get; init; }
    public Guid OptionId { get; init; }
    public DateTime CastedAt { get; init; }
}

// ==================== Results Contracts ====================

public record SurveyResultsResponse
{
    public Guid SurveyId { get; init; }
    public string Title { get; init; } = string.Empty;
    public long TotalVotes { get; init; }
    public List<QuestionResultResponse> Questions { get; init; } = [];
}

public record QuestionResultResponse
{
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public long TotalVotes { get; init; }
    public List<OptionResultResponse> Options { get; init; } = [];
}

public record OptionResultResponse
{
    public Guid OptionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public long Votes { get; init; }
    public double Percentage { get; init; }
}

// ==================== Common Contracts ====================

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}
