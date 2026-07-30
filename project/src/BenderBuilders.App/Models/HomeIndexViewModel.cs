using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.App.Models;

public class HomeIndexViewModel
{
    public IReadOnlyList<ProposalDto> RecentProposals { get; set; } = new List<ProposalDto>();
}
