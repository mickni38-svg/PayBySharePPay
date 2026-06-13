# Copilot Instructions

## Project Guidelines

* Kun push kode til git (git push) når brugeren eksplicit beder om det. Commit må gerne laves, men push skal afvente brugerens instruktion.
* Opret kun tests hvis brugeren beder om det

# Project Development Workflow

For new features or user stories:

1. Analyze the existing solution.
2. Identify impacted projects and files.
3. Create an implementation plan.
4. Create a test plan when relevant.
5. Present the analysis and plan before making significant code changes.
6. Then implement the solution.

Implementation Rules

* Follow existing architecture.
* Reuse existing patterns and services.
* Avoid unnecessary abstractions.
* Avoid large refactorings.
* Keep changes focused on the requested feature.
* Avoid doing changes to files that are not directly related to the feature.

Do not create plans for small bug fixes, text changes, configuration updates, or trivial refactorings.

---

# PayNSync Development Workflow

Før du implementerer en feature:

1. Identificér hvilken use case der arbejdes på.
2. Læs relevante dokumenter under `/docs`.
3. Læs use case-dokumentationen under `docs/usecases/`.
4. Forklar opgaven og den planlagte løsning.
5. Vent på accept før implementering.
6. Implementér løsningen.
7. Kør build og tests.
8. Ret eventuelle fejl.
9. Opdater `docs/current-state.md` og den relevante `docs/usecases/UC-XX-*.md` hvis funktionaliteten er ændret.

---

# PayNSync – Solution Knowledge

## What the system does
PayNSync is a group payment platform. A Host creates a group order at a Merchant, invites Participants, each participant orders via the merchant's ordering page (using a unique ParticipantToken link), and the Host approves to capture everyone's payment via MobilePay/Vipps.

## Projects
| Project | Role |
|---------|------|
| `Api.PayBySharePay` | ASP.NET Core 9 Web API — controllers, JWT auth, middleware |
| `Service.PayBySharePay` | Business logic, interfaces, DTOs |
| `DataStorage.PayBySharePay` | EF Core, entities, repositories, migrations (SQL Server) |
| `Infrastructure.Payments.PayBySharePay` | `IPaymentProvider` implementations: `FakePaymentProvider` + `MobilePaySandboxPaymentProvider` (Vipps) |
| `Tests.PayBySharePay` | xUnit unit tests (in-memory fakes, no EF InMemory) |
| `Frontend.PayBySharePay` | Angular 19 SPA (standalone components, signals) |
| `Frontend.MerchantDemo` | Static HTML merchant ordering demo (Pizzeria Roma) |

## Naming conventions
* Domain names: **Order**, **OrderParticipant**, **Participant** — NOT GroupPayment/GroupPaymentMember/UserEntity.
* Amounts in `ParticipantPayment`: `AmountMinorUnits` (long, øre). Amounts in `Payment` and `MerchantOrderDraft`: decimal (kr).
* Currency defaults to `"DKK"`.

## Core domain objects
* `Participant` — Person or Merchant (single table, `ParticipantType` enum)
* `Order` — group order, created by Host, optional `MerchantParticipantId`
* `OrderParticipant` — join record with `Status` and `ParticipantToken` (unique GUID)
* `ParticipantPayment` — provider-backed payment per participant per order (`ParticipantPaymentStatus` enum)
* `MerchantOrderDraft` + `MerchantOrderLine` — what a participant ordered at the merchant
* `PaymentEventLog` — immutable audit trail for every payment state transition

## Key services
* `GroupPaymentOrchestrationService` — reserve + capture + cancel via `IPaymentProvider`
* `OrderService` — create orders, participant invitations, `CheckAndSetReadyToPayAsync`
* `MerchantOrderService` — validate `ParticipantToken`, create drafts, trigger ReadyToPay
* `ParticipantPaymentStateService` — owns all `ParticipantPayment` state transitions + event logging

## Order status machine
`Collecting → ReadyToPay → HostApproved → Capturing → Paid | PartiallyFailed`  
Cancel: any non-Paid status → `Cancelled`  
Legacy path: `ReadyToPay → Completed` (via `/complete` or `/pay` endpoints)

