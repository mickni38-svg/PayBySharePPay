# Step 02 – Domænemodel og DbContext

## Formål
At etablere datamodellen korrekt fra start.

## Krav der skal implementeres i dette step
- Opret `Participant` som fælles deltager-entitet
- Opret `ParticipantType` enum med mindst `Person` og `Merchant`
- Opret entiteter:
  - `FriendRelation`
  - `Order`
  - `OrderParticipant`
  - `Payment`
  - `Message`
- Tilføj merchant-relaterede felter på deltageren
- Opret EF Core `DbContext`
- Konfigurér relationer og constraints
- Gør modellen klar til SQL Server og migrationer

## Vigtige domæneregler
- Merchant er også en deltager
- Merchant har ekstra firma- og betalingsfelter
- `OrderParticipant` skal pege på `Participant`

## Må ikke implementeres endnu
- repository-klasser
- services
- API controllers
- Angular
