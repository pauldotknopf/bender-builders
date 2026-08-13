using System.IO;
using BenderBuilders.App.Models;
using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BenderBuilders.App.Controllers;

public class ProposalsController : Controller
{
    private const int PageSize = 10;

    private readonly IProposalService _proposalService;
    private readonly IInvoiceService _invoiceService;
    private readonly IInvoiceLineItemService _lineItemService;

    public ProposalsController(IProposalService proposalService, IInvoiceService invoiceService, IInvoiceLineItemService lineItemService)
    {
        _proposalService = proposalService;
        _invoiceService = invoiceService;
        _lineItemService = lineItemService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null)
    {
        if (page < 1)
        {
            page = 1;
        }

        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var proposals = await _proposalService.GetProposalsAsync(page, PageSize, search);
        return View(new ProposalsIndexViewModel { Proposals = proposals, Search = search });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProposalDto { ProposalDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProposalDto proposal)
    {
        if (!ModelState.IsValid)
        {
            return View(proposal);
        }

        var saved = await _proposalService.SaveProposalAsync(proposal);
        TempData["Success"] = "Proposal created.";
        return RedirectToAction(nameof(Edit), new { id = saved.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var proposal = await _proposalService.GetProposalAsync(id);
        if (proposal is null)
        {
            return NotFound();
        }

        var invoices = await _invoiceService.GetInvoicesForProposalAsync(id);
        var summaries = new List<InvoiceSummary>();
        foreach (var invoice in invoices)
        {
            var lineItems = await _lineItemService.GetLineItemsForInvoiceAsync(invoice.Id);
            summaries.Add(new InvoiceSummary
            {
                Invoice = invoice,
                Total = lineItems.Sum(x => x.Amount)
            });
        }

        return View(new ProposalEditViewModel
        {
            Proposal = proposal,
            Invoices = summaries
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var proposal = await _proposalService.GetProposalAsync(id);
        if (proposal is null)
        {
            return NotFound();
        }

        await _proposalService.DeleteProposalAsync(id);

        TempData["Success"] = "Proposal deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Print(int id)
    {
        var proposal = await _proposalService.GetProposalAsync(id);
        if (proposal is null)
        {
            return NotFound();
        }

        return View(proposal);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProposalDto proposal)
    {
        if (!ModelState.IsValid)
        {
            return View(proposal);
        }

        var saved = await _proposalService.SaveProposalAsync(proposal);
        TempData["Success"] = "Proposal saved.";
        return RedirectToAction(nameof(Edit), new { id = saved.Id });
    }

    /// <summary>
    /// Generate a PDF of the proposal using Electron's webContents.printToPDF.
    /// Creates a hidden browser window, loads the print view, generates the PDF,
    /// and closes the window — all server-side.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GeneratePdf(int id)
    {
        var proposal = await _proposalService.GetProposalAsync(id);
        if (proposal is null)
        {
            return NotFound();
        }

        // Build the URL to the print view (no layout, clean HTML for PDF).
        var printUrl = $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(Print), "Proposals", new { id })}";

        // Create a hidden browser window for PDF generation.
        var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
        {
            Show = false,
            Width = 1024,
            Height = 768,
            WebPreferences = new WebPreferences
            {
                NodeIntegration = true
            }
        });

        // Navigate to the print view (synchronous LoadURL, then wait for load event).
        browserWindow.LoadURL(printUrl);

        // Give the page a moment to fully render (CSS, fonts, etc.).
        await Task.Delay(1500);

        // Generate the PDF using Electron's printToPDF API.
        // The API writes to a file path and returns success/failure.
        var pdfPath = Path.GetTempFileName();

        try
        {
            var pdfOptions = new PrintToPDFOptions
            {
                Landscape = false,
                PrintBackground = true
            };

            var success = await browserWindow.WebContents.PrintToPDFAsync(pdfPath, pdfOptions);
            if (!success)
            {
                return StatusCode(500, "PDF generation failed.");
            }

            var pdfData = await System.IO.File.ReadAllBytesAsync(pdfPath);

            return File(
                pdfData,
                "application/pdf",
                $"Proposal-{proposal.CustomerName}-{proposal.ProposalDate:yyyyMMdd}.pdf");
        }
        finally
        {
            browserWindow.Close();
            if (System.IO.File.Exists(pdfPath))
            {
                System.IO.File.Delete(pdfPath);
            }
        }
    }
}
