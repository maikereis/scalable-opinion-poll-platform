using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Repositories;

public interface IBallotRepository
{
    Task AddAsync(Ballot ballot, CancellationToken cancellationToken = default);
    Task<bool> HasVotedAsync(VoterFingerprint fingerprint, SurveyId surveyId, CancellationToken cancellationToken = default);
    Task<long> CountBySurveyAsync(SurveyId surveyId, CancellationToken cancellationToken = default);
    Task<long> CountByOptionAsync(OptionId optionId, CancellationToken cancellationToken = default);
}
