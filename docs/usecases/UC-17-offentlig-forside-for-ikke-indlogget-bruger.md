# UC-17 – Vis offentlig forside for ikke-indlogget bruger

## Formål

PayNSync skal have en tydelig forskel mellem forsiden for en bruger, der er logget ind, og en bruger, der ikke er logget ind.

I dag vises den normale PayNSync-forside også for en ikke-indlogget bruger. Det betyder, at brugeren kan se funktioner og tekster som:

- "Start en gruppebetaling"
- "Du har endnu ingen spisesteder på din venneliste"
- "Find spisested"
- "Overblik"
- "Beskeder"

Disse funktioner giver først rigtig mening, når brugeren er logget ind.

En ikke-indlogget bruger skal derfor i stedet se en enkel offentlig velkomstforside, som forklarer PayNSync og tydeligt guider brugeren til login eller oprettelse af bruger.

Den eksisterende personlige forside skal fortsat vises efter login.

---

## Aktører

### Primær aktør

En besøgende, som åbner PayNSync uden at være logget ind.

### Sekundær aktør

En eksisterende PayNSync-bruger, som logger ind og derefter får adgang til den almindelige personlige forside.

---

# Overordnet brugerflow

## Ikke logget ind

Når brugeren åbner PayNSync uden en aktiv login-session:

1. PayNSync viser den offentlige forside.
2. PayNSync-logoet vises øverst som i den eksisterende løsning.
3. Den personlige gruppebetalingsfunktionalitet vises ikke.
4. Brugeren får en kort forklaring på, hvad PayNSync bruges til.
5. Brugeren kan vælge:
   - **Log ind**
   - **Opret bruger**
6. Efter succesfuldt login vises den eksisterende personlige PayNSync-forside.

Flow:

```text
Offentlig forside
      |
      +--> Log ind
      |       |
      |       +--> Succes
      |               |
      |               v
      |        Personlig forside
      |
      +--> Opret bruger
              |
              +--> Eksisterende registreringsflow
```

---

# UI – ikke logget ind

Den offentlige forside skal visuelt passe til PayNSync's eksisterende design.

Det eksisterende dark theme, spacing, typografi, navigation og PayNSync-branding skal genbruges.

Der skal ikke laves et nyt eller separat designsystem.

## Logo

Det eksisterende PayNSync-logo beholdes øverst på siden.

Eksisterende logo-komponent/assets skal genbruges.

Der skal ikke oprettes et nyt logo.

---

## Velkomstområde

Under logoet vises eksempelvis:

### Overskrift

**Betal sammen – uden besværet**

### Beskrivelse

**Opret en gruppebetaling, inviter venner og betal hver jeres del.**

Teksten må gerne justeres en smule, hvis det er nødvendigt for eksisterende layout/responsivitet, men betydningen skal bevares.

Formålet er, at en ny bruger hurtigt kan forstå, hvad PayNSync er.

---

# Primær handling – Log ind

Der skal være en tydelig primær CTA:

**Log ind**

Denne skal visuelt være den vigtigste handling på siden.

Ved klik åbnes det eksisterende login-flow.

Eksisterende authentication/login-komponenter og services skal genbruges.

Der må ikke implementeres et nyt parallelt login-system.

---

# Sekundær handling – Opret bruger

Der vises desuden en sekundær handling:

**Opret bruger**

Ved klik navigeres brugeren til eksisterende registreringsflow, hvis dette allerede findes.

Hvis registrering allerede håndteres som del af eksisterende authentication-flow, skal den eksisterende løsning genbruges.

Der må ikke implementeres et nyt registreringssystem alene til denne use case.

---

# Elementer der IKKE skal vises når brugeren er logget ud

Følgende eksisterende elementer fra den personlige forside skal skjules for en ikke-indlogget bruger:

- Start en gruppebetaling
- information om vennelistens spisesteder
- Find spisested
- Overblik
- igangværende gruppebetalinger
- Beskeder
- betalingsanmodninger
- andre bruger-/kontoafhængige dashboarddata

