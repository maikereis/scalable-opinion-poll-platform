using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Services;

public interface IVotingService
{
    Task<Result<Ballot>> CastVoteAsync(
        UserId userId,
        SurveyId surveyId,
        QuestionId questionId,
        OptionId optionId,
        DeviceInfo? deviceInfo = null,
        CancellationToken cancellationToken = default);
}
