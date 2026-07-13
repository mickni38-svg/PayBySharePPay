# UC-07 — Dark Theme Navigation

## Metadata
| Felt | Værdi |
|------|-------|
| UC-ID | UC-07 |
| Navn | Dark Theme Navigation |
| Aktør | Bruger |
| Prioritet | Middel |
| Status | Implementeret |
| Bug-rettelse | BUG-UC07-001 rettet: duplikeret logo fjernet, bottom nav genoprettet, dark-CSS fikset med `:host-context()` |

## Kort beskrivelse
Når brugeren vælger Dark Theme på profil-siden, ændres forsiden og navigationslayoutet: forsiden viser et PayNSync hero-logo og neon-glow-kanter på kortene, knapperne Deltagere og Profil skjules fra forsiden, og bottom nav forenkles til tre punkter: Hjem, Deltagere og Mere.

## Prækonditioner
- Brugeren er på profil-siden og har valgt Dark Theme (eller har dark theme gemt fra tidligere)

## Postkonditioner
- Dark theme er aktivt (data-theme="dark" på html-element)
- Forsiden viser PayNSync hero-logo øverst
- CTA-kortet og statuskortene har neon-glow kanter (pink/grøn)
- Action-kortene (Overblik og Beskeder) har neon-glow kanter og centreret ikon/label-layout
- Deltagere-kortet og Profil-kortet på forsiden er usynlige (visibility: hidden)
- "Log ind"-knappen er hvid med sort tekst i dark mode
- Bottom nav viser tre punkter: Hjem, Deltagere (/find-participants), Mere (/profile)
- Al navigation til /profile og /find-participants virker stadig via URL og via Mere-linket

## Normalforløb
1. Bruger åbner profil-siden
2. Bruger klikker på "Mørk" tema-knappen
3. ThemeService.setTheme('dark') kaldes – data-theme="dark" sættes på `<html>`
4. HomeComponent viser PayNSync hero-sektion med SVG-kreditkortsillustration og branding
5. HomeComponent skjuler Deltagere- og Profil-action-kortene (visibility: hidden via `action-card--hidden`)
6. Action-kortene (Overblik, Beskeder) skifter til centreret ikon-layout med neon-glow border og box-shadow
7. CTA-kortet og statuskortene viser neon-glow border (pink hhv. grøn)
8. "Log ind"-knappen skifter til hvid baggrund med sort tekst
9. BottomNavComponent forenkles: viser kun Hjem, Deltagere og Mere (tre links)

## Alternativt forløb – Brugeren skifter tilbage til Standard
- Deltagere og Profil-kortene på forsiden bliver synlige igen
- Action-kortene skifter tilbage til normal layout (ikon venstre, subtitle, border-bottom accent)
- CTA og statuskort viser normal border (ingen glow)
- "Log ind"-knappen vender tilbage til primærfarve
- Bottom nav viser alle fem punkter: Hjem, Overblik, Opret, Brugere, Beskeder

## Tekniske noter
- ThemeService.current() er et Angular signal – reaktive bindings opdateres automatisk
- Kortene skjules med CSS-klassen `action-card--hidden` (visibility:hidden bevarer layoutet)
- Bottom nav bruger `@if`/`@else`-direktiv: `themeService.current() !== 'dark'` vs. `=== 'dark'`
- Neon-glow implementeres som `box-shadow` og `border` via `[data-theme="dark"]`-selektorer i CSS
- Glow-farven på action-kortene sættes dynamisk via inline `[style.box-shadow]`-binding med kortets `accent`-hex-farve (+ `40` alpha suffix)
- Hero-sektionen er et `<svg>`-baseret kreditkortsdesign med CSS-tekst (Pay hvid, NSync orange #F59E0B)
- Overblik-accent: `#38BDF8` (sky blue), Beskeder-accent: `#EC4899` (hot pink)