Dette gælder kun selve forsiden.

Funktionaliteten må ikke fjernes fra applikationen.

Den skal fortsat være tilgængelig efter login.

---

# UI – logget ind

Når brugeren er logget ind, skal den eksisterende personlige forside fortsat vises.

Denne use case må derfor ikke redesigne eller fjerne den eksisterende authenticated home page.

Den skal fortsat kunne indeholde blandt andet:

```text
Start en gruppebetaling

Du har endnu ingen spisesteder på din venneliste

Find spisested

Overblik
Se igangværende gruppebetalinger

Beskeder
Se dine anmodninger
```

Hvis brugeren allerede har venner, spisesteder, gruppebetalinger eller beskeder, skal eksisterende data og eksisterende visning fortsat fungere uændret.

---

# Authentication state

Forsiden skal afgøre hvilken variant, der skal vises ud fra applikationens eksisterende authentication-state.

Eksempel:

```text
authenticated == false
    -> offentlig forside

authenticated == true
    -> eksisterende personlig forside
```

Der skal bruges den authentication/session-mekanisme, som PayNSync allerede anvender.

Der må ikke introduceres en ekstra lokal boolean eller separat login-state alene til denne side, hvis applikationen allerede har en central authentication service/state.

Ved refresh skal korrekt forside stadig blive vist ud fra eksisterende session/token.

---

# Profil og login

PayNSync har allerede flyttet **Log ud** til Profil-siden.

Dette skal bevares.

Der skal skelnes mellem:

### Ikke logget ind

Brugeren har ikke en egentlig profil endnu.

Profil-navigation kan bruges som adgang til login/registrering.

### Logget ind

Profil-siden viser brugerens eksisterende profilfunktionalitet og mulighed for:

**Log ud**

Login skal ikke flyttes ind som eneste adgang via Profil-siden.

Den offentlige forside skal have en tydelig **Log ind**-knap.

---

# Bottom navigation

Den eksisterende bottom navigation består af:

- Forside
- Venner
- Beskeder
- Profil

Navigationen skal som udgangspunkt fortsat være synlig, så PayNSync bevarer samme app-struktur.

## Ikke logget ind

### Forside

Forside er aktiv og viser den offentlige forside.

### Profil

Hvis brugeren vælger Profil uden at være logget ind, skal brugeren kunne komme til eksisterende login/registreringsflow.

### Venner og Beskeder

En ikke-indlogget bruger må ikke få adgang til private brugerdata.

Implementeringen skal følge eksisterende routing/auth-guard-mønster.

Hvis der allerede findes auth guards, skal disse genbruges.

Ved forsøg på at åbne en beskyttet side skal brugeren sendes til login eller den relevante eksisterende authentication-side.

Der skal ikke implementeres en ny parallel mekanisme til dette.

---

# Login success

Når brugeren logger korrekt ind:

1. authentication-state opdateres via eksisterende auth-løsning.
2. brugeren navigeres til forsiden.
3. den offentlige forside forsvinder.
4. den eksisterende personlige forside vises.

Dette må ikke kræve manuel refresh.

Eksempel:

```text
Offentlig forside
       |
       v
     Login
       |
       v
Authentication success
       |
       v
      /
       |
       v
Personlig PayNSync-forside
```

---

# Logout

Eksisterende logout på Profil-siden skal fortsat anvendes.

Når brugeren logger ud:

1. eksisterende authentication/session fjernes.
2. private brugerdata må ikke længere vises.
3. brugeren navigeres til forsiden.
4. den offentlige forside vises.

Det må ikke være nødvendigt manuelt at reloade browseren.

---

# Routing

Eksisterende routes skal genbruges så langt som muligt.

Forsiden bør fortsat være den eksisterende home/root route.

Eksempel:

```text
/
```

Det er authentication-state, som bestemmer indholdet.

Der bør derfor som udgangspunkt ikke oprettes to forskellige offentlige URL'er som:

