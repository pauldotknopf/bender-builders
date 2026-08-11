using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.App.Models;

/// <summary>
/// Backs the invoice create/edit form. Carries the invoice itself, its
/// client-managed line items (posted back as a whole), and the owning
/// proposal so the page can show which customer the invoice belongs to.
/// </summary>
public class InvoiceFormViewModel
{
    public required InvoiceDto Invoice { get; set; }

    public List<InvoiceLineItemDto> LineItems { get; set; } = new();

    public ProposalDto Proposal { get; set; }
}
