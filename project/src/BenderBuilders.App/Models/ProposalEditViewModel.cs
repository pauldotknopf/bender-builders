using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.App.Models;

public class ProposalEditViewModel
{
    public required ProposalDto Proposal { get; set; }

    public IReadOnlyList<InvoiceSummary> Invoices { get; set; } = new List<InvoiceSummary>();
}
