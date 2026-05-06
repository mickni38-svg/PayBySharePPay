# SBYS + Copilot Claude Sonnet 4.6 — Step-by-step guide

## Formål
Denne guide fortæller præcis, hvordan du bør arbejde med Copilot Claude Sonnet 4.6 i Visual Studio uden at ramme token-limit.

Arbejd efter princippet:

> Én chat = én opgave = få relevante filer

Undgå at uploade hele projektet og alle dokumenter på én gang.

---

# Overordnet workflow

## Trin 1 — Start med arkitektur/gap-analyse
Upload kun:

- `01_AI_CONTEXT_OVERVIEW.md`
- `02_AI_CONTEXT_ARCHITECTURE.md`
- eventuelt solution/project tree som tekst
- relevante `.csproj` filer hvis Copilot ikke selv kan se projektet

Brug prompten i afsnit **Prompt 1**.

---

## Trin 2 — Directory + merchants
Upload kun:

- `01_AI_CONTEXT_OVERVIEW.md`
- `03_AI_CONTEXT_DIRECTORY_AND_MERCHANTS.md`
- relevante filer:
  - `BatchPayContext.cs`
  - `UserEntity.cs`
  - `MerchantEntity.cs`
  - `MerchantIntegrationEntity.cs`
  - `DirectoryEntryDto.cs`
  - `UsersController.cs`
  - `FriendsController.cs`
  - `DirectoryController.cs`
  - frontend directory/find page filer

Brug prompten i afsnit **Prompt 2**.

---

## Trin 3 — Group payment core flow
Upload kun:

- `01_AI_CONTEXT_OVERVIEW.md`
- `04_AI_CONTEXT_GROUP_PAYMENT_FLOW.md`
- relevante filer:
  - `GroupPaymentEntity.cs`
  - `GroupPaymentMemberEntity.cs`
  - `GroupPaymentsController.cs`
  - `CreateGroupPaymentRequestDto.cs`
  - `GroupPaymentDto.cs`
  - group payment service/interface filer
  - frontend create/overview page filer

Brug prompten i afsnit **Prompt 3**.

---

## Trin 4 — Merchant order / fake pizza demo
Upload kun:

- `01_AI_CONTEXT_OVERVIEW.md`
- `05_AI_CONTEXT_MERCHANT_ORDER_AND_FAKE_PIZZA.md`
- relevante backend filer:
  - `MerchantEntity.cs`
  - `MerchantIntegrationEntity.cs`
  - `GroupPaymentEntity.cs`
  - `GroupPaymentMemberEntity.cs`
  - `BatchPayContext.cs`
  - eksisterende controllers/services
- relevante frontend overview/status filer

Brug prompten i afsnit **Prompt 4**.

---

## Trin 5 — Security
Upload kun:

- `01_AI_CONTEXT_OVERVIEW.md`
- `06_AI_CONTEXT_SECURITY.md`
- auth/security relevante filer
- merchant integration relevante filer

Brug prompten i afsnit **Prompt 5**.

---

# Prompt 1 — Arkitektur og gap-analyse

```text
Læs de uploadede SBYS AI-context dokumenter først.

Opgave:
Lav en gap-analyse af projektet i forhold til SBYS MVP.

Vigtige regler:
- Implementér ikke kode endnu.
- Scan projektstrukturen og eksisterende filer.
- Sammenlign med dokumentationen.
- Hold analysen kort, men konkret.
- Prioritér alt som MVP Critical, MVP Optional eller Later Phase.

Analysen skal opdeles i:
- Backend/API
- Domain entities
- Database/migrations
- DTOs/contracts
- Services/business logic
- Web frontend/mobile view
- Merchant integration
- Group payment flow
- Security/auth
- Notifications/status
- Technical debt/risks

Afslut med:
1. Anbefalet implementeringsrækkefølge
2. Første lille opgave
3. Præcis hvilke filer der skal ændres/oprettes først
4. Vent på min godkendelse før du implementerer
```

---

# Prompt 2 — Directory + merchants

