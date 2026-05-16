# Data og State

## Database-entities (backend)

| Entity | Tabel | Centrale felter |
|--------|-------|-----------------|
| `Participant` | Participants | Id, Name, Email, Phone, Type (Person/Merchant) |
| `Order` | Orders | Id, Title, Category, Status, CreatedByParticipantId, MerchantParticipantId, JoinToken, CreatedAt |
| `OrderParticipant` | OrderParticipants | OrderId, ParticipantId, Status (Invited/Accepted/Paid), Type (Host/Guest/Merchant) |
| `Payment` | Payments | Id, OrderId, ParticipantId, Amount, Status |
| `MerchantOrderDraft` | MerchantOrderDrafts | Id, OrderId, MerchantParticipantId, TotalAmount, Status, ExpiresAtUtc |
| `MerchantOrderLine` | MerchantOrderLines | Id, DraftId, ParticipantId, Name, Quantity, UnitPrice |
| `Message` | Messages | Id, OrderId, ParticipantId, Content, CreatedAt |
| `FriendRelation` | FriendRelations | Id, ParticipantId, FriendParticipantId |

### Ordrestatus-værdier
`Collecting` → `Ready` → `Completed` / `Cancelled`

### OrderParticipant-status-værdier
`Invited` → `Accepted` → `Paid`

---

## DTOs – service-lag

| DTO | Bruges til |
|-----|-----------|
| `CreateOrderDto` | Opret ordre |
| `OrderDto` | Returnér ordre |
| `OrderSummaryDto` | Kortfattet ordrevisning (liste) |
| `OrderOverviewDto` | Fuld ordredetalje inkl. deltagere og betalinger |
| `CreatePersonDto` | Opret person |
| `CreateMerchantDto` | Opret merchant |
| `RegisterPaymentDto` | Registrér betaling |
| `PaymentDto` | Returnér betaling |
| `InitMerchantOrderDto` | Opret merchant-draft |
| `MerchantOrderDraftDto` | Returnér draft |
| `DirectoryEntryDto` | Søgeresultat i telefonbog |
| `MessageDto` | Besked |

---

## Frontend-modeller (Angular)

| Model/Interface | Fil | Formål |
|-----------------|-----|--------|
| `OrderSummaryApiDto` | `order.model.ts` | Ordre fra API |
| `OrderParticipantApiDto` | `order.model.ts` | Deltager på ordre |
| `ActivityItem` | `activity.model.ts` | Aktivitetshændelse |
| `ActivityFeed` | `activity.model.ts` | Samling af aktiviteter + unreadCount |
| `DirectoryEntry` | `directory.model.ts` | Bruger i telefonbog |
| `MessageModel` | `message.model.ts` | Besked |
| `PaymentModel` | `payment.model.ts` | Betaling |
| `FriendModel` | `friend.model.ts` | Ven/kontakt |

---

## State management (frontend)

Angular **signals** bruges til lokal komponentstate og service-state.

| Signal | Placering | Hvad den holder |
|--------|-----------|-----------------|
| `_token` | `AuthService` | JWT token fra localStorage |
| `_user` | `AuthService` | `{ participantId, name }` fra localStorage |
| `isLoggedIn` | `AuthService` | Computed: er bruger logget ind? |
| `currentUserId` | `AuthService` | Computed: brugerens participant-id |
| `statusCards` | `HomeComponent` | Aktuelle statuskort |
| `loading` | diverse komponenter | Loading state |
| `error` | diverse komponenter | Fejltilstand |

Der er **ingen global state management** (ingen NgRx, ingen Akita). Hver komponent henter sine egne data via services.

---

## Persistens

| Data | Lager |
|------|-------|
| JWT token | `localStorage` (nøgle: `sbys_token`) |
| Brugerinfo (participantId, name) | `localStorage` (nøgle: `sbys_user`) |
| Alle ordrer, betalinger, brugere | Azure SQL / lokal SQL Server via EF Core |
| Aktivitetsfeed | Bygges client-side fra ordredata – gemmes ikke |

---

## Kendte begrænsninger

- Aktivitetsfeed er ikke en rigtig log – den udledes fra den aktuelle tilstand af ordredata. Historiske hændelser der ikke afspejles i nuværende data vises ikke.
- `unreadCount` i `ActivityFeed` baseres på aktiviteter inden for de seneste 24 timer – ikke reel læst/ulæst-tracking.
- Ingen offline-support eller caching i frontend.
- `JoinToken` på `Order` er genereret men bruges ikke i noget flow.