```text
/public-home
/private-home
```

medmindre den eksisterende arkitektur allerede er bygget sådan.

Foretrukken løsning:

```text
/
 |
 +-- unauthenticated -> Public Home
 |
 +-- authenticated   -> Existing Home
```

---

# Genbrug af eksisterende kode

Codex skal først undersøge eksisterende implementation af:

- Home/forside
- authentication service
- login
- registrering
- profil
- logout
- route guards
- bottom navigation
- global application state

Eksisterende komponenter og patterns skal genbruges.

Undgå duplicate authentication logic.

Undgå duplicate navigation logic.

Undgå duplicate login components.

---

# Responsivt design

Siden skal fortsat fungere som mobil-first PayNSync UI.

Den skal minimum fungere korrekt på:

- mobiltelefon
- PWA
- tablet
- desktop browser

På mobil skal CTA-knapper være nemme at trykke på.

Der må ikke introduceres horisontal scrolling.

Eksisterende spacing og max-width-regler skal genbruges.

---

# Loading state

Hvis authentication-state endnu ikke er kendt under application startup, må PayNSync ikke kortvarigt vise den forkerte forside.

Eksempel på problem der skal undgås:

```text
App åbner
↓
Offentlig forside vises 200 ms
↓
Session findes
↓
Personlig forside vises
```

Dette giver et visuelt "flash".

Hvis eksisterende auth-løsning har en loading/initializing state, skal denne anvendes.

Eksempel:

```text
Auth initializing
      |
      v
Vis eksisterende loading state
      |
      +--> Authenticated
      |       -> personlig forside
      |
      +--> Not authenticated
              -> offentlig forside
```

Der skal ikke nødvendigvis introduceres en ny loading-komponent, hvis en allerede findes.

---

# Sikkerhed

Skjulning af UI må ikke bruges som eneste adgangskontrol.

Private routes/API-kald skal fortsat være beskyttet gennem den eksisterende authentication/authorization-løsning.

En ikke-indlogget bruger må ikke kunne hente:

- vennedata
- beskeder
- gruppebetalinger
- betalingsanmodninger
- profilinformation

alene ved manuelt at navigere til en route.

Denne use case ændrer ikke backend authorization, medmindre eksisterende implementation viser, at det er nødvendigt for at opfylde use casen.

---

# Backend

Use casen forventes primært at være en frontendændring.

Der skal ikke tilføjes backend endpoints, hvis den nødvendige authentication-state allerede kan bestemmes gennem eksisterende løsning.

Hvis login, logout og current-user/session allerede fungerer, skal disse genbruges.

---

# Tests

Automatiske tests skal dække forskellen mellem authenticated og unauthenticated state.

Eksterne/live services må ikke kaldes fra tests.

Authentication skal mockes/stubbes efter eksisterende test-pattern.

## Test 1 – Ikke logget ind

Given:

```text
authentication state = unauthenticated
```

When:

```text
home page åbnes
```

Then:

- PayNSync-logo vises
- velkomstoverskrift vises
- beskrivende PayNSync-tekst vises
- Log ind vises
- Opret bruger vises
- Start en gruppebetaling vises ikke
- Overblik vises ikke
- Beskeder-dashboard vises ikke

---

## Test 2 – Logget ind

Given:

```text
authentication state = authenticated
```

When:

```text
home page åbnes
```

Then:

den eksisterende personlige PayNSync-forside vises.

---

## Test 3 – Login

Given:

brugeren befinder sig på offentlig forside.

When:

brugeren gennemfører eksisterende login-flow.

Then:

brugeren ender på den personlige forside uden manuel browser refresh.

---

## Test 4 – Logout

Given:

brugeren er logget ind.

When:

brugeren logger ud fra Profil.

Then:

- session fjernes
- brugeren sendes til forsiden
- offentlig forside vises
- private home-data vises ikke længere

---

## Test 5 – Beskyttet navigation

Given:

