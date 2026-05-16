# Brugerflows

## Navigation – routes

| Route | Komponent | Beskrivelse |
|-------|-----------|-------------|
| `/` | → redirect til `/home` | |
| `/login` | `LoginComponent` | Email-login |
| `/register` | `RegisterComponent` | Opret person eller merchant |
| `/home` | `HomeComponent` | Forside med statuskort og genveje |
| `/orders` | `OrdersComponent` | Liste over ordrer |
| `/orders/create` | `CreateOrderComponent` | Opret ny ordre |
| `/orders/:id` | `OrderDetailComponent` | Detaljer for én ordre |
| `/messages` | `MessagesComponent` | Beskeder |
| `/activity` | `ActivityComponent` | Seneste aktivitet |
| `/pending-participants` | `PendingParticipantsComponent` | Afventende deltagere |
| `/find-participants` | `FindParticipantsComponent` | Søg brugere |
| `**` | → redirect til `/home` | Fallback |

Alle routes er lazy-loaded.

## Bundnavigation

`BottomNavComponent` er tilgængeligt på alle sider og giver genvej til: Hjem, Overblik, Opret, Brugere, Beskeder.

---

## Flow 1: Login

```
/login
  → Bruger indtaster email
  → POST /api/auth/login
  → JWT token gemmes i localStorage
  → Redirect til /home
```

Ingen password kræves. Login er email-opslag mod `Participants`-tabellen.

---

## Flow 2: Opret ordre

```
/home eller /orders
  → Klik "Opret"
  → /orders/create
	→ Udfyld titel, kategori, besked
	→ Vælg merchant (valgfrit)
	→ Inviter deltagere
  → POST /api/orders
  → Redirect til /home ved success
```

---

## Flow 3: Se og administrér ordre

```
/orders
  → Liste over egne ordrer (GET /api/orders?participantId=X)
  → Klik på ordre
  → /orders/:id
	→ Se ordrelinjer, deltagerstatus, betalinger
	→ Registrér betaling (POST /api/payments)
```

---

## Flow 4: Afventende deltagere

```
/home
  → Klik på "X deltagere afventer"-kortet
  → /pending-participants
	→ Oversigt over deltagere på tværs af ordrer der mangler handling
	→ Mulighed for at sende påmindelser (afhænger af implementering)
```

Beregnes client-side via `computePendingSummary()` i `order.model.ts`.

---

## Flow 5: Seneste aktivitet

```
/home
  → Klik på "Seneste aktivitet"-kortet
  → /activity
	→ Viser aktivitetsliste grupperet i "I dag", "I går", "Tidligere"
	→ Eller empty state: "Ingen nye aktiviteter siden sidst"
```

Aktiviteter bygges client-side i `ActivityService` fra ordredata. Ingen backend aktivitetslog.

---

## Flow 6: Beskeder

```
Bundnavigation → Beskeder
  → /messages
	→ Liste over beskeder (tilknyttet ordrer)
```

---

## Flow 7: Registrering

```
/register
  → Vælg: Person eller Merchant
  → Udfyld oplysninger
  → POST /api/auth/register-person eller /api/auth/register-merchant
  → Redirect til /login
```

---

## Uklare eller ufærdige flows

- **Invitations-flow via link/token:** `Order.JoinToken` feltet findes i databasen, men der er ingen UI eller API-endpoint til at håndtere join via token.
- **Påmindelser til afventende deltagere:** Knappen/logikken til at sende påmindelser fra `/pending-participants` er uklart implementeret – det er uklart ud fra den nuværende kodebase om der sendes emails eller blot vises en besked.
- **Merchant-flow i UI:** Det er uklart ud fra den nuværende kodebase hvordan en merchant-bruger opretter/administrerer ordredrafts via frontend. Merchant-draft API eksisterer, men der er ingen dedikeret merchant-UI-komponent.
