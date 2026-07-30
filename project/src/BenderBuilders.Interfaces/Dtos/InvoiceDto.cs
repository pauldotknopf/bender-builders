namespace BenderBuilders.Interfaces.Dtos;

/// <summary>
/// POCO data-transfer object representing an invoice. Maps to the
/// <c>Invoice</c> ORM model. The owning proposal is referenced ID-based
/// via <see cref="ProposalId"/>; line items reference their invoice via
/// <c>InvoiceLineItemDto.InvoiceId</c>.
/// </summary>
public class InvoiceDto
{
    public int Id { get; set; }

    public int ProposalId { get; set; }

    public DateTime InvoiceDate { get; set; }
}
