# Test plan — UC-02

## Automated tests

Test the merchant carousel utilities for:

- 0, 1, 8 and more than 8 merchants.
- Full and partial case-insensitive name matching.
- Zero search results.
- Recent merchants before alphabetical fallback.
- Updating recent merchant history without duplicates.
- Invalid stored history.
- ArrowLeft, ArrowRight, Home and End scroll targets.
- Unsupported keys causing no carousel action.

## Build verification

- Compile the Angular production build.
- Run the Angular unit-test suite non-interactively when the available CI environment supports Chrome Headless.
- Review the pull-request diff for unrelated changes.

## Manual checks

- Confirm the carousel remains inside the narrow mobile content width.
- Confirm swipe and mouse scrolling still work.
- Confirm keyboard focus is visible and arrow keys scroll the carousel.
- Confirm selecting a merchant opens wizard step 1 with its real ID.
- Confirm direct wizard access without merchant state redirects home.
- Confirm logout is absent from home and available on profile.
