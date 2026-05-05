# Step 04 – Services

## Formål
At implementere use cases og forretningslogik.

## Krav der skal implementeres i dette step
- Opret service-interfaces og services til:
  - søg deltagere
  - opret person
  - opret merchant
  - tilføj ven
  - opret ordre
  - hent ordreoverblik
  - registrer betaling
  - hent/opret beskeder
- Tilføj validering og forretningsregler
- Brug repositories fra data-laget
- Returnér DTO'er hvor det giver mening

## Vigtige regler
- service-laget ejer forretningslogikken
- services må ikke kende til UI
