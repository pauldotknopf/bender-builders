using ServiceStack.DataAnnotations;

namespace BenderBuilders.Services.Models;

/// <summary>
/// ORM-mapped model representing an invoice. Each invoice belongs to a single
/// proposal and can have many line items.
/// </summary>
[Alias("Invoice")]
public class Invoice
{
    [AutoIncrement]
    public int Id { get; set; }

    [References(typeof(Proposal))]
    public int ProposalId { get; set; }

    public DateTime InvoiceDate { get; set; }

    [Reference]
    public List<InvoiceLineItem> LineItems { get; set; } = new();
}
