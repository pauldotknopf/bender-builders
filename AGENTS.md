# Description

This repo is a completely client-side application that allows users to manage proposals and invoices.

# High level features

* Manage proposals and invoices.
* Each proposal can have multilple invoices (one-to-many)
* Generating PDF documents for each proposal/invoice for external use (sending emails, etc)

# Detailed features

* A table/grid that allows users to see all proposals.
* A form used to add/edit proposals.
* A table/grid that allows users to see all invoices for a particular proposal.
* Each proposal has the following fields:
  * Customer Name -  text
  * Proposal date - date (no time)
  * Address 1 - text
  * Address 2 - text
  * City - text
  * State - text
  * Phone number - text
  * Job location - text
  * Fed ID number - text
  * Proposal summary - text
* Each invoice has the following fields:
  * Invoice date - date (no time)
  * Individual line items (one-to-many). Each line item has the following fields:
    * Description - text
    * Amount - money (could be negative)
* Both proposals and invoices have a button that opens a new print-friendly page of each respectively, allowing the end-user to use the browsers "Save to PDF" functionality.

# Technology

