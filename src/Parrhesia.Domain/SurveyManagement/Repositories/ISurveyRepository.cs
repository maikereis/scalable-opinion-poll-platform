using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.SurveyManagement.Repositories;

public interface ISurveyRepository
{
    Task<Survey?> GetByIdAsync(SurveyId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Survey>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Survey survey, CancellationToken cancellationToken = default);
    Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(SurveyId id, CancellationToken cancellationToken = default);
}
