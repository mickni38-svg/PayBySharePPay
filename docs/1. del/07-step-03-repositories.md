# Step 03 – Repositories

## Formål
At implementere dataadgang gennem repositories.

## Krav der skal implementeres i dette step
- Opret repository-interfaces
- Opret konkrete repository-klasser
- Understøt dataadgang til:
  - deltagere
  - merchants
  - venrelationer
  - ordrer
  - betalinger
  - beskeder
- Understøt søgning på deltagere
- Understøt hentning af ordreoverblik-data
- Registrér repositories i dependency injection

## Vigtige regler
- repositories må kun håndtere dataadgang
- ingen forretningslogik i repositories
- brug async/await
