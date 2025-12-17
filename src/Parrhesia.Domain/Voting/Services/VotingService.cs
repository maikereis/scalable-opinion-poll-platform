using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.Repositories;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Services;

public class VotingService : IVotingService
{
    private readonly IBallotRepository _ballotRepository;
    private readonly IFingerprintGenerator _fingerprintGenerator;
    private readonly ISurveyQueryService _surveyQueryService;

    public VotingService(
        IBallotRepository ballotRepository,
        IFingerprintGenerator fingerprintGenerator,
        ISurveyQueryService surveyQueryService)
    {
        _ballotRepository = ballotRepository;
        _fingerprintGenerator = fingerprintGenerator;
        _surveyQueryService = surveyQueryService;
    }

    public async Task<Result<Ballot>> CastVoteAsync(
        UserId userId,
        SurveyId surveyId,
        QuestionId questionId,
        OptionId optionId,
        DeviceInfo? deviceInfo = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Verify survey is active and accepts votes
        var surveyStatus = await _surveyQueryService.GetSurveyStatusAsync(surveyId, cancellationToken);

        if (surveyStatus == null)
            return Result.Failure<Ballot>("Survey not found");

        if (!surveyStatus.IsActive)
            return Result.Failure<Ballot>("Survey is not active");

        // 2. Verify option belongs to question
        if (!surveyStatus.HasOption(questionId, optionId))
            return Result.Failure<Ballot>("Invalid option for the specified question");

        // 3. Generate fingerprint
        var fingerprint = _fingerprintGenerator.Generate(userId, surveyId);

        // 4. Check for duplicate vote
        if (await _ballotRepository.HasVotedAsync(fingerprint, surveyId, cancellationToken))
            return Result.Failure<Ballot>("User has already voted in this survey");

        // 5. Create ballot
        var ballot = Ballot.Cast(surveyId, questionId, optionId, fingerprint, deviceInfo);

        // 6. Persist
        await _ballotRepository.AddAsync(ballot, cancellationToken);

        return Result.Success(ballot);
    }
}
