namespace BenderBuilders.Interfaces.Dtos;

/// <summary>
/// POCO data-transfer object representing a proposal. Maps to the
/// <c>Proposal</c> ORM model. Relationships are expressed ID-based:
/// invoices reference their proposal via <c>InvoiceDto.ProposalId</c>.
/// </summary>
public class ProposalDto
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime ProposalDate { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PhoneNumber { get; set; }

    public string? JobLocation { get; set; }

    public string? FedIdNumber { get; set; }

    public string? ProposalSummary { get; set; }
}
