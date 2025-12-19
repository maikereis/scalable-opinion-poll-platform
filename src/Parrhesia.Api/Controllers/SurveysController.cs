using Microsoft.AspNetCore.Mvc;
using Parrhesia.Api.Contracts;
using Parrhesia.Api.Middleware;
using Parrhesia.Application.Common;
using Parrhesia.Application.SurveyManagement.ActivateSurvey;
using Parrhesia.Application.SurveyManagement.AddOption;
using Parrhesia.Application.SurveyManagement.AddQuestion;
using Parrhesia.Application.SurveyManagement.CreateSurvey;
using Parrhesia.Application.SurveyManagement.GetSurvey;
using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Api.Controllers;

[ApiController]
[Route("api/v1/surveys")]
[Produces("application/json")]
public class SurveysController : ControllerBase
{
    private readonly IUseCase<CreateSurveyRequest, Result<CreateSurveyResponse>> _createSurveyUseCase;
    private readonly IUseCase<GetSurveyRequest, Result<GetSurveyResponse>> _getSurveyUseCase;
    private readonly IUseCase<AddQuestionRequest, Result<AddQuestionResponse>> _addQuestionUseCase;
    private readonly IUseCase<AddOptionRequest, Result<AddOptionResponse>> _addOptionUseCase;
    private readonly IUseCase<ActivateSurveyRequest, Result<ActivateSurveyResponse>> _activateSurveyUseCase;
    private readonly ISurveyRepository _surveyRepository;
    private readonly ILogger<SurveysController> _logger;

    public SurveysController(
        IUseCase<CreateSurveyRequest, Result<CreateSurveyResponse>> createSurveyUseCase,
        IUseCase<GetSurveyRequest, Result<GetSurveyResponse>> getSurveyUseCase,
        IUseCase<AddQuestionRequest, Result<AddQuestionResponse>> addQuestionUseCase,
        IUseCase<AddOptionRequest, Result<AddOptionResponse>> addOptionUseCase,
        IUseCase<ActivateSurveyRequest, Result<ActivateSurveyResponse>> activateSurveyUseCase,
        ISurveyRepository surveyRepository,
        ILogger<SurveysController> logger)
    {
        _createSurveyUseCase = createSurveyUseCase;
        _getSurveyUseCase = getSurveyUseCase;
        _addQuestionUseCase = addQuestionUseCase;
        _addOptionUseCase = addOptionUseCase;
        _activateSurveyUseCase = activateSurveyUseCase;
        _surveyRepository = surveyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all surveys with pagination and filtering (Admin).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<SurveyListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSurveys(
        [FromQuery] string? status = "all",
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string order = "desc",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GetSurveys called with status={Status}, limit={Limit}, offset={Offset}", 
                status, limit, offset);

            // Validate parameters
            limit = Math.Min(Math.Max(1, limit), 50);
            offset = Math.Max(0, offset);

            // Get surveys based on status filter
            IEnumerable<Domain.SurveyManagement.Aggregates.Survey> surveys;
            
            if (status?.ToLower() == "active")
            {
                surveys = await _surveyRepository.GetActiveAsync(cancellationToken);
            }
            else if (status?.ToLower() == "draft")
            {
                var allSurveys = await _surveyRepository.GetAllAsync(cancellationToken);
                surveys = allSurveys.Where(s => s.Status.IsDraft);
            }
            else if (status?.ToLower() == "closed")
            {
                var allSurveys = await _surveyRepository.GetAllAsync(cancellationToken);
                surveys = allSurveys.Where(s => s.Status.IsClosed);
            }
            else
            {
                // Get all surveys
                surveys = await _surveyRepository.GetAllAsync(cancellationToken);
            }

            _logger.LogInformation("Found {Count} surveys", surveys.Count());

        // Apply sorting
        surveys = sortBy.ToLower() switch
        {
            "title" => order.ToLower() == "asc" 
                ? surveys.OrderBy(s => s.Title.Value)
                : surveys.OrderByDescending(s => s.Title.Value),
            "startdate" => order.ToLower() == "asc"
                ? surveys.OrderBy(s => s.CollectionPeriod.StartDate)
                : surveys.OrderByDescending(s => s.CollectionPeriod.StartDate),
            _ => order.ToLower() == "asc"
                ? surveys.OrderBy(s => s.CreatedAt)
                : surveys.OrderByDescending(s => s.CreatedAt)
        };

        var totalCount = surveys.Count();
        
        // Apply pagination
        var paginatedSurveys = surveys
            .Skip(offset)
            .Take(limit)
            .ToList();

        var items = paginatedSurveys.Select(s => new SurveyListItemResponse
        {
            Id = s.SurveyId.Value,
            Title = s.Title.Value,
            Status = s.Status.Value,
            StartDate = s.CollectionPeriod.StartDate,
            EndDate = s.CollectionPeriod.EndDate,
            QuestionCount = s.Questions.Count
        }).ToList();

        var response = new PaginatedResponse<SurveyListItemResponse>
        {
            Data = items,
            Pagination = new PaginationInfo
            {
                Total = totalCount,
                Limit = limit,
                Offset = offset,
                HasMore = offset + limit < totalCount
            }
        };

                    return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSurveys");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Creates a new survey in draft status.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SurveyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSurvey(
        [FromBody] CreateSurveyApiRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createSurveyUseCase.ExecuteAsync(
            new CreateSurveyRequest(
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new ErrorResponse { Message = result.Error });

        var response = new SurveyResponse
        {
            Id = result.Value.SurveyId,
            Title = result.Value.Title,
            Status = result.Value.Status,
            StartDate = result.Value.StartDate,
            EndDate = result.Value.EndDate,
            CreatedAt = result.Value.CreatedAt
        };

        return CreatedAtAction(nameof(GetSurvey), new { id = response.Id }, response);
    }

    /// <summary>
    /// Gets a survey by ID with all questions and options.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SurveyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSurvey(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getSurveyUseCase.ExecuteAsync(
            new GetSurveyRequest(id),
            cancellationToken);

        if (result.IsFailure)
            return NotFound();

        var survey = result.Value;
        var response = new SurveyResponse
        {
            Id = survey.SurveyId,
            Title = survey.Title,
            Description = survey.Description,
            Status = survey.Status,
            StartDate = survey.StartDate,
            EndDate = survey.EndDate,
            CreatedAt = survey.CreatedAt,
            UpdatedAt = survey.UpdatedAt,
            Questions = survey.Questions.Select(q => new QuestionResponse
            {
                Id = q.QuestionId,
                Text = q.Text,
                Order = q.Order,
                Options = q.Options.Select(o => new OptionResponse
                {
                    Id = o.OptionId,
                    Text = o.Text,
                    Order = o.Order
                }).ToList()
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets all active surveys with pagination.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(PaginatedResponse<SurveyListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSurveys(
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string order = "desc",
        CancellationToken cancellationToken = default)
    {
        // Validate and constrain parameters
        limit = Math.Min(Math.Max(1, limit), 50);
        offset = Math.Max(0, offset);

        var surveys = await _surveyRepository.GetActiveAsync(cancellationToken);

        // Apply sorting
        IEnumerable<Domain.SurveyManagement.Aggregates.Survey> sortedSurveys = sortBy.ToLower() switch
        {
            "title" => order.ToLower() == "asc"
                ? surveys.OrderBy(s => s.Title.Value)
                : surveys.OrderByDescending(s => s.Title.Value),
            "startdate" => order.ToLower() == "asc"
                ? surveys.OrderBy(s => s.CollectionPeriod.StartDate)
                : surveys.OrderByDescending(s => s.CollectionPeriod.StartDate),
            _ => order.ToLower() == "asc"
                ? surveys.OrderBy(s => s.CreatedAt)
                : surveys.OrderByDescending(s => s.CreatedAt)
        };

        var totalCount = sortedSurveys.Count();

        // Apply pagination
        var paginatedSurveys = sortedSurveys
            .Skip(offset)
            .Take(limit)
            .ToList();

        var items = paginatedSurveys.Select(s => new SurveyListItemResponse
        {
            Id = s.SurveyId.Value,
            Title = s.Title.Value,
            Status = s.Status.Value,
            StartDate = s.CollectionPeriod.StartDate,
            EndDate = s.CollectionPeriod.EndDate,
            QuestionCount = s.Questions.Count
        }).ToList();

        var response = new PaginatedResponse<SurveyListItemResponse>
        {
            Data = items,
            Pagination = new PaginationInfo
            {
                Total = totalCount,
                Limit = limit,
                Offset = offset,
                HasMore = offset + limit < totalCount
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Updates a draft survey.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(SurveyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSurvey(
        Guid id,
        [FromBody] UpdateSurveyApiRequest request,
        CancellationToken cancellationToken)
    {
        var surveyId = SurveyId.Create(id);
        var survey = await _surveyRepository.GetByIdAsync(surveyId, cancellationToken);

        if (survey == null)
            return NotFound();

        if (!survey.Status.IsDraft)
            return Conflict(new ErrorResponse
            {
                Message = "Cannot modify survey after it has started"
            });

        // Apply updates
        if (!string.IsNullOrWhiteSpace(request.Title))
            survey.UpdateTitle(request.Title);

        if (request.Description != null)
            survey.UpdateDescription(request.Description);

        if (request.EndDate.HasValue)
            survey.UpdateCollectionPeriod(survey.CollectionPeriod.StartDate, request.EndDate.Value);

        await _surveyRepository.UpdateAsync(survey, cancellationToken);

        return Ok(new SurveyResponse
        {
            Id = survey.SurveyId.Value,
            Title = survey.Title.Value,
            Description = survey.Description,
            Status = survey.Status.Value,
            StartDate = survey.CollectionPeriod.StartDate,
            EndDate = survey.CollectionPeriod.EndDate,
            CreatedAt = survey.CreatedAt,
            UpdatedAt = survey.UpdatedAt,
            Questions = survey.Questions.Select(q => new QuestionResponse
            {
                Id = q.QuestionId.Value,
                Text = q.Text.Value,
                Order = q.Order,
                Options = survey.GetOptionsForQuestion(q.QuestionId).Select(o => new OptionResponse
                {
                    Id = o.OptionId.Value,
                    Text = o.Text.Value,
                    Order = o.Order
                }).ToList()
            }).ToList()
        });
    }

    /// <summary>
    /// Deletes a draft survey (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSurvey(Guid id, CancellationToken cancellationToken)
    {
        var surveyId = SurveyId.Create(id);
        var survey = await _surveyRepository.GetByIdAsync(surveyId, cancellationToken);

        if (survey == null)
            return NotFound();

        if (!survey.Status.IsDraft)
            return BadRequest(new ErrorResponse
            {
                Message = "Can only delete surveys in draft status"
            });

        // In a real implementation, you'd implement soft delete
        // For now, we'll just close it with a specific reason
        survey.Close("Deleted by user");
        await _surveyRepository.UpdateAsync(survey, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Adds a question to a draft survey.
    /// </summary>
    [HttpPost("{surveyId:guid}/questions")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddQuestion(
        Guid surveyId,
        [FromBody] AddQuestionApiRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addQuestionUseCase.ExecuteAsync(
            new AddQuestionRequest(surveyId, request.Text, request.Order),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found"))
                return NotFound();
            return BadRequest(new ErrorResponse { Message = result.Error });
        }

        var response = new QuestionResponse
        {
            Id = result.Value.QuestionId,
            Text = result.Value.Text,
            Order = result.Value.Order
        };

        return CreatedAtAction(
            nameof(GetSurvey),
            new { id = surveyId },
            response);
    }

    /// <summary>
    /// Adds an option to a question in a draft survey.
    /// </summary>
    [HttpPost("{surveyId:guid}/questions/{questionId:guid}/options")]
    [ProducesResponseType(typeof(OptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddOption(
        Guid surveyId,
        Guid questionId,
        [FromBody] AddOptionApiRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addOptionUseCase.ExecuteAsync(
            new AddOptionRequest(surveyId, questionId, request.Text, request.Order),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found"))
                return NotFound();
            return BadRequest(new ErrorResponse { Message = result.Error });
        }

        var response = new OptionResponse
        {
            Id = result.Value.OptionId,
            Text = result.Value.Text,
            Order = result.Value.Order
        };

        return CreatedAtAction(
            nameof(GetSurvey),
            new { id = surveyId },
            response);
    }

    /// <summary>
    /// Activates a draft survey, making it available for voting.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(SurveyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateSurvey(Guid id, CancellationToken cancellationToken)
    {
        var result = await _activateSurveyUseCase.ExecuteAsync(
            new ActivateSurveyRequest(id),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found"))
                return NotFound();
            return BadRequest(new ErrorResponse { Message = result.Error });
        }

        return Ok(new { result.Value.SurveyId, result.Value.Status, result.Value.ActivatedAt });
    }

    /// <summary>
    /// Closes an active survey.
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseSurvey(
        Guid id,
        [FromBody] CloseSurveyRequest? request,
        CancellationToken cancellationToken)
    {
        var surveyId = SurveyId.Create(id);
        var survey = await _surveyRepository.GetByIdAsync(surveyId, cancellationToken);

        if (survey == null)
            return NotFound();

        survey.Close(request?.Reason ?? string.Empty);
        await _surveyRepository.UpdateAsync(survey, cancellationToken);

        return Ok(new { survey.SurveyId.Value, Status = survey.Status.Value, ClosedAt = survey.UpdatedAt });
    }
}

public record CloseSurveyRequest
{
    public string Reason { get; init; } = string.Empty;
}

// Pagination response wrapper
public record PaginatedResponse<T>
{
    public List<T> Data { get; init; } = [];
    public PaginationInfo Pagination { get; init; } = null!;
}

public record PaginationInfo
{
    public int Total { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
    public bool HasMore { get; init; }
}