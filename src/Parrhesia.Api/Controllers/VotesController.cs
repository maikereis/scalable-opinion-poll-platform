using Microsoft.AspNetCore.Mvc;
using Parrhesia.Api.Contracts;
using Parrhesia.Api.Middleware;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Repositories;
using Parrhesia.Domain.Voting.Services;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Api.Controllers;

[ApiController]
[Route("api/v1/surveys/{surveyId:guid}/votes")]
[Produces("application/json")]
public class VotesController : ControllerBase
{
    private readonly IVotingService _votingService;
    private readonly IBallotRepository _ballotRepository;
    private readonly IFingerprintGenerator _fingerprintGenerator;
    private readonly ISurveyQueryService _surveyQueryService;
    private readonly ILogger<VotesController> _logger;

    public VotesController(
        IVotingService votingService,
        IBallotRepository ballotRepository,
        IFingerprintGenerator fingerprintGenerator,
        ISurveyQueryService surveyQueryService,
        ILogger<VotesController> logger)
    {
        _votingService = votingService;
        _ballotRepository = ballotRepository;
        _fingerprintGenerator = fingerprintGenerator;
        _surveyQueryService = surveyQueryService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CastVote(
        Guid surveyId,
        [FromBody] CastVoteApiRequest request,
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        CancellationToken cancellationToken)
    {
        var effectiveUserId = userId ?? Guid.NewGuid();

        _logger.LogInformation(
            "=== CastVote START === UserId={UserId}, SurveyId={SurveyId}, QuestionId={QuestionId}, OptionId={OptionId}",
            effectiveUserId, surveyId, request.QuestionId, request.OptionId);

        // DEBUG: Verificar o que o SurveyQueryService está retornando
        var surveyStatus = await _surveyQueryService.GetSurveyStatusAsync(
            SurveyId.Create(surveyId), cancellationToken);

        if (surveyStatus != null)
        {
            _logger.LogInformation(
                "Survey Status: IsActive={IsActive}, QuestionCount={QuestionCount}",
                surveyStatus.IsActive, surveyStatus.QuestionsWithOptions.Count);

            foreach (var (qId, opts) in surveyStatus.QuestionsWithOptions)
            {
                _logger.LogInformation(
                    "Question {QuestionId} has {OptionCount} options: [{Options}]",
                    qId.Value, 
                    opts.Count, 
                    string.Join(", ", opts.Where(o => o != null).Select(o => o.Value)));
            }

            // Verificar se a questão existe
            var questionExists = surveyStatus.QuestionsWithOptions
                .Any(kvp => kvp.Key.Value == request.QuestionId);
            _logger.LogInformation("Question exists: {QuestionExists}", questionExists);

            // Verificar se a opção existe para essa questão
            var hasOption = surveyStatus.HasOption(
                QuestionId.Create(request.QuestionId),
                OptionId.Create(request.OptionId));
            _logger.LogInformation("HasOption result: {HasOption}", hasOption);
        }
        else
        {
            _logger.LogWarning("SurveyStatus is NULL!");
        }

        var deviceInfo = GetDeviceInfo();

        var result = await _votingService.CastVoteAsync(
            UserId.Create(effectiveUserId),
            SurveyId.Create(surveyId),
            QuestionId.Create(request.QuestionId),
            OptionId.Create(request.OptionId),
            deviceInfo,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("CastVote FAILED: {Error}", result.Error);

            if (result.Error.Contains("not found"))
                return NotFound(new ErrorResponse { Message = result.Error });

            if (result.Error.Contains("already voted"))
                return Conflict(new ErrorResponse { Message = result.Error });

            return BadRequest(new ErrorResponse 
            { 
                Status = 400,
                Title = "Bad Request",
                Message = result.Error 
            });
        }

        var ballot = result.Value;
        var response = new VoteResponse
        {
            BallotId = ballot.BallotId.Value,
            SurveyId = ballot.SurveyId.Value,
            QuestionId = ballot.QuestionId.Value,
            OptionId = ballot.SelectedOptionId.Value,
            CastedAt = ballot.CastedAt
        };

        _logger.LogInformation("=== CastVote SUCCESS === BallotId={BallotId}", ballot.BallotId.Value);

        return CreatedAtAction(null, response);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(VoteStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyVoteStatus(
        Guid surveyId,
        [FromHeader(Name = "X-User-Id")] Guid? userId,
        CancellationToken cancellationToken)
    {
        if (!userId.HasValue)
        {
            return Unauthorized(new ErrorResponse
            {
                Message = "Missing or invalid authentication. Please provide X-User-Id header."
            });
        }

        var fingerprint = _fingerprintGenerator.Generate(
            UserId.Create(userId.Value),
            SurveyId.Create(surveyId));

        var hasVoted = await _ballotRepository.HasVotedAsync(
            fingerprint,
            SurveyId.Create(surveyId),
            cancellationToken);

        var response = new VoteStatusResponse
        {
            HasVoted = hasVoted,
            Vote = hasVoted ? new VoteInfo
            {
                VotedAt = DateTime.UtcNow
            } : null
        };

        return Ok(response);
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVoteCount(Guid surveyId, CancellationToken cancellationToken)
    {
        var count = await _ballotRepository.CountBySurveyAsync(
            SurveyId.Create(surveyId),
            cancellationToken);

        return Ok(new { surveyId, totalVotes = count });
    }

    [HttpGet("check")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckVoteStatus(
        Guid surveyId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        var fingerprint = _fingerprintGenerator.Generate(
            UserId.Create(userId),
            SurveyId.Create(surveyId));

        var hasVoted = await _ballotRepository.HasVotedAsync(
            fingerprint,
            SurveyId.Create(surveyId),
            cancellationToken);

        return Ok(new { surveyId, userId, hasVoted });
    }

    private DeviceInfo? GetDeviceInfo()
    {
        var deviceId = Request.Headers["X-Device-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(deviceId))
            return null;

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return DeviceInfo.Create(deviceId, userAgent, ipAddress);
    }
}

public record VoteStatusResponse
{
    public bool HasVoted { get; init; }
    public VoteInfo? Vote { get; init; }
}

public record VoteInfo
{
    public Guid? OptionId { get; init; }
    public DateTime VotedAt { get; init; }
}