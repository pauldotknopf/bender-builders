using System.Data;
using BenderBuilders.Services.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using SharpDataAccess.Migrations;

namespace BenderBuilders.Services.Migrations;

public class _0001_InitialMigration : IMigration
{
    [Alias("Invoice")]
    public class _Invoice
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(_Proposal))]
        public int ProposalId { get; set; }

        public DateTime InvoiceDate { get; set; }

        [Reference]
        public List<_InvoiceLineItem> LineItems { get; set; } = new();
    }
    
    [Alias("InvoiceLineItem")]
    public class _InvoiceLineItem
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(_Invoice))]
        public int InvoiceId { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Line item amount. May be negative (e.g. discounts / adjustments).
        /// </summary>
        public decimal Amount { get; set; }
    }
    
    [Alias("Proposal")]
    public class _Proposal
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
        public List<_Invoice> Invoices { get; set; } = new();
    }
    
    public void Run(IDbConnection connection)
    {
        OrmLiteUtils.PrintSql();
        connection.CreateTable<_Proposal>();
        connection.CreateTable<_Invoice>();
        connection.CreateTable<_InvoiceLineItem>();
    }

    public int Version => 1;
}