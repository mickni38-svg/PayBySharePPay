# 12 – Test og Kvalitet

## Testprojekter

| Testprojekt | Formål | Kommando |
|---|---|---|
| `Tests.PayBySharePay` | Unit tests | `dotnet test tests\Tests.PayBySharePay` |

---

## Kør tests

```powershell
# Kør alle tests
dotnet test

# Kør med verbose output
dotnet test --verbosity normal

# Kør via Visual Studio
# → Test Explorer → Kør alle tests
```

---

## Teststatus

> ⚠️ **Testdækningen er lav i den nuværende implementation.**

Testprojektet (`Tests.PayBySharePay`) eksisterer, men der er begrænset antal tests. Der er ingen integration tests eller end-to-end tests pt.

---

## Mangler i testdækning

| Område | Status | Anbefaling |
|---|---|---|
| `OrderService` unit tests | ❌ Mangler/sparsomt | Tilføj tests for `CreateOrderAsync`, `GetOrderOverviewAsync` |
| `PaymentService` unit tests | ❌ Mangler | Test betalingsregistrering og validering |
| `AuthController` integration tests | ❌ Mangler | Test login/register flow |
| `MerchantOrdersController` tests | ❌ Mangler | Test token-validering |
| API integration tests (in-memory DB) | ❌ Mangler | End-to-end API test med `WebApplicationFactory` |
| Angular frontend tests | ⚠️ Ukendt | Angular genererer `.spec.ts`-filer – tjek testdækning |
| MerchantDemo tests | ❌ Mangler | Ingen testinfrastruktur til vanilla JS |

---

## Anbefalede næste trin for testdækning

1. **Unit tests til services** med mocked repositories (brug `Moq` eller `NSubstitute`)
2. **Integration tests** via `WebApplicationFactory<Program>` og in-memory SQLite
3. **Angular unit tests** med Jasmine/Karma (allerede scaffoldet)
4. **CI test pipeline** via GitHub Actions der kører `dotnet test` ved pull request

---

## Kodekvalitet

| Område | Status |
|---|---|
| Code review proces | ⚠️ Ikke formaliseret |
| Static analysis / linting | ⚠️ Ikke konfigureret |
| Angular lint | ⚠️ Tjek `ng lint` |
| Swagger dokumentation | ✅ Tilgængeligt i Development |
| XML documentation comments | ⚠️ Delvist på controllers |

---

## Se også

- [Backend](04-backend.md)
- [Fejlfinding](13-fejlfinding.md)
