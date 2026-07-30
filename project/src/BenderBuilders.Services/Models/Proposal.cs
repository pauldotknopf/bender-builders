using ServiceStack.DataAnnotations;

namespace BenderBuilders.Services.Models;

/// <summary>
/// ORM-mapped model representing a proposal. A proposal can have many invoices.
/// </summary>
[Alias("Proposal")]
public class Proposal
{
    [AutoIncrement]
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

    [Reference]
    public List<Invoice> Invoices { get; set; } = new();
}