## Payment status machine (ParticipantPaymentStatus)
`Created → ReservationStarted → Reserved → CapturePending → Captured`  
Failure paths: `ReservationFailed`, `CaptureFailed`, `Cancelled`, `Expired`, `Refunded`

## API authentication
* `OrdersController` — `[Authorize]` (JWT required)
* `POST /api/merchant-orders` — `[AllowAnonymous]` (merchant websites call this)
* `POST /api/payments/webhooks/*` — `[AllowAnonymous]` (no signature validation yet)
* `ParticipantsController`, `FriendsController`, `MessagesController`, `DirectoryController` — no `[Authorize]` at class level

## Payment provider selection
Controlled by `Payments:Provider` in `appsettings.json`: `"Fake"` (default) or `"MobilePay"`.  
Switch via `AddPaymentInfrastructure(config)` in `PaymentInfrastructureExtensions`.

## Frontend routes (Angular)
`/home`, `/orders`, `/orders/create`, `/orders/:id`, `/messages`, `/profile`, `/pending-participants`, `/find-participants`, `/login`, `/register`

## Known open questions / gaps
1. No endpoint to join an order by `JoinToken` (generated but unused)
2. No endpoint to add participants after order creation
3. `Declined` participant status has no backend flow
4. Webhook signature validation not implemented
5. `ExternalPaymentService` (used in `/pay`) always returns success — legacy stub
6. `ParticipantsController` has no auth — may be intentional for public search

## Documentation files
* `docs/project-overview.md` — what the system is, projects, tech stack, deployed URLs
* `docs/architecture.md` — layering diagram, controllers, services, data model
* `docs/business-rules.md` — all business rules derived from code
* `docs/glossary.md` — domain term definitions
* `docs/current-state.md` — feature-by-feature ✅/⚠️/❌ implementation status
* `docs/flows.md` — step-by-step description of all major flows
* `docs/usecases/UC-XX-navn.md` — one file per use case (see format below)

## Use case format (`docs/usecases/`)
Each use case follows this structure and naming:
* File: `UC-{id}-{kebab-navn}.md` — e.g. `UC-01-opret-bruger.md`
* Sections: Overblik, Aktører, Prækonditioner, Postkonditioner, Normalforløb, Alternative forløb, Undtagelsesforløb, Datamodel, API-endpoints, Implementeringsstatus, Kendte mangler og gaps, Tekniske noter, Relaterede use cases
* Status per implementeringsdel: ✅ implementeret / ⚠️ delvist / ❌ ikke implementeret
* Gaps dokumenteres med prioritet: 🔴 Høj / 🟡 Medium / 🟢 Lav
* Use cases reverse-engineeres fra kodebasen — ikke fra ønsker

### Oprettede use cases
* `docs/usecases/UC-01-opret-bruger.md` — Registrering af Person og Merchant
* `docs/usecases/UC-02-log-ind.md` — Login med e-mail og password
* `docs/usecases/UC-03-log-ud.md` — Manuel og automatisk logout
* `docs/usecases/UC-04-opdater-profil.md` — Rediger navn, e-mail og telefon
* `docs/usecases/UC-05-find-deltagere-tilfoj-ven.md` — Katalog-søgning og vennehåndtering
* `docs/usecases/UC-06-opret-ordre.md` — Opret gruppeordre og invitér deltagere
* `docs/usecases/UC-07-se-ordrer-og-overblik.md` — Se ordreliste og ordreoverblik
* `docs/usecases/UC-08-bestil-via-merchant-link.md` — Deltager bestiller via merchant-link
* `docs/usecases/UC-09-reserver-betaling.md` — Reserver betaling hos payment provider
* `docs/usecases/UC-10-godkend-og-capture.md` — Host godkender og capture'r alle betalinger
* `docs/usecases/UC-11-annuller-ordre.md` — Host annullerer ordre og frigiver reservationer
* `docs/usecases/UC-12-beskeder.md` — Se og sende beskeder, ulæst tæller
* `docs/usecases/UC-13-payment-webhook.md` — Modtag async betalingsstatus fra provider
* `docs/usecases/UC-14-legacy-betaling.md` — Manuelt betalingsflow (stub, pre-provider)
* `docs/usecases/UC-15-dev-og-seed-tools.md` — DevController og seed-CLI til testdata

