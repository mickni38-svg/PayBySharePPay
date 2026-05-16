# Features

## Oversigt

| Feature | Status |
|---------|--------|
| Login (email-kun) | ✅ Implementeret |
| Registrering (person) | ✅ Implementeret |
| Registrering (merchant) | ✅ Implementeret |
| Forside med statuskort | ✅ Implementeret |
| Opret ordre | ✅ Implementeret |
| Overblik over ordrer | ✅ Implementeret |
| Ordredetaljer | ✅ Implementeret |
| Inviter deltagere til ordre | ✅ Implementeret |
| Merchant-draft system | ✅ Implementeret |
| Betalingsregistrering | ✅ Implementeret |
| Afventende deltagere (pending flow) | ✅ Implementeret |
| Seneste aktivitet (aktivitetsfeed) | ✅ Implementeret |
| Beskeder på ordre | ✅ Implementeret |
| Brugersøgning / telefonbog | ✅ Implementeret |
| Venneliste | ✅ Implementeret |
| JWT authentication | ✅ Implementeret |
| Password / rigtig auth | ❌ Ikke implementeret |
| Push-notifikationer | ❌ Ikke implementeret |
| Læst/ulæst status på aktiviteter | ⚠️ Delvist (TODO i kode) |
| Invitations-flow via link/token | ⚠️ Delvist (`JoinToken` findes på `Order`, ingen UI) |

---

## Feature-beskrivelser

### Login (email-kun)
Brugeren logger ind med email. API'et slår emailen op i `Participants`-tabellen og returnerer en JWT-token. Ingen password-validering.

**Filer:**
- `Api/Controllers/AuthController.cs`
- `Service/Services/ParticipantService.cs`
- `Frontend/features/login/login.component.ts`
- `Frontend/core/services/auth.service.ts`

---

### Registrering
Ny person eller merchant kan oprettes. Person kræver navn og email. Merchant kræver yderligere firmaoplysninger.

**Filer:**
- `Api/Controllers/AuthController.cs` (register-endpoints)
- `Frontend/features/register/register.component.ts`

---

### Forside med statuskort
Forsiden viser to dynamiske statuskort:
1. **X deltagere afventer** – antal deltagere på tværs af ordrer der mangler at gøre noget
2. **Seneste aktivitet** – viser om der er sket noget nyt siden sidst (bygget client-side fra ordredata)

**Filer:**
- `Frontend/features/home/home.component.ts`
- `Frontend/features/home/home.component.html`
- `Frontend/core/services/activity.service.ts`
- `Frontend/core/models/activity.model.ts`

---

### Opret ordre
Vært opretter en ordre med titel, kategori, besked og inviterede deltagere. Valgfrit: tilknyt en merchant.

**Filer:**
- `Frontend/features/create-order/create-order.component.ts`
- `Api/Controllers/OrdersController.cs`
- `Service/Services/OrderService.cs`

---

### Overblik over ordrer
Liste over alle ordrer hvor brugeren er deltager. Viser status, merchant og deltagere.

**Filer:**
- `Frontend/features/orders/orders.component.ts`
- `Api/Controllers/OrdersController.cs`

---

### Ordredetaljer
Detaljeret visning af én ordre inkl. deltagerstatus, betalingsstatus og ordrelinjer fra merchant.

**Filer:**
- `Frontend/features/order-detail/order-detail.component.ts`
- `Api/Controllers/OrdersController.cs` (`GET /api/orders/{id}/overview`)

---

### Merchant-draft system
En merchant kan oprette en bestillingsoversigt (draft) med ordrelinjer (retter, priser, antal) tilknyttet en bestemt ordre. Deltagere kan se ordrelinjer og hvad de skylder.

**Filer:**
- `Api/Controllers/MerchantOrdersController.cs`
- `Service/Services/MerchantOrderService.cs`
- `DataStorage/Entities/MerchantOrderDraft.cs`
- `DataStorage/Entities/MerchantOrderLine.cs`

---

### Betalingsregistrering
Betalinger registreres med beløb og sættes til status `Completed`. Opdaterer deltagerens status på ordren.

**Filer:**
- `Api/Controllers/PaymentsController.cs`
- `Service/Services/PaymentService.cs`
- `DataStorage/Entities/Payment.cs`

---

### Afventende deltagere
Oversigt på tværs af alle ordrer over deltagere der mangler at betale eller vælge bestilling. Afsender kan sende påmindelser.

**Filer:**
- `Frontend/features/pending-participants/pending-participants.component.ts`
- `Frontend/core/models/order.model.ts` (`computePendingSummary()`)

---

### Seneste aktivitet
Aktivitetskort på forsiden og separat aktivitetsside der viser seneste hændelser (ordrestatusændringer, betalinger, deltagerhandlinger). Aktiviteter genereres client-side fra ordredata – der er ingen dedikeret aktivitetslog i backend.

**Filer:**
- `Frontend/features/activity/activity.component.ts`
- `Frontend/core/services/activity.service.ts`
- `Frontend/core/models/activity.model.ts`

**TODO:** Læst/ulæst-status er ikke implementeret. Se kommentar i `activity.component.ts`.

---

### Beskeder
Beskedtråd per ordre.

**Filer:**
- `Frontend/features/messages/messages.component.ts`
- `Api/Controllers/MessagesController.cs`
- `Service/Services/MessageService.cs`

---

### Brugersøgning / telefonbog
Søg efter andre brugere for at tilføje dem som deltagere eller venner.

**Filer:**
- `Frontend/features/find-participants/find-participants.component.ts`
- `Api/Controllers/DirectoryController.cs`
- `Service/Services/DirectoryService.cs`

---

### Venneliste
Tilføj og se venner/kontakter.

**Filer:**
- `Api/Controllers/FriendsController.cs`
- `DataStorage/Repositories/FriendRelationRepository.cs`

---

### JWT authentication
Alle API-endpoints (undtagen login/register) kræver `Authorization: Bearer <token>`. Frontend-interceptor tilføjer token automatisk.

**Filer:**
- `Api/Auth/JwtTokenService.cs`
- `Api/Program.cs`
- `Frontend/core/interceptors/api.interceptor.ts`
