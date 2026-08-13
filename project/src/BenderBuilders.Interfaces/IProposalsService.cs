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
    /// Returns a single page of proposals, ordered by proposal date descending.
    /// </summary>
    /// <param name="page">The 1-based page number. Values below 1 are treated as 1.</param>
    /// <param name="pageSize">The maximum number of proposals per page.</param>
    /// <param name="search">
    /// Optional text used to filter proposals against their customer name, summary,
    /// city, state, and job location. Matches are case-insensitive. <c>null</c>,
    /// empty, or whitespace returns all proposals.
    /// </param>
    Task<PagedResultDto<ProposalDto>> GetProposalsAsync(int page, int pageSize, string search);

    /// <summary>
    /// Returns the proposal with the given id, or <c>null</c> if none exists.
    /// </summary>
    Task<ProposalDto> GetProposalAsync(int id);

    /// <summary>
    /// Creates or updates a proposal. When <see cref="ProposalDto.Id"/> is 0 a new
    /// proposal is inserted; otherwise the existing proposal is updated. Returns the
    /// saved proposal with its <see cref="ProposalDto.Id"/> populated.
    /// </summary>
    Task<ProposalDto> SaveProposalAsync(ProposalDto proposal);
}