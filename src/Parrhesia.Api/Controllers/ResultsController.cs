using System.Text;
using Microsoft.AspNetCore.Mvc;
using Parrhesia.Api.Contracts;
using Parrhesia.Api.Middleware;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Repositories;

namespace Parrhesia.Api.Controllers;

[ApiController]
[Route("api/v1/surveys/{surveyId:guid}/results")]
[Produces("application/json")]
public class ResultsController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IBallotRepository _ballotRepository;

    public ResultsController(
        ISurveyRepository surveyRepository,
        IBallotRepository ballotRepository)
    {
        _surveyRepository = surveyRepository;
        _ballotRepository = ballotRepository;
    }

    /// <summary>
    /// Gets the voting results for a survey.
    /// </summary>
    /// <remarks>
    /// Returns vote counts and percentages for each option in each question.
    /// Results are only available for active or closed surveys.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(SurveyResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetResults(
        Guid surveyId,
        [FromQuery] bool includeTimeSeries = false,
        [FromQuery] bool includeMetadata = false,
        CancellationToken cancellationToken = default)
    {
        var survey = await _surveyRepository.GetByIdAsync(
            SurveyId.Create(surveyId),
            cancellationToken);

        if (survey == null)
            return NotFound();

        if (survey.Status.IsDraft)
            return BadRequest(new ErrorResponse
            {
                Message = "Results are not available for draft surveys"
            });

        var totalVotes = await _ballotRepository.CountBySurveyAsync(
            survey.SurveyId,
            cancellationToken);

        var questionResults = new List<QuestionResultResponse>();

        foreach (var question in survey.Questions.OrderBy(q => q.Order))
        {
            var options = survey.GetOptionsForQuestion(question.QuestionId);

            // Get raw counts for all options
            var rawResults = new List<(Option option, long votes)>();
            long questionVotes = 0;

            foreach (var option in options)
            {
                var optionVotes = await _ballotRepository.CountByOptionAsync(
                    option.OptionId, 
                    cancellationToken);
                questionVotes += optionVotes;
                rawResults.Add((option, optionVotes));
            }

            // Project into response DTO
            var finalOptionResults = rawResults.Select(r => new OptionResultResponse
            {
                OptionId = r.option.OptionId.Value,
                Text = r.option.Text.Value,
                Votes = r.votes,
                Percentage = questionVotes > 0
                    ? Math.Round((double)r.votes / questionVotes * 100, 2)
                    : 0
            }).ToList();

            questionResults.Add(new QuestionResultResponse
            {
                QuestionId = question.QuestionId.Value,
                Text = question.Text.Value,
                TotalVotes = questionVotes,
                Options = finalOptionResults
            });
        }

        var response = new SurveyResultsResponse
        {
            SurveyId = survey.SurveyId.Value,
            Title = survey.Title.Value,
            TotalVotes = totalVotes,
            Questions = questionResults
        };

        return Ok(response);
    }

    /// <summary>
    /// Exports survey results in various formats.
    /// </summary>
    /// <remarks>
    /// Supports CSV and JSON export formats. CSV includes headers and is suitable
    /// for spreadsheet applications. JSON provides structured data for programmatic access.
    /// </remarks>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportResults(
        Guid surveyId,
        [FromQuery] string format = "csv",
        [FromQuery] bool includeRawVotes = false,
        CancellationToken cancellationToken = default)
    {
        var survey = await _surveyRepository.GetByIdAsync(
            SurveyId.Create(surveyId),
            cancellationToken);

        if (survey == null)
            return NotFound();

        if (survey.Status.IsDraft)
            return BadRequest(new ErrorResponse
            {
                Message = "Cannot export results for draft surveys"
            });

        // Get results data
        var questionResults = new List<QuestionResultData>();

        foreach (var question in survey.Questions.OrderBy(q => q.Order))
        {
            var options = survey.GetOptionsForQuestion(question.QuestionId);
            var optionResults = new List<OptionResultData>();
            long questionVotes = 0;

            foreach (var option in options.OrderBy(o => o.Order))
            {
                var votes = await _ballotRepository.CountByOptionAsync(
                    option.OptionId,
                    cancellationToken);
                questionVotes += votes;

                optionResults.Add(new OptionResultData
                {
                    OptionText = option.Text.Value,
                    Votes = votes,
                    Order = option.Order
                });
            }

            // Calculate percentages
            foreach (var optionResult in optionResults)
            {
                optionResult.Percentage = questionVotes > 0
                    ? Math.Round((double)optionResult.Votes / questionVotes * 100, 2)
                    : 0;
            }

            questionResults.Add(new QuestionResultData
            {
                QuestionText = question.Text.Value,
                TotalVotes = questionVotes,
                Options = optionResults
            });
        }

        // Generate export based on format
        return format.ToLower() switch
        {
            "csv" => GenerateCsvExport(survey, questionResults),
            "json" => GenerateJsonExport(survey, questionResults),
            _ => BadRequest(new ErrorResponse { Message = "Invalid format. Supported: csv, json" })
        };
    }

    /// <summary>
    /// Gets a summary of voting statistics for a survey.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(Guid surveyId, CancellationToken cancellationToken)
    {
        var survey = await _surveyRepository.GetByIdAsync(
            SurveyId.Create(surveyId),
            cancellationToken);

        if (survey == null)
            return NotFound();

        var totalVotes = await _ballotRepository.CountBySurveyAsync(
            survey.SurveyId,
            cancellationToken);

        var summary = new
        {
            SurveyId = survey.SurveyId.Value,
            Title = survey.Title.Value,
            Status = survey.Status.Value,
            QuestionCount = survey.Questions.Count,
            TotalVotes = totalVotes,
            IsAcceptingVotes = survey.IsAcceptingVotes(),
            Period = new
            {
                Start = survey.CollectionPeriod.StartDate,
                End = survey.CollectionPeriod.EndDate,
                DaysRemaining = survey.CollectionPeriod.EndDate > DateTime.UtcNow
                    ? (int)(survey.CollectionPeriod.EndDate - DateTime.UtcNow).TotalDays
                    : 0
            }
        };

        return Ok(summary);
    }

    private FileContentResult GenerateCsvExport(
        Domain.SurveyManagement.Aggregates.Survey survey,
        List<QuestionResultData> questionResults)
    {
        var csv = new StringBuilder();

        // Header
        csv.AppendLine($"Survey: {survey.Title.Value}");
        csv.AppendLine($"Status: {survey.Status.Value}");
        csv.AppendLine($"Period: {survey.CollectionPeriod.StartDate:yyyy-MM-dd} to {survey.CollectionPeriod.EndDate:yyyy-MM-dd}");
        csv.AppendLine();

        // Results for each question
        foreach (var question in questionResults)
        {
            csv.AppendLine($"Question: {question.QuestionText}");
            csv.AppendLine($"Total Votes: {question.TotalVotes}");
            csv.AppendLine();
            csv.AppendLine("Option,Votes,Percentage");

            foreach (var option in question.Options.OrderBy(o => o.Order))
            {
                csv.AppendLine($"\"{option.OptionText}\",{option.Votes},{option.Percentage}%");
            }

            csv.AppendLine();
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"survey-results-{survey.SurveyId.Value}.csv";

        return File(bytes, "text/csv", fileName);
    }

    private FileContentResult GenerateJsonExport(
        Domain.SurveyManagement.Aggregates.Survey survey,
        List<QuestionResultData> questionResults)
    {
        var exportData = new
        {
            survey = new
            {
                id = survey.SurveyId.Value,
                title = survey.Title.Value,
                status = survey.Status.Value,
                period = new
                {
                    start = survey.CollectionPeriod.StartDate,
                    end = survey.CollectionPeriod.EndDate
                }
            },
            questions = questionResults.Select(q => new
            {
                text = q.QuestionText,
                totalVotes = q.TotalVotes,
                options = q.Options.Select(o => new
                {
                    text = o.OptionText,
                    votes = o.Votes,
                    percentage = o.Percentage
                })
            }),
            exportedAt = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        var fileName = $"survey-results-{survey.SurveyId.Value}.json";

        return File(bytes, "application/json", fileName);
    }
}

// Internal data structures for export
internal record QuestionResultData
{
    public string QuestionText { get; init; } = string.Empty;
    public long TotalVotes { get; init; }
    public List<OptionResultData> Options { get; init; } = [];
}

internal record OptionResultData
{
    public string OptionText { get; init; } = string.Empty;
    public long Votes { get; set; }
    public double Percentage { get; set; }
    public int Order { get; init; }
}