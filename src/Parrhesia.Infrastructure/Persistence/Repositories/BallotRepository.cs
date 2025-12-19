using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.Repositories;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Infrastructure.Persistence.Repositories;

public class BallotRepository : IBallotRepository
{
    private readonly ParrhesiaDbContext _context;

    public BallotRepository(ParrhesiaDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Ballot ballot, CancellationToken cancellationToken = default)
    {
        _context.Ballots.Add(ballot);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasVotedAsync(
        VoterFingerprint fingerprint, 
        SurveyId surveyId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Ballots
            .AnyAsync(b => 
                b.VoterFingerprint == fingerprint && 
                b.SurveyId == surveyId, 
                cancellationToken);
    }

    public async Task<long> CountBySurveyAsync(SurveyId surveyId, CancellationToken cancellationToken = default)
    {
        return await _context.Ballots
            .LongCountAsync(b => b.SurveyId == surveyId, cancellationToken);
    }

    public async Task<long> CountByOptionAsync(OptionId optionId, CancellationToken cancellationToken = default)
    {
        return await _context.Ballots
            .LongCountAsync(b => b.SelectedOptionId == optionId, cancellationToken);
    }
}
