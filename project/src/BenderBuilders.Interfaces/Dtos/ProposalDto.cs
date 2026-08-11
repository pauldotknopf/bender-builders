using System.ComponentModel.DataAnnotations;

namespace BenderBuilders.Interfaces.Dtos;

/// <summary>
/// POCO data-transfer object representing a proposal. Maps to the
/// <c>Proposal</c> ORM model. Relationships are expressed ID-based:
/// invoices reference their proposal via <c>InvoiceDto.ProposalId</c>.
/// </summary>
public class ProposalDto
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Proposal Date")]
    public DateTime ProposalDate { get; set; }

    [Display(Name = "Address 1")]
    public string Address1 { get; set; }

    [Display(Name = "Address 2")]
    public string Address2 { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Display(Name = "Job Location")]
    public string JobLocation { get; set; }

    [Display(Name = "Fed ID Number")]
    public string FedIdNumber { get; set; }

    [Display(Name = "Proposal Summary")]
    public string ProposalSummary { get; set; }
}