```text
Læs de uploadede SBYS AI-context dokumenter og relevante kodefiler.

Opgave:
Implementér eller ret directory/merchant feature, så web frontend i mobilvisning kan vise både almindelige brugere og spisesteder i samme liste.

Regler:
- Merchant er ikke en UserEntity.
- Merchant skal vises i samme directory-liste.
- Brug eksisterende patterns og namespaces.
- Skriv hele filer.
- Lav ikke breaking changes uden forklaring.
- Implementér kun directory/merchant delen.

Forventet resultat:
- API endpoint der returnerer users + merchants.
- DTO/model til fælles directory entry.
- Frontend liste i mobilvisning.
- Merchants skal visuelt skelnes fra users.
- Users kan vælges/tilføjes som venner.
- Merchants kan vises som spisesteder/favoritter senere, men ikke blandes som UserEntity.

Start med at beskrive:
1. Hvilke filer du ændrer
2. Hvorfor
3. Derefter implementér hele filer
```

---

# Prompt 3 — Group payment core flow

```text
Læs de uploadede SBYS AI-context dokumenter og relevante kodefiler.

Opgave:
Implementér eller ret det centrale group payment flow.

Regler:
- Opretter vælger venner/deltagere.
- GroupPayment skal kunne vises i overview/status.
- Fokus er MVP.
- Ingen betalingsgateway endnu.
- Ingen merchant order integration i denne opgave, medmindre der allerede findes fundament.
- Skriv hele filer.

Forventet resultat:
- En bruger kan oprette gruppebetaling.
- Deltagere gemmes korrekt.
- Overview kan vise group payments for en bruger.
- Statusfelter skal være klar til senere merchant/payment flow.

Start med:
1. Kort analyse af eksisterende group payment kode
2. Hvilke mangler der findes
3. Implementér første nødvendige rettelse
```

---

# Prompt 4 — Merchant order / fake pizza demo

```text
Læs de uploadede SBYS AI-context dokumenter og relevante kodefiler.

Opgave:
Design og implementér første MVP fundament til at simulere en pizza-webshop, der sender en ordre til SBYS.

Vigtige regler:
- Model A: Merchant håndterer betaling. SBYS håndterer orchestration/status.
- Når bruger klikker “Betal som gruppe” på fake pizza-web, skal merchant sende ordredata til SBYS.
- SBYS skal gemme ordrelinjer, totalbeløb, merchant reference og status.
- Ordren må ikke frigives til produktion før alle deltagere er reserveret/betalt.
- Implementér kun første lille trin, ikke hele betalingsflowet.
- Skriv hele filer.

Forventet første MVP:
- Entity/entities til merchant group order draft og participant order, hvis de mangler.
- DTOs til init order payload.
- API endpoint til at modtage ordre fra merchant.
- Status = Draft/Collecting.
- Overview kan senere vise ordredata.

Start med:
1. Foreslå minimal datamodel
2. Foreslå endpoints
3. Implementér kun datamodel + migration-klare entities/context ændringer
```

---

# Prompt 5 — Security/auth

```text
Læs de uploadede SBYS AI-context dokumenter og relevante sikkerheds-/API-filer.

Opgave:
Planlæg og implementér sikkerhed trinvist.

Regler:
- JWT bruges til SBYS user login/API.
- groupPaymentId må aldrig alene give adgang.
- Invitation links skal bruge joinToken eller signed token.
- Merchant server-to-server calls skal HMAC-signeres.
- SBYS webhooks til merchant skal også signeres.
- Implementér ikke alt på én gang.

Start med:
1. Kort security gap-analyse
2. Foreslå token strategy
3. Foreslå hvilke endpoints der skal beskyttes først
4. Implementér kun første lille sikkerhedstrin
```

---

# Generelle regler til alle Copilot chats

Brug altid disse regler nederst i prompten:

```text
Generelle regler:
- Skriv hele filer, ikke uddrag.
- Brug eksisterende namespaces.
- Brug eksisterende arkitektur.
- Undgå over-engineering.
- Implementér kun den aftalte opgave.
- Hvis du er usikker, så forklar antagelsen før du ændrer kode.
- Efter implementering: giv test-trin og migrations-kommandoer hvis relevant.
```
