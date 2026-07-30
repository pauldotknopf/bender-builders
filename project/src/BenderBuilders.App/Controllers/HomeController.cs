using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BenderBuilders.App.Models;
using BenderBuilders.Interfaces;

namespace BenderBuilders.App.Controllers;

public class HomeController : Controller
{
    private const int RecentProposalCount = 5;

    private readonly IProposalService _proposalService;

    public HomeController(IProposalService proposalService)
    {
        _proposalService = proposalService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            RecentProposals = await _proposalService.GetRecentProposalsAsync(RecentProposalCount)
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
