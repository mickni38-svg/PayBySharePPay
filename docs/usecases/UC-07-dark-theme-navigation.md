# UC-07 — Dark Theme Navigation

## Metadata
| Felt | Værdi |
|------|-------|
| UC-ID | UC-07 |
| Navn | Dark Theme Navigation |
| Aktør | Bruger |
| Prioritet | Middel |
| Status | Implementeret |

## Kort beskrivelse
Når brugeren vælger Dark Theme på profil-siden, ændres navigationslayoutet: knapperne Deltagere og Profil skjules fra forsiden, og Profil-adgang flyttes til bottom navigation-baren.

## Prækonditioner
- Brugeren er logget ind
- Brugeren er på profil-siden og har valgt Dark Theme

## Postkonditioner
- Dark theme er aktivt (data-theme="dark" på html-element)
- Deltagere-kortet og Profil-kortet på forsiden er usynlige
- Profil-linket vises i bottom nav i stedet for Brugere-linket
- Al navigation til /profile og /find-participants virker stadig via URL

## Normalforløb
1. Bruger åbner profil-siden
2. Bruger klikker på "Mørk" tema-knappen
3. ThemeService.setTheme('dark') kaldes – data-theme="dark" sættes på `<html>`
4. HomeComponent skjuler Deltagere- og Profil-action-kortene (visibility: hidden)
5. BottomNavComponent viser Profil-link og skjuler Brugere-link

## Alternativt forløb – Brugeren skifter tilbage til Standard/Pink
- Deltagere og Profil-kortene på forsiden bliver synlige igen
- Brugere-linket vises i bottom nav, Profil-linket skjules

## Tekniske noter
- ThemeService.current() er et Angular signal – reaktive bindings opdateres automatisk
- Kortene skjules med CSS-klassen `action-card--hidden` (visibility:hidden bevarer layoutet)
- Bottom nav bruger `@if`-direktiver til at vise/skjule links
