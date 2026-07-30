using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.Interfaces;

public interface IProposalService
{
    /// <summary>
    /// Returns the most recent proposals, ordered by proposal date descending.
    /// </summary>
    /// <param name="count">Maximum number of proposals to return.</param>
    Task<IReadOnlyList<ProposalDto>> GetRecentProposalsAsync(int count);
}