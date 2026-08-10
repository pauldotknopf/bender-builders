using System.IO;
using BenderBuilders.App.Models;
using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BenderBuilders.App.Controllers;

public class InvoicesController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IInvoiceLineItemService _lineItemService;
    private readonly IProposalService _proposalService;

    public InvoicesController(IInvoiceService invoiceService, IInvoiceLineItemService lineItemService, IProposalService proposalService)
    {
        _invoiceService = invoiceService;
        _lineItemService = lineItemService;
        _proposalService = proposalService;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int proposalId)
    {
        var proposal = await _proposalService.GetProposalAsync(proposalId);
        if (proposal is null)
        {
            return NotFound();
        }

        return View(new InvoiceFormViewModel
        {
            Invoice = new InvoiceDto
            {
                ProposalId = proposalId,
                InvoiceDate = DateTime.Today
            },
            Proposal = proposal
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceDto invoice, List<InvoiceLineItemDto> lineItems)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormViewModelAsync(invoice, lineItems));
        }

        var saved = await _invoiceService.SaveInvoiceAsync(invoice);
        await _lineItemService.ReplaceLineItemsForInvoiceAsync(saved.Id, CleanLineItems(lineItems));

        TempData["Success"] = "Invoice created.";
        return RedirectToAction(nameof(Edit), new { id = saved.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        var lineItems = await _lineItemService.GetLineItemsForInvoiceAsync(id);
        return View(await BuildFormViewModelAsync(invoice, lineItems.ToList()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InvoiceDto invoice, List<InvoiceLineItemDto> lineItems)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormViewModelAsync(invoice, lineItems));
        }

        var saved = await _invoiceService.SaveInvoiceAsync(invoice);
        await _lineItemService.ReplaceLineItemsForInvoiceAsync(saved.Id, CleanLineItems(lineItems));

        TempData["Success"] = "Invoice saved.";
        return RedirectToAction(nameof(Edit), new { id = saved.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        await _invoiceService.DeleteInvoiceAsync(id);

        TempData["Success"] = "Invoice deleted.";
        return RedirectToAction("Edit", "Proposals", new { id = invoice.ProposalId });
    }

    [HttpGet]
    public async Task<IActionResult> Print(int id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        var lineItems = await _lineItemService.GetLineItemsForInvoiceAsync(id);
        return View(await BuildFormViewModelAsync(invoice, lineItems.ToList()));
    }

    /// <summary>
    /// Generate a PDF of the invoice using Electron's webContents.printToPDF.
    /// Creates a hidden browser window, loads the print view, generates the PDF,
    /// and closes the window — all server-side.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GeneratePdf(int id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        var printUrl = $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(Print), "Invoices", new { id })}";

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

        browserWindow.LoadURL(printUrl);

        await Task.Delay(1500);

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
                $"Invoice-{invoice.InvoiceDate:yyyyMMdd}.pdf");
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

    private async Task<InvoiceFormViewModel> BuildFormViewModelAsync(InvoiceDto invoice, List<InvoiceLineItemDto> lineItems)
    {
        var proposal = invoice.ProposalId == 0 ? null : await _proposalService.GetProposalAsync(invoice.ProposalId);

        return new InvoiceFormViewModel
        {
            Invoice = invoice,
            LineItems = lineItems,
            Proposal = proposal
        };
    }

    private static List<InvoiceLineItemDto> CleanLineItems(List<InvoiceLineItemDto> lineItems)
    {
        return lineItems
            .Where(x => !string.IsNullOrWhiteSpace(x.Description) || x.Amount != 0m)
            .Select(x => new InvoiceLineItemDto
            {
                Id = x.Id,
                InvoiceId = x.InvoiceId,
                Description = x.Description.Trim(),
                Amount = x.Amount
            })
            .ToList();
    }
}
