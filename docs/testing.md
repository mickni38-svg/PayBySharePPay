# Test

## Testprojekter

| Projekt | Framework | Placering |
|---------|-----------|-----------|
| `Tests.PayBySharePay` | xUnit | `src/Tests.PayBySharePay/` |

## Aktuel teststatus

**Testprojektet er næsten tomt.**

`UnitTest1.cs` indeholder én tom test:
```csharp
public class UnitTest1
{
	[Fact]
	public void Test1()
	{
		// tom
	}
}
```

Der er **ingen meningsfulde tests** i projektet på nuværende tidspunkt.

## Områder der ikke er dækket

- Ingen unit tests for service-laget (`OrderService`, `PaymentService`, osv.)
- Ingen unit tests for `ActivityService` (client-side aktivitetsfeed-logik)
- Ingen integrationstests for API-controllers
- Ingen tests for `computePendingSummary()` i `order.model.ts`
- Ingen end-to-end tests (Playwright, Cypress eller lignende)

## Hvordan køres tests

```bash
cd src/Tests.PayBySharePay
dotnet test
```

## Forslag til næste tests der bør skrives

### Høj prioritet

1. **`PaymentService.RegisterPaymentAsync`**
   - Test at betaling med beløb <= 0 kaster `ArgumentException`
   - Test at betaling på ikke-eksisterende ordre kaster `KeyNotFoundException`
   - Test happy path

2. **`OrderService.CreateOrderAsync`**
   - Test oprettelse med gyldige data
   - Test at ugyldige participant-ids håndteres

3. **`computePendingSummary()` (TypeScript)**
   - Test at korrekt antal afventende deltagere beregnes
   - Test edge cases: ingen ordrer, alle betalt, kun host

4. **`ActivityService.buildFeed()` (Angular)**
   - Test at ordredata mappes korrekt til `ActivityItem[]`
   - Test at `unreadCount` beregnes korrekt

### Medium prioritet

5. **Integrationstest for `AuthController.Login`**
   - Test at ukendt email returnerer 401
   - Test at known email returnerer valid JWT

6. **Integrationstest for `OrdersController`**
   - Test at uautoriseret request returnerer 401

## Anbefalinger

- Tilføj `Moq` eller `NSubstitute` til mocking af repositories
- Overvej `Microsoft.AspNetCore.Mvc.Testing` til integrationstests af API
- Overvej Playwright til E2E-tests af Angular-frontend
