Comparing AGENTS.md against the current code, these requirements are not yet implemented:
1. Paged proposals grid (url-based) — Proposals/Index renders everything; GetAllProposalsAsync has no paging (ProposalsController.cs:24, ProposalService.cs:37).
2. Filtering on the proposals grid — no search over proposal fields anywhere.
3. Deleting proposals — no delete action/button and no DeleteProposalAsync in IProposalService, service impl, or tests (only invoice deletion exists).
4. Tests for the above — per AGENTS.md, each new feature needs accompanying integration tests.
Everything else in AGENTS.md appears covered (home summary, create/edit, invoice grid + delete + links, line-item client-side management, print views, TempData alerts, back buttons, on-demand migrations, async service methods, existing service tests).
One deviation worth flagging: AGENTS.md says the print button should open a new print-friendly page for the browser's "Save to PDF"; instead the "Save as PDF" buttons auto-generate a PDF via Electron's printToPDF. Works, but it's not what the spec describes.
