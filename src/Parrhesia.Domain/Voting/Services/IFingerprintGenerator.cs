using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Services;

public interface IFingerprintGenerator
{
    VoterFingerprint Generate(UserId userId, SurveyId surveyId);
}
