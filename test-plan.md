# Test plan — UC-19

## Scope

- Use case: `docs/usecases/UC-19-samlet-merchant-ordrekontrakt.md`
- Changed behaviour: Efter fuld capture opretter PayNSync én permanent, samlet merchant-ordre og callback uden deltageridentitet eller betalingsreferencer.

## Automated tests

| Scenario | Level | Expected result |
|---|---|---|
| Alle ordrepersoner har captured betaling | Unit/service | Én permanent merchant-ordre oprettes |
| En ordreperson mangler captured betaling | Unit/service | Ingen merchant-ordre oprettes |
| Finalisering kaldes flere gange | Unit/service | Samme merchant-ordre returneres; ingen dublet |
| Hostdata og leveringsadresse | Unit/service | Navn, telefon og ordre-adresse kopieres som snapshot |
| Flere deltager-drafts | Unit/service | Alle linjer kopieres fladt uden participant-data |
| Ens produkter som separate linjer | Unit/service | Linjerne bevares separat |
| Linjesum svarer til captured beløb | Unit/service | Total og items gemmes korrekt |
| Linjesum afviger fra captured beløb | Unit/service | Finalisering afvises |
| Forskellige captured valutaer | Unit/service | Finalisering afvises |
| Draft tilhører en anden merchant | Unit/service | Validering afvises før capture |
| Draft-valuta afviger fra betalingsvaluta | Unit/service | Validering afvises før capture |
| Partial capture failure | Orchestration unit | Ingen finalisering og ingen callback |
| Successful capture | Orchestration unit | Finalisering sker før callback |
| Allerede `Paid` ordre | Orchestration unit | Merchant-ordren sikres idempotent uden nyt provider-capture |
| Callback-payload | Unit | Indeholder samlet ordre, host, levering og flade linjer; ingen participant/payment-referencefelter |

## Manual verification

| Step | Expected result |
|---|---|
| Gennemgå migration | Nye tabeller, relationer, decimal precision og unique source-order index er korrekte |
| Gennemgå entity/DTO | Ingen participant-id, participant-navn, token eller provider-reference på final merchant-ordre |
| Gennemgå orchestration | Finalisering nås kun efter alle captures er lykkedes |
| Gennemgå diff | Ingen Angular-, provider-, auth-, secret- eller deploymentændringer |

## Regression areas

- Reserve og capture state machine.
- Retry efter `PartiallyFailed`.
- Merchant callback og dev callback-store.
- UC-18 delivery-address snapshot.
- Fake-provider end-to-end flow.

## Environment/configuration

- Brug eksisterende xUnit-fakes; ingen EF InMemory og ingen nye dependencies.
- Ingen live Vipps/MobilePay eller HTTP-callbacks i tests.
- Kør `dotnet build PayBySharePay.sln --configuration Release`.
- Kør `dotnet test PayBySharePay.sln --configuration Release --no-build --verbosity normal`.

## Not tested

- Order Hub UI/PWA.
- Produktionsdatabase-deploy.
- Automatisk retry/outbox efter databasefejl (UC-24).
- Merchant-specifik API-mapping (UC-21).
