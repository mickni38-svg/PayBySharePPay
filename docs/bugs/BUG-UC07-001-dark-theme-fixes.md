# BUG-UC07-001 — Dark Theme Navigation: tre fejl fra UC-07 implementering

## Metadata
| Felt | Værdi |
|------|-------|
| Bug-ID | BUG-UC07-001 |
| Relateret UC | UC-07 Dark Theme Navigation |
| Opdaget | Visuel inspektion af implementering |
| Prioritet | Høj |
| Status | Rettet ✅ |

---

## Bug 1 — Duplikeret logo/hero-sektion

### Symptom
Forsiden viser to PayNSync-logoer: det eksisterende logo i den globale app-header (`app.component.ts`) og et nyt SVG-hero-kort tilføjet direkte i `HomeComponent`.

### Årsag
`app.component.ts` renderer allerede en `<header class="app-header">` med `images/logo.png` for **alle** sider i appen. UC-07-implementeringen tilføjede fejlagtigt en ekstra `<section class="home__hero">` med en SVG-kreditkortsillustration direkte i `home.component.html`.

### Påvirkning
Alle brugere — uanset tema — ser et dobbelt logo øverst på forsiden.

### Rettelse
Fjern `home__hero`-sektionen og tilhørende CSS fra `home.component.html` og `home.component.css`. Det eksisterende logo i `app-header` er det korrekte og skal bevares uændret.

---

## Bug 2 — Bottom navigation omstruktureret forkert

### Symptom
Bottom nav er ændret fra en simpel 5-element struktur (Forside/Hjem, Overblik, Opret, Brugere, Beskeder) til to helt separate grene med `@if`/`@else`. I dark mode vises kun 3 punkter, og i standard mode er strukturen restruktureret med alle elementer inde i et `@if`-blok.

### Årsag
UC-07-implementeringen omskrev hele nav-templaten i stedet for kun at tilføje de betingede ændringer for dark mode oven på den eksisterende struktur. Den originale struktur anvendte individuelle `@if`-betingelser per element; den nye struktur bruger `@if (themeService.current() !== 'dark') { ... alle elementer ... } @else { ... }`.

### Påvirkning
- Standard-tema brugere: navigationsstrukturen fungerer, men koden er unødigt omstruktureret og svær at vedligeholde
- Dark-tema brugere: nav ændres fra 5 til 3 elementer
- "Forside"-labelen er omdøbt til "Hjem" uden godkendelse

### Rettelse
Gendan bottom nav til den originale struktur med individuelle `@if`-betingelser. Bevar den eksisterende ændring: Brugere-link skjules og Profil-link vises i dark mode (som den var før UC-07-implementeringen).

---

## Bug 3 — Dark mode CSS-styles virker ikke (Angular ViewEncapsulation)

### Symptom
Dark mode er aktiv (`data-theme="dark"` er sat på `<html>`), men CTA-kortet, statuskortene og action-kortene viser ikke neon-glow borders, centreret layout eller hvid login-knap som i `newHome.png`.

### Årsag
Angular's `ViewEncapsulation.Emulated` (default) scoper komponent-CSS ved at tilføje et unikt attribut-suffix (f.eks. `[_ngcontent-xxx]`) til alle selektorer. En regel som:
```css
[data-theme="dark"] .cta-card { ... }
```
kompileres til:
```css
[data-theme="dark"] .cta-card[_ngcontent-xxx] { ... }
```
`data-theme="dark"` sidder på `<html>`-elementet, men `[_ngcontent-xxx]` attributten er kun på elementer **inde i** komponenten. Angular kan ikke matche begge dele i den samme sammensat selektor på tværs af DOM-niveauer.

Den korrekte Angular-måde at referere til ancestor-attributter er `:host-context()`:
```css
:host-context([data-theme="dark"]) .cta-card { ... }
```
Dette matcher korrekt når `data-theme="dark"` er til stede på et ancestor-element (f.eks. `<html>`).

### Påvirkning
Alle dark-mode visuelle effekter i `HomeComponent` er ikke synlige:
- Ingen neon-glow kanter på CTA-kortet, statuskort eller action-kort
- Action-kortene bevarer deres normale layout (ikon venstre + subtitle) i stedet for centreret ikon-layout
- "Log ind"-knappen forbliver farvet (primærfarve) i stedet for hvid

### Rettelse
Erstat alle `[data-theme="dark"] .selector`-regler i `home.component.css` med `:host-context([data-theme="dark"]) .selector`.
