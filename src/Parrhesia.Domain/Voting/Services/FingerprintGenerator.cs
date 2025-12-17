using System.Security.Cryptography;
using System.Text;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Services;

public class FingerprintGenerator : IFingerprintGenerator
{
    private readonly string _systemSalt;

    public FingerprintGenerator(string systemSalt)
    {
        if (string.IsNullOrWhiteSpace(systemSalt))
            throw new ArgumentException("System salt is required", nameof(systemSalt));

        _systemSalt = systemSalt;
    }

    public VoterFingerprint Generate(UserId userId, SurveyId surveyId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(surveyId);

        var input = $"{userId.Value}:{surveyId.Value}:{_systemSalt}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var hexString = Convert.ToHexString(hash);

        return VoterFingerprint.Create(hexString);
    }
}
