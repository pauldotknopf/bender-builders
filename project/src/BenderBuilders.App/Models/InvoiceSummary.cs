using BenderBuilders.Interfaces.Dtos;

namespace BenderBuilders.App.Models;

/// <summary>
/// One invoice row shown on the proposal edit page. The total is computed
/// server-side so the grid can display it without per-row queries in the view.
/// </summary>
public class InvoiceSummary
{
    public required InvoiceDto Invoice { get; set; }

    public decimal Total { get; set; }
}
