using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using BenderBuilders.Services.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;

namespace BenderBuilders.Services.Tests;

[TestClass]
public class ProposalServiceTests : DataTestBase
{
    [TestMethod]
    public async Task GetRecentProposals_returns_most_recent_first_and_respects_count()
    {
        var sp = BuildServiceProvider();
        var dataService = sp.GetRequiredService<IDataService>();
        var proposalService = sp.GetRequiredService<IProposalService>();

        // The service migrates on-demand, so seed after it has run once.
        await proposalService.GetRecentProposalsAsync(1);
        
        using (var conScope = new ConScope(dataService))
        {
            var connection = conScope.Connection;
            connection.Save(new Proposal { CustomerName = "Oldest", ProposalDate = new DateTime(2026, 1, 1) });
            connection.Save(new Proposal { CustomerName = "Middle", ProposalDate = new DateTime(2026, 6, 15) });
            connection.Save(new Proposal { CustomerName = "Newest", ProposalDate = new DateTime(2026, 12, 31) });
        }

        var recent = await proposalService.GetRecentProposalsAsync(2);

        recent.Should().HaveCount(2);
        recent[0].CustomerName.Should().Be("Newest");
        recent[1].CustomerName.Should().Be("Middle");
    }

    [TestMethod]
    public async Task GetRecentProposals_returns_empty_when_no_proposals()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var recent = await proposalService.GetRecentProposalsAsync(5);

