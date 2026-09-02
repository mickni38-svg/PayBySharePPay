# Implementation plan — UC-02

## Scope

Complete the remaining acceptance criteria in `docs/usecases/UC-02-home-merchant-carousel.md`.

## Current implementation

The home page already loads merchant friends dynamically, limits the carousel to eight cards, supports case-insensitive name search, displays logos with initials fallback, transfers the merchant ID to the wizard, and redirects invalid direct wizard navigation to the home page.

## Changes

1. Store selected merchant IDs per logged-in user in `localStorage`.
2. Sort merchant friends by the stored recent-use order and then alphabetically.
3. Add keyboard navigation (ArrowLeft, ArrowRight, Home and End) to the carousel.
4. Keep the carousel viewport contained within the page content width.
5. Remove username/logout controls from the home page development panel.
6. Add the existing logout action to the profile page and navigate to login after logout.
7. Add focused unit tests for filtering, limits, recent ordering and keyboard scroll targets.
8. Update UC-02 and current-state documentation after verification.

## Impact

- Frontend only.
- No API changes.
- No database or migration changes.
- No payment or external integration changes.
- No dependency or deployment changes.

## Compatibility and risks

Existing users without recent-merchant data see alphabetical ordering. Recent ordering is device/browser-local and is isolated by participant ID. Invalid or unavailable local storage falls back safely to alphabetical ordering.
