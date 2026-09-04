# BUG-04 — Forside har forkert skalering og unødvendig vertikal scrolling

**Status:** Implementeret  
**Prioritet:** 🟠 Medium  
**Opdaget:** 2026-09-04  
**Komponent:** Angular frontend / offentlig forside / mobilvisning

## Problem

Den offentlige PayNSync-forside på mobil har tre visuelle problemer:

1. PayNSync-logoet øverst fremstår for lille i forhold til resten af siden.
2. Forsiden kan scrolles vertikalt, selv om alt indhold bør kunne være synligt på én mobilskærm.
3. Det store forsidebillede/hero-billede er for smalt og udnytter ikke den tilgængelige bredde.

Det giver siden et mere kompakt og mindre færdigt udtryk end ønsket.

## Ønsket adfærd

Forsiden skal optimeres til mobil, så den fremstår som én samlet fullscreen-side uden unødvendig scrolling.

PayNSync-logoet skal være tydeligere og større.

Hero-/forsidebilledet skal være større og gå tættere ud mod venstre og højre kant.

Login- og "Opret bruger"-knapperne skal fortsat være synlige nederst uden, at brugeren behøver at scrolle.

## Visuelle ændringer

### PayNSync-logo

Logoet øverst skal øges i størrelse.

Det skal fortsat være centreret og bevare korrekt aspect ratio.

Logoet må ikke blive så stort, at hero-indholdet eller knapperne skubbes uden for viewporten.

### Hero-/forsidebillede

Det store forsidebillede skal fylde mere i bredden.

Det bør have væsentligt mindre side-margin end i dag og komme tættere ud mod mobilskærmens kanter.

Billedets aspect ratio skal bevares, så der ikke opstår stretching.

### Ingen vertikal scrolling

På normale mobilskærme skal hele forsiden kunne vises uden vertikal scrolling.

Det skal undersøges, hvad der skaber den ekstra højde nederst på siden, fx:

- `margin-bottom`
- `padding-bottom`
- `min-height`
- `100vh` kombineret med padding
- browser safe-area
- containerhøjde
- skjult element nederst
- knapcontainer med ekstra spacing

Problemet skal løses ved at rette layoutet og ikke ved blot at skjule relevant indhold.

## Acceptance Criteria

1. PayNSync-logoet er synligt større end i den nuværende version.
2. Logoet er centreret og ikke deformt.
3. Hero-billedet går tættere ud mod skærmens venstre og højre kant.
4. Hero-billedet bevarer korrekt aspect ratio.
5. På en normal iPhone-/Android-mobilvisning kan forsiden ses uden vertikal scrolling.
6. "Log ind" og "Opret bruger" er begge fuldt synlige.
7. Der er ikke et tomt eller usynligt område under knapperne, som gør siden scroll-bar.
8. Layoutet må ikke bruge `overflow: hidden` som workaround, hvis det skjuler reelt indhold.
9. Siden skal fortsat fungere responsivt på mindre og større mobilskærme.
10. Eksisterende login- og opret-bruger-funktionalitet må ikke ændres.

## Test

Test minimum på:

- iPhone Safari
- Android Chrome
- ca. 390 px bred viewport
- ca. 430 px bred viewport
- lav mobilhøjde, fx ca. 667–750 px

Der skal især kontrolleres, at der **ikke opstår scrolling på grund af få pixels ekstra højde nederst**.


## Implementering

Implementeret på `main` 2026-09-04.

- Offentlig forside bruger nu en særskilt `home--public` layouttilstand uden den globale `padding-bottom: 90px`, som skabte unødvendig scrolling.
- Hero-billedet går kant-til-kant i bredden på mobil og skaleres større, hvilket samtidig gør PayNSync-logoet i hero-grafikken større.
- Layoutet bruger `100dvh` og højdebegrænsning på hero-billedet, så login- og opret-bruger-knapperne forbliver synlige på lave mobilskærme.
- Der er tilføjet en ekstra tilpasning for skærmhøjder under 720 px.
