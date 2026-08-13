using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.App.Models;

public class ProposalsIndexViewModel
{
    public PagedResultDto<ProposalDto> Proposals { get; set; } = new();

    public string Search { get; set; }
}