        recent.Should().BeEmpty();
    }

    [TestMethod]
    public async Task SaveProposalAsync_inserts_a_new_proposal()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var saved = await proposalService.SaveProposalAsync(new ProposalDto
        {
            CustomerName = "Jane Doe",
            ProposalDate = new DateTime(2026, 5, 1),
            City = "Springfield"
        });

        saved.Id.Should().BeGreaterThan(0);

        var fetched = await proposalService.GetProposalAsync(saved.Id);
        fetched.Should().NotBeNull();
        fetched!.CustomerName.Should().Be("Jane Doe");
        fetched.City.Should().Be("Springfield");
        fetched.ProposalDate.Should().Be(new DateTime(2026, 5, 1));
    }

    [TestMethod]
    public async Task SaveProposalAsync_updates_an_existing_proposal()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var saved = await proposalService.SaveProposalAsync(new ProposalDto
        {
            CustomerName = "Original",
            ProposalDate = new DateTime(2026, 5, 1)
        });

        saved.CustomerName = "Updated";
        var updated = await proposalService.SaveProposalAsync(saved);

        updated.Id.Should().Be(saved.Id);

        var all = await proposalService.GetAllProposalsAsync();
        all.Should().HaveCount(1);
        all[0].CustomerName.Should().Be("Updated");
    }

    [TestMethod]
    public async Task GetProposalAsync_returns_null_when_missing()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var fetched = await proposalService.GetProposalAsync(999);

        fetched.Should().BeNull();
    }

    [TestMethod]
    public async Task GetAllProposalsAsync_returns_all_ordered_by_date_descending()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Oldest", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Newest", ProposalDate = new DateTime(2026, 12, 31) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Middle", ProposalDate = new DateTime(2026, 6, 15) });

        var all = await proposalService.GetAllProposalsAsync();

        all.Should().HaveCount(3);
        all.Select(p => p.CustomerName).Should().ContainInOrder("Newest", "Middle", "Oldest");
    }

    [TestMethod]
    public async Task GetProposalsAsync_returns_paged_results_ordered_by_date_descending()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "One", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Two", ProposalDate = new DateTime(2026, 2, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Three", ProposalDate = new DateTime(2026, 3, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Four", ProposalDate = new DateTime(2026, 4, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Five", ProposalDate = new DateTime(2026, 5, 1) });

        var page1 = await proposalService.GetProposalsAsync(1, 2, null);

        page1.TotalCount.Should().Be(5);
        page1.TotalPages.Should().Be(3);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);
        page1.Items.Select(p => p.CustomerName).Should().ContainInOrder("Five", "Four");

        var page2 = await proposalService.GetProposalsAsync(2, 2, null);

        page2.Items.Select(p => p.CustomerName).Should().ContainInOrder("Three", "Two");

        var page3 = await proposalService.GetProposalsAsync(3, 2, null);

        page3.Items.Select(p => p.CustomerName).Should().ContainInOrder("One");
    }

    [TestMethod]
    public async Task GetProposalsAsync_returns_empty_items_when_page_out_of_range()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "One", ProposalDate = new DateTime(2026, 1, 1) });

        var result = await proposalService.GetProposalsAsync(5, 10, null);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.Page.Should().Be(5);
    }

    [TestMethod]
    public async Task GetProposalsAsync_clamps_page_below_one_to_first_page()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "One", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Two", ProposalDate = new DateTime(2026, 2, 1) });

        var result = await proposalService.GetProposalsAsync(0, 10, null);

        result.Page.Should().Be(1);
        result.Items.Select(p => p.CustomerName).Should().ContainInOrder("Two", "One");
    }

    [TestMethod]
    public async Task GetProposalsAsync_filters_by_customer_name_case_insensitively()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Springfield Plumbing", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Shelbyville Electric", ProposalDate = new DateTime(2026, 2, 1) });

        var result = await proposalService.GetProposalsAsync(1, 10, "springfield");

        result.TotalCount.Should().Be(1);
        result.Items.Select(p => p.CustomerName).Should().ContainInOrder("Springfield Plumbing");
    }

    [TestMethod]
    public async Task GetProposalsAsync_filters_by_city_and_state()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "One", City = "Springfield", State = "IL", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Two", City = "Shelbyville", State = "IL", ProposalDate = new DateTime(2026, 2, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Three", City = "Springfield", State = "MO", ProposalDate = new DateTime(2026, 3, 1) });

        var byCity = await proposalService.GetProposalsAsync(1, 10, "shelbyville");

        byCity.TotalCount.Should().Be(1);
        byCity.Items.Select(p => p.CustomerName).Should().ContainInOrder("Two");

        var byState = await proposalService.GetProposalsAsync(1, 10, "mo");

        byState.TotalCount.Should().Be(1);
        byState.Items.Select(p => p.CustomerName).Should().ContainInOrder("Three");
    }

    [TestMethod]
    public async Task GetProposalsAsync_filters_by_proposal_summary()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "One", ProposalSummary = "Full kitchen remodel", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Two", ProposalSummary = "Bathroom addition", ProposalDate = new DateTime(2026, 2, 1) });

        var result = await proposalService.GetProposalsAsync(1, 10, "kitchen");

        result.TotalCount.Should().Be(1);
        result.Items.Select(p => p.CustomerName).Should().ContainInOrder("One");
    }

    [TestMethod]
    public async Task GetProposalsAsync_search_with_no_matches_returns_empty()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "One", ProposalDate = new DateTime(2026, 1, 1) });

        var result = await proposalService.GetProposalsAsync(1, 10, "no-such-thing");

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [TestMethod]
    public async Task GetProposalsAsync_whitespace_search_returns_all_proposals()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "One", ProposalDate = new DateTime(2026, 1, 1) });
        await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Two", ProposalDate = new DateTime(2026, 2, 1) });

        var result = await proposalService.GetProposalsAsync(1, 10, "   ");

        result.TotalCount.Should().Be(2);
        result.Items.Select(p => p.CustomerName).Should().ContainInOrder("Two", "One");
    }

    [TestMethod]
    public async Task DeleteProposalAsync_removes_proposal_its_invoices_and_their_line_items()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();

        var proposal = await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "To Delete", ProposalDate = new DateTime(2026, 1, 1) });
        var otherProposal = await proposalService.SaveProposalAsync(new ProposalDto { CustomerName = "Keep", ProposalDate = new DateTime(2026, 2, 1) });

        var invoice = await invoiceService.SaveInvoiceAsync(new InvoiceDto { ProposalId = proposal.Id, InvoiceDate = new DateTime(2026, 3, 15) });
        var otherInvoice = await invoiceService.SaveInvoiceAsync(new InvoiceDto { ProposalId = otherProposal.Id, InvoiceDate = new DateTime(2026, 4, 15) });

        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoice.Id, Description = "Labor", Amount = 100m });
        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = otherInvoice.Id, Description = "Materials", Amount = 50m });

        var deleted = await proposalService.DeleteProposalAsync(proposal.Id);

        deleted.Should().BeTrue();
        (await proposalService.GetProposalAsync(proposal.Id)).Should().BeNull();
        (await invoiceService.GetInvoicesForProposalAsync(proposal.Id)).Should().BeEmpty();
        (await lineItemService.GetLineItemsForInvoiceAsync(invoice.Id)).Should().BeEmpty();

        (await proposalService.GetProposalAsync(otherProposal.Id)).Should().NotBeNull();
        (await lineItemService.GetLineItemsForInvoiceAsync(otherInvoice.Id)).Should().HaveCount(1);
    }

    [TestMethod]
    public async Task DeleteProposalAsync_returns_false_when_missing()
    {
        var sp = BuildServiceProvider();
        var proposalService = sp.GetRequiredService<IProposalService>();

        var deleted = await proposalService.DeleteProposalAsync(999);

        deleted.Should().BeFalse();
    }
}
