using BenderBuilders.Interfaces;
using BenderBuilders.Interfaces.Dtos;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BenderBuilders.Services.Tests;

[TestClass]
public class InvoiceLineItemServiceTests : DataTestBase
{
    private static async Task<int> CreateInvoiceAsync(IServiceProvider sp)
    {
        var proposalService = sp.GetRequiredService<IProposalService>();
        var invoiceService = sp.GetRequiredService<IInvoiceService>();

        var proposal = await proposalService.SaveProposalAsync(new ProposalDto
        {
            CustomerName = "Customer",
            ProposalDate = new DateTime(2026, 1, 1)
        });

        var invoice = await invoiceService.SaveInvoiceAsync(new InvoiceDto
        {
            ProposalId = proposal.Id,
            InvoiceDate = new DateTime(2026, 2, 1)
        });

        return invoice.Id;
    }

    [TestMethod]
    public async Task SaveLineItemAsync_inserts_a_new_line_item()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);

        var saved = await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto
        {
            InvoiceId = invoiceId,
            Description = "Labor",
            Amount = 1234.56m
        });

        saved.Id.Should().BeGreaterThan(0);

        var fetched = await lineItemService.GetLineItemAsync(saved.Id);
        fetched.Should().NotBeNull();
        fetched!.InvoiceId.Should().Be(invoiceId);
        fetched.Description.Should().Be("Labor");
        fetched.Amount.Should().Be(1234.56m);
    }

    [TestMethod]
    public async Task SaveLineItemAsync_supports_negative_amounts()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);

        var saved = await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto
        {
            InvoiceId = invoiceId,
            Description = "Discount",
            Amount = -50.25m
        });

        var fetched = await lineItemService.GetLineItemAsync(saved.Id);
        fetched!.Amount.Should().Be(-50.25m);
    }

    [TestMethod]
    public async Task SaveLineItemAsync_updates_an_existing_line_item()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);

        var saved = await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto
        {
            InvoiceId = invoiceId,
            Description = "Original",
            Amount = 10m
        });

        saved.Description = "Updated";
        saved.Amount = 20m;
        var updated = await lineItemService.SaveLineItemAsync(saved);

        updated.Id.Should().Be(saved.Id);

        var all = await lineItemService.GetLineItemsForInvoiceAsync(invoiceId);
        all.Should().HaveCount(1);
        all[0].Description.Should().Be("Updated");
        all[0].Amount.Should().Be(20m);
    }

    [TestMethod]
    public async Task GetLineItemAsync_returns_null_when_missing()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();

        var fetched = await lineItemService.GetLineItemAsync(999);

        fetched.Should().BeNull();
    }

    [TestMethod]
    public async Task GetLineItemsForInvoiceAsync_returns_only_that_invoices_line_items_in_order()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);
        var otherInvoiceId = await CreateInvoiceAsync(sp);

        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoiceId, Description = "First", Amount = 1m });
        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoiceId, Description = "Second", Amount = 2m });
        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = otherInvoiceId, Description = "Other", Amount = 3m });

        var lineItems = await lineItemService.GetLineItemsForInvoiceAsync(invoiceId);

        lineItems.Should().HaveCount(2);
        lineItems.Select(x => x.Description).Should().ContainInOrder("First", "Second");
    }

    [TestMethod]
    public async Task GetLineItemsForInvoiceAsync_returns_empty_when_no_line_items()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);

        var lineItems = await lineItemService.GetLineItemsForInvoiceAsync(invoiceId);

        lineItems.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DeleteLineItemAsync_removes_the_line_item()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);

        var saved = await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto
        {
            InvoiceId = invoiceId,
            Description = "Labor",
            Amount = 10m
        });

        var deleted = await lineItemService.DeleteLineItemAsync(saved.Id);

        deleted.Should().BeTrue();
        (await lineItemService.GetLineItemAsync(saved.Id)).Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteLineItemAsync_returns_false_when_missing()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();

        var deleted = await lineItemService.DeleteLineItemAsync(999);

        deleted.Should().BeFalse();
    }

    [TestMethod]
    public async Task ReplaceLineItemsForInvoiceAsync_inserts_updates_and_deletes()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);

        var keep = await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoiceId, Description = "Keep", Amount = 10m });
        var remove = await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoiceId, Description = "Remove", Amount = 20m });

        keep.Description = "Kept and edited";
        keep.Amount = 15m;

        var replaced = await lineItemService.ReplaceLineItemsForInvoiceAsync(invoiceId, new[]
        {
            keep,
            new InvoiceLineItemDto { InvoiceId = invoiceId, Description = "Brand new", Amount = -5m }
        });

        replaced.Should().HaveCount(2);
        replaced.Select(x => x.Id).Should().OnlyContain(id => id > 0);

        var lineItems = await lineItemService.GetLineItemsForInvoiceAsync(invoiceId);
        lineItems.Should().HaveCount(2);
        lineItems.Should().ContainSingle(x => x.Id == keep.Id && x.Description == "Kept and edited" && x.Amount == 15m);
        lineItems.Should().ContainSingle(x => x.Description == "Brand new" && x.Amount == -5m);
        (await lineItemService.GetLineItemAsync(remove.Id)).Should().BeNull();
    }

    [TestMethod]
    public async Task ReplaceLineItemsForInvoiceAsync_with_empty_collection_removes_all()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);

        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoiceId, Description = "Labor", Amount = 10m });
        await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto { InvoiceId = invoiceId, Description = "Materials", Amount = 20m });

        var replaced = await lineItemService.ReplaceLineItemsForInvoiceAsync(invoiceId, Array.Empty<InvoiceLineItemDto>());

        replaced.Should().BeEmpty();
        (await lineItemService.GetLineItemsForInvoiceAsync(invoiceId)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task ReplaceLineItemsForInvoiceAsync_forces_the_target_invoice_and_leaves_other_invoices_alone()
    {
        var sp = BuildServiceProvider();
        var lineItemService = sp.GetRequiredService<IInvoiceLineItemService>();
        var invoiceId = await CreateInvoiceAsync(sp);
        var otherInvoiceId = await CreateInvoiceAsync(sp);

        var otherLineItem = await lineItemService.SaveLineItemAsync(new InvoiceLineItemDto
        {
            InvoiceId = otherInvoiceId,
            Description = "Belongs elsewhere",
            Amount = 99m
        });

        // A caller posting back a foreign id / invoice id must not touch the other invoice.
        var replaced = await lineItemService.ReplaceLineItemsForInvoiceAsync(invoiceId, new[]
        {
            new InvoiceLineItemDto { Id = otherLineItem.Id, InvoiceId = otherInvoiceId, Description = "Hijack attempt", Amount = 1m }
        });

        replaced.Should().HaveCount(1);
        replaced[0].InvoiceId.Should().Be(invoiceId);
        replaced[0].Id.Should().NotBe(otherLineItem.Id);

        var otherLineItems = await lineItemService.GetLineItemsForInvoiceAsync(otherInvoiceId);
        otherLineItems.Should().HaveCount(1);
        otherLineItems[0].Description.Should().Be("Belongs elsewhere");
        otherLineItems[0].Amount.Should().Be(99m);
    }
}
