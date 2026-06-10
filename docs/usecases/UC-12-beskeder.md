# UC-12 — Beskeder

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-12 |
| Navn | Beskeder |
| Primær aktør | Logget-ind bruger |
| Formål | Se, sende og markere beskeder som læst |
| Trigger | Bruger navigerer til `/messages` eller beskeder modtages via systemhændelser |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Bruger** | Modtager og kan sende beskeder |
| **System** | Opretter automatisk beskeder ved ordre- og betalingshændelser |
| **API** | `MessagesController` — ingen auth |

---

## Prækonditioner

- Bruger er logget ind (`currentUserId` fra `AuthService`).

---

## Postkonditioner

- Beskeder er hentet og vist.
- Ulæste beskeder er markeret som læst.

---

## Normalforløb — Se beskedindbakke

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Navigerer til `/messages` |
| 2 | Frontend | `GET /api/messages/by-participant/{currentUserId}` |
| 3 | API | `MessageService.GetByParticipantAsync(participantId)` |
| 4 | API | Returnerer `IEnumerable<MessageDto>` |
| 5 | Frontend | Viser alle beskeder — nyeste øverst |
| 6 | Frontend | `POST /api/messages/mark-read?participantId={currentUserId}` |
| 7 | API | `MessageService.MarkAllReadAsync(participantId)` |

---

## Normalforløb — Hent ulæst tæller (badge)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Frontend | `GET /api/messages/unread-count?participantId={currentUserId}` |
| 2 | API | Returnerer antal ulæste beskeder som `int` |
| 3 | Frontend | Viser badge i navigation (bottom nav) |

---

## System-genererede beskeder

Systemet opretter automatisk `Message`-records ved følgende hændelser:

| Hændelse | Modtager | Indhold |
|----------|----------|---------|
| Ordre oprettet med merchant | Alle deltagere inkl. host | Bestillingslink med `ParticipantToken` |
| Ordre oprettet uden merchant | Inviterede deltagere (ikke host) | Generel invitationsbesked |
| Alle deltagere har bestilt (`ReadyToPay`) | Host | Link til `/orders` med opfordring til at godkende |
| Betaling registreret (legacy) | Host | Bekræftelse via `PaymentService` |

---

## Normalforløb — Send manuel besked

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | `POST /api/messages` med `{ orderId, participantId, content }` |
| 2 | API | `MessageService.CreateMessageAsync(dto)` |
| 3 | Service | Opretter `Message`-record |
| 4 | API | Returnerer `HTTP 201` med `MessageDto` |

---

## Datamodel

### `MessageDto`
| Felt | Indhold |
|------|---------|
| `id` | Besked-ID |
| `orderId` | Tilknyttet ordre |
| `participantId` | Modtager |
| `participantName` | Modtagerens navn |
| `content` | Beskedtekst (fri tekst, kan indeholde URL) |
| `createdAt` | Tidsstempel |
| `isRead` | bool |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `GET /api/messages/by-participant/{id}` | GET | Anonym | 200 + `IEnumerable<MessageDto>` |
| `GET /api/messages/order/{orderId}` | GET | Anonym | 200 + `IEnumerable<MessageDto>` |
| `GET /api/messages/unread-count?participantId={id}` | GET | Anonym | 200 + `int` |
| `POST /api/messages/mark-read?participantId={id}` | POST | Anonym | 204 |
| `POST /api/messages` | POST | Anonym | 201 + `MessageDto` |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — beskedindbakke `/messages` | ✅ | |
| Frontend — ulæst-tæller i navigation | ✅ | |
| Frontend — markér alle læst ved åbning | ✅ | |
| API — alle CRUD-endpoints | ✅ | |
| System-besked ved ordreoprettelse | ✅ | `OrderService` |
| System-besked ved `ReadyToPay` | ✅ | `OrderService.CheckAndSetReadyToPayAsync()` |
| System-besked ved betaling (legacy) | ✅ | `PaymentService` |
| `[Authorize]` på endpoints | ❌ | Alle `MessagesController`-endpoints er anonyme |
| Real-time beskeder (push/WebSocket) | ❌ | Ingen SignalR eller polling |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **Ingen `[Authorize]` på besked-endpoints** | 🔴 Høj | Alle endpoints er anonyme. Enhver kan hente, oprette og markere beskeder for en vilkårlig deltager. |
| G2 | **Ingen ejerskabsvalidering** | 🔴 Høj | `mark-read` og `by-participant` validerer ikke at `participantId` matcher JWT `sub`. |
| G3 | **Ingen real-time opdatering** | 🟡 Medium | Bruger skal manuelt genindlæse siden for at se nye beskeder. |
| G4 | **Bestillingslink i beskedtekst** | 🟢 Lav | `ParticipantToken` er indlejret i en fri tekststreng i `Message.Content` — ikke struktureret data. |

---

## Tekniske noter

- Beskeder er ikke sletbare — ingen `DELETE`-endpoint.
- `Message.Content` er fritekst og kan indeholde URLs (bestillingslinks, ordrelinks).
- `isRead` sættes ved `MarkAllReadAsync` — ingen per-besked markering.

---

## Relaterede use cases

- [UC-06 — Opret Ordre](UC-06-opret-ordre.md)
- [UC-08 — Bestil via Merchant-link](UC-08-bestil-via-merchant-link.md)
