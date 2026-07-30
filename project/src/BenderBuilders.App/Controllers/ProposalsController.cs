using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BenderBuilders.App.Controllers;

public class ProposalsController : Controller
{
    private readonly IProposalService _proposalService;

    public ProposalsController(IProposalService proposalService)
    {
        _proposalService = proposalService;
    }

    public async Task<IActionResult> Index()
    {
        var proposals = await _proposalService.GetAllProposalsAsync();
        return View(proposals);
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
}
