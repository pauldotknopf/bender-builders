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

* The entire project is located under ```./project```.
* BenderBuilders.App is a ASP.NET MVC project, using Razor ```cshtml``` files.
* ElectronNET.Core v0.5.1 is being used to wrap the project up in an electron environment.
* jQuery v3.7.1 is being used.
* Bootstrap v5.3.3 (css and js) is being used.
* ServiceStack.OrmLite v10 is used for SQLite access. SharpDataAccess is used to manage the raw IDbConnections.

# Considerations

* Traditional post-backs (with MVC model binding) should be preferred (as opposed to $.ajax or similar).
* The service layer should have a good coverage of integration tests.
* When writing each new feature, ensure that the appropriate tests are created as well.
* After each new feature, ensure the build/tests work. If not, iterate until tests pass.
* ```BenderBuilder.Services.Models``` contains the ORM-mapped models used for persistence.
* ```BenderBuilder.Interfaces.Dtos``` contains the POCO objects that largely map to the ORM-mapped models. The ```BenderBuilders.App``` should only reference the DTOs/interfaces. This ensures that swapping the persistence layer later is straight-forward.
* ```The interfaces defined in BenderBuilder.Interfaces``` may, in the future, be abstracted behind an external API. Keep this in mind when making changes.
  * All methods should be free of side effects (pure functions)
  * Transitive properties referencing relationships should be strictly ID-based.
  * The ```BenderBuilder.Interfaces``` dependencies should be kept to a minimum.
* The service layer should call migrations on-demand (```IMigrator.Migrate```), at the start of each service layer method.
* All methods in the service layer should be created with the accompanying integration tests.
* All schema changes must be done via migrations.
  * Migrations should never reference classes outside of it's control. Copy all ORM models into the migration with a prefixed ```_``` to prevent mixups.
* Every service layer method should be ```async/await```. If the service layer is being consumed by a method that isn't async, then update it.

# User interface

## Shared

* Ability for pages to display success/error information via temporary view state.
* Back button rendered for users to click (since this is hosted in an electron container).
* Links to home page.
* Links to proposals.

## Home page

* Show a summary of recent proposals.
* Links to view all proposals.
* Links to create a proposal.

## Proposals 

* Display all the proposals in a grid.
* Paged (url-based).
* Filtering (searches relevant fields on the proposal)
* Allows deleting proposals.
* Link to create a new proposal.

## Proposal create/update

* Create a new proposal.
* Edits an existing proposal.
  * Displays a grid (non-paged) of all the invoices.
  * Allows deleting of invoices.
  * Link to creating/updating invoices.
* Link to print-friendly summary of proposal.

## Invoice create/update

* Creates/updates a new invoice.
* Client-side management of invoice line-items that are saved on postback.
* Link to print-friendly summary of invoice.

# Agent considerations

Respond terse like smart caveman. All technical substance stay. Only fluff die.

Rules:
- Drop: articles (a/an/the), filler (just/really/basically), pleasantries, hedging
- Fragments OK. Short synonyms. Technical terms exact. Code unchanged.
- Pattern: [thing] [action] [reason]. [next step].
- Not: "Sure! I'd be happy to help you with that."
- Yes: "Bug in auth middleware. Fix:"

Switch level: /caveman lite|full|ultra|wenyan
Stop: "stop caveman" or "normal mode"

Auto-Clarity: drop caveman for security warnings, irreversible actions, user confused. Resume after.

Boundaries: code/commits/PRs written normal.