brugeren ikke er logget ind.

When:

brugeren forsøger at åbne en beskyttet route såsom Venner eller Beskeder.

Then:

brugeren får ikke adgang til private data og håndteres af eksisterende auth/routing-flow.

---

## Test 6 – Auth initialization

Given:

authentication-status stadig initialiseres.

When:

applikationen starter.

Then:

den forkerte home-variant må ikke vises kortvarigt før authentication-state er kendt.

---

# Acceptance Criteria

## AC1 – Offentlig forside

**Given** brugeren ikke er logget ind  
**When** brugeren åbner PayNSync  
**Then** vises en offentlig PayNSync-forside.

---

## AC2 – Forklaring af PayNSync

**Given** brugeren ikke er logget ind  
**When** forsiden vises  
**Then** kan brugeren tydeligt se, at PayNSync bruges til gruppebetaling, hvor deltagere betaler hver deres del.

---

## AC3 – Login CTA

**Given** brugeren ikke er logget ind  
**When** forsiden vises  
**Then** findes en tydelig primær handling "Log ind".

---

## AC4 – Opret bruger

**Given** brugeren ikke er logget ind  
**When** forsiden vises  
**Then** findes en sekundær handling "Opret bruger".

---

## AC5 – Private home-elementer skjules

**Given** brugeren ikke er logget ind  
**When** forsiden vises  
**Then** vises brugerafhængige funktioner såsom Overblik, Beskeder og Start gruppebetaling ikke.

---

## AC6 – Eksisterende home bevares

**Given** brugeren er logget ind  
**When** forsiden åbnes  
**Then** vises den eksisterende personlige PayNSync-forside.

---

## AC7 – Login skifter forside

**Given** brugeren befinder sig på offentlig forside  
**When** login gennemføres  
**Then** vises den personlige forside uden manuel refresh.

---

## AC8 – Logout skifter forside

**Given** brugeren er logget ind  
**When** brugeren logger ud via Profil  
**Then** navigeres brugeren til den offentlige forside.

---

## AC9 – Profil

**Given** brugeren er logget ind  
**When** Profil åbnes  
**Then** eksisterende mulighed for Log ud bevares.

---

## AC10 – Beskyttede funktioner

**Given** brugeren ikke er logget ind  
**When** brugeren forsøger at tilgå Venner, Beskeder eller andre private funktioner  
**Then** må private data ikke vises.

---

## AC11 – Genbrug eksisterende authentication

Implementeringen skal anvende eksisterende authentication-, routing- og session-patterns og må ikke skabe et parallelt login-system.

---

## AC12 – Ingen auth-flicker

Ved application startup må den offentlige forside ikke kortvarigt vises for en bruger med en gyldig eksisterende session, mens authentication-state initialiseres.

---

# Ikke en del af denne use case

Denne use case omfatter ikke:

- redesign af den personlige forside
- ændring af gruppebetalingsflowet
- ændring af betalingslogik
- ændring af MobilePay/Vipps integration
- redesign af Profil
- ny authentication-provider
- nyt login-system
- ændring af eksisterende bruger-/vennemodel
- backendændringer uden konkret behov
- ny landing-page marketingløsning
- SEO-optimering

---

# Implementeringsinstruktion til Codex

Før implementering:

1. Læs projektets `.ai/00-ROUTER.md` og `.ai/01-CONTRACT.md`.
2. Følg projektets eksisterende instructions og arkitektur.
3. Find den nuværende Home/Forside-komponent.
4. Find eksisterende authentication-state/service.
5. Find eksisterende login- og registreringsflow.
6. Find Profil og eksisterende logout.
7. Find eksisterende route guards.
8. Find bottom navigation.
9. Genbrug eksisterende komponenter og styling.
10. Undersøg eksisterende tests og følg samme test-pattern.

Implementeringen skal være den mindst invasive ændring, der opfylder use casen.

Eksisterende funktionalitet for en logget-ind bruger må ikke regressere.
