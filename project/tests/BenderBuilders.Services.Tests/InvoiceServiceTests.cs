using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BenderBuilders.Services.Tests;

[TestClass]
public class InvoiceServiceTests : DataTestBase
{
    private static async Task<int> CreateProposalAsync(IServiceProvider sp, string customerName = "Customer")
    {
        var proposalService = sp.GetRequiredService<IProposalService>();
        var proposal = await proposalService.SaveProposalAsync(new ProposalDto
        {
            CustomerName = customerName,
            ProposalDate = new DateTime(2026, 1, 1)
        });
        return proposal.Id;
    }

    [TestMethod]
    public async Task SaveInvoiceAsync_inserts_a_new_invoice()
    {
        var sp = BuildServiceProvider();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();
        var proposalId = await CreateProposalAsync(sp);

        var saved = await invoiceService.SaveInvoiceAsync(new InvoiceDto
        {
            ProposalId = proposalId,
            InvoiceDate = new DateTime(2026, 3, 15)
        });

        saved.Id.Should().BeGreaterThan(0);

        var fetched = await invoiceService.GetInvoiceAsync(saved.Id);
        fetched.Should().NotBeNull();
        fetched!.ProposalId.Should().Be(proposalId);
        fetched.InvoiceDate.Should().Be(new DateTime(2026, 3, 15));
    }

    [TestMethod]
    public async Task SaveInvoiceAsync_updates_an_existing_invoice()
    {
        var sp = BuildServiceProvider();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();
        var proposalId = await CreateProposalAsync(sp);

        var saved = await invoiceService.SaveInvoiceAsync(new InvoiceDto
        {
            ProposalId = proposalId,
            InvoiceDate = new DateTime(2026, 3, 15)
        });

        saved.InvoiceDate = new DateTime(2026, 4, 20);
        var updated = await invoiceService.SaveInvoiceAsync(saved);

        updated.Id.Should().Be(saved.Id);

        var all = await invoiceService.GetInvoicesForProposalAsync(proposalId);
        all.Should().HaveCount(1);
        all[0].InvoiceDate.Should().Be(new DateTime(2026, 4, 20));
    }

    [TestMethod]
    public async Task GetInvoiceAsync_returns_null_when_missing()
    {
        var sp = BuildServiceProvider();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();

        var fetched = await invoiceService.GetInvoiceAsync(999);

        fetched.Should().BeNull();
    }

    [TestMethod]
    public async Task GetInvoicesForProposalAsync_returns_only_that_proposals_invoices_newest_first()
    {
        var sp = BuildServiceProvider();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();
        var proposalId = await CreateProposalAsync(sp, "Mine");
        var otherProposalId = await CreateProposalAsync(sp, "Theirs");

        await invoiceService.SaveInvoiceAsync(new InvoiceDto { ProposalId = proposalId, InvoiceDate = new DateTime(2026, 1, 1) });
        await invoiceService.SaveInvoiceAsync(new InvoiceDto { ProposalId = proposalId, InvoiceDate = new DateTime(2026, 12, 31) });
        await invoiceService.SaveInvoiceAsync(new InvoiceDto { ProposalId = proposalId, InvoiceDate = new DateTime(2026, 6, 15) });
        await invoiceService.SaveInvoiceAsync(new InvoiceDto { ProposalId = otherProposalId, InvoiceDate = new DateTime(2026, 7, 1) });

        var invoices = await invoiceService.GetInvoicesForProposalAsync(proposalId);

        invoices.Should().HaveCount(3);
        invoices.Select(i => i.InvoiceDate).Should().ContainInOrder(
            new DateTime(2026, 12, 31),
            new DateTime(2026, 6, 15),
            new DateTime(2026, 1, 1));
    }

    [TestMethod]
    public async Task GetInvoicesForProposalAsync_returns_empty_when_no_invoices()
    {
        var sp = BuildServiceProvider();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();
        var proposalId = await CreateProposalAsync(sp);

        var invoices = await invoiceService.GetInvoicesForProposalAsync(proposalId);

        invoices.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DeleteInvoiceAsync_removes_the_invoice_and_its_line_items()
    {
        var sp = BuildServiceProvider();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var proposalId = await CreateProposalAsync(sp);

        var invoice = await invoiceService.SaveInvoiceAsync(new InvoiceDto
        {
            ProposalId = proposalId,
            InvoiceDate = new DateTime(2026, 3, 15)
        });
        var keptInvoice = await invoiceService.SaveInvoiceAsync(new InvoiceDto
        {
            ProposalId = proposalId,
            InvoiceDate = new DateTime(2026, 5, 15)
        });

        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoice.Id, Description = "Labor", Amount = 100m });
        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = keptInvoice.Id, Description = "Materials", Amount = 50m });

        var deleted = await invoiceService.DeleteInvoiceAsync(invoice.Id);

        deleted.Should().BeTrue();
        (await invoiceService.GetInvoiceAsync(invoice.Id)).Should().BeNull();
        (await lineItemService.GetLineItemsForInvoiceAsync(invoice.Id)).Should().BeEmpty();
        (await lineItemService.GetLineItemsForInvoiceAsync(keptInvoice.Id)).Should().HaveCount(1);
    }

    [TestMethod]
    public async Task DeleteInvoiceAsync_returns_false_when_missing()
    {
        var sp = BuildServiceProvider();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();

        var deleted = await invoiceService.DeleteInvoiceAsync(999);

        deleted.Should().BeFalse();
    }
}
