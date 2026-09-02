# PayNSync – use cases og implementeringsrækkefølge

## Formål

Mappen er PayNSyncs nummererede feature-backlog. Implementér kun én use case ad gangen. Hver ny opgave skal følge repoets analyse-, plan-, approval-, test- og review-workflow.

## Implementeringsprofil for UC-08–UC-15

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- Use casene er bevidst opdelt i små, sammenhængende vertical slices.
- Betaling, sikkerhed, public API, database og secrets har obligatorisk approval gate.
- Modellen må ikke udvide scope, opfinde forretningsregler eller installere dependencies uden godkendelse.

## Afsluttet redesign

1. ✅ `UC-01-merchant-logo.md`
2. ✅ `UC-02-home-merchant-carousel.md`
3. ✅ `UC-03-wizard-step-1-participants.md`
4. ✅ `UC-04-wizard-step-2-details.md`
5. ✅ `UC-05-wizard-step-3-review-create.md`

UC-07 om dark-theme-navigation er allerede implementeret og registreret i `docs/current-state.md`. Der findes ingen aktiv UC-06-fil.

## Ny backlog – anbefalet rækkefølge

1. `UC-08-jwt-identitet-og-host-autorisation.md`  
   Fjern tillid til bruger-ID fra request-body og brug JWT-identiteten.
2. `UC-09-beskyt-dev-endpoints.md`  
   Fjern destruktive udvikler-ruter fra Simply/produktion.
3. `UC-10-vipps-webhook-hmac.md`  
   Verificér Vipps Webhooks API HMAC før statusændringer. Kræver webhook-secret og ekstern integrationsafklaring.
4. `UC-11-ens-jwt-udloebstid.md`  
   Fjern forskellen mellem tokenets `exp` og auth-responsens `ExpiresAt`.
5. `UC-12-send-paamindelser.md`  
   Erstat frontend-placeholder med rigtige beskeder til afventende deltagere.
6. `UC-13-join-gruppeordre-med-token.md`  
   Aktivér det eksisterende JoinToken efter Product Owner-beslutninger.
7. `UC-14-refunder-captured-betaling.md`  
   Tilføj idempotent refund efter særskilt betalings- og rollebeslutning.
8. ✅ `UC-15-profil-og-kontocenter.md`
   Saml profil, login, person-/merchantregistrering, Vipps-testmapping og development-only værktøjer i et rollebaseret kontocenter.

UC-15 er implementeret uafhængigt af UC-10–UC-14 efter særskilt approval af ændringerne til merchant-login og auth-kontrakten.

## Fælles arbejdsgang

1. Læs den valgte use case og klassificér opgaven.
2. Følg det matchende workflow i `.ai/workflows`.
3. Sammenhold use casen med kode, tests, `docs/current-state.md`, arkitektur og forretningsregler.
4. Opret/opdatér `implementation-plan.md` og `test-plan.md`.
5. Præsenter påvirkede lag, risici, API/database/security/payment-impact og forventede filer.
6. Vent på godkendelse, når repo-instruktionerne kræver det.
7. Implementér kun den valgte use case.
8. Kør relevante builds/tests og udfør en separat review-pass.
9. Opdatér kun berørt dokumentation efter verificeret implementering.

## Fælles regler

- Genbrug eksisterende services, repositories, state machine, DTO'er, Angular-mønstre og testinfrastruktur.
- Ingen live Vipps, Google eller andre betalte/eksterne kald i automatiske tests.
- Ingen secrets i kode, frontend, logs eller dokumentation.
- Ingen nye dependencies uden eksplicit godkendelse.
- Betalingsstatus ændres kun gennem `ParticipantPaymentStateService`.
- Bevar idempotens i betaling, webhook, reminder, join og retry.
- Merchant Demo må aldrig indeholde Vipps-credentials eller kalde Vipps direkte.
- Afslut med ændrede filer, testresultater, uverificerede forhold og resterende begrænsninger.
