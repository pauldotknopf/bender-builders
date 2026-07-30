using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.Interfaces;

public interface IProposalService
{
    /// <summary>
    /// Returns the most recent proposals, ordered by proposal date descending.
    /// </summary>
    /// <param name="count">Maximum number of proposals to return.</param>
    Task<IReadOnlyList<ProposalDto>> GetRecentProposalsAsync(int count);

    /// <summary>
    /// Returns all proposals, ordered by proposal date descending.
    /// </summary>
    Task<IReadOnlyList<ProposalDto>> GetAllProposalsAsync();

    /// <summary>
    /// Returns the proposal with the given id, or <c>null</c> if none exists.
    /// </summary>
    Task<ProposalDto?> GetProposalAsync(int id);

    /// <summary>
    /// Creates or updates a proposal. When <see cref="ProposalDto.Id"/> is 0 a new
    /// proposal is inserted; otherwise the existing proposal is updated. Returns the
    /// saved proposal with its <see cref="ProposalDto.Id"/> populated.
    /// </summary>
    Task<ProposalDto> SaveProposalAsync(ProposalDto proposal);
}