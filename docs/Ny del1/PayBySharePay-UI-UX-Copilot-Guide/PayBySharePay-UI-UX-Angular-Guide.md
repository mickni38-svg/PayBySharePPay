# PayBySharePay UI/UX Refactor Guide – Angular / HTML / TypeScript / SCSS

Denne guide bruges som input til GitHub Copilot Agent i Visual Studio.
Den er skrevet specifikt til **Angular 19 standalone components** med `.html`, `.scss` og `.ts` filer.
Formålet er at løfte PayBySharePay mobilappens UI fra en funktionel prototype til et professionelt, moderne fintech-design.

Appen skal fortsat være enkel, mørk, mobilvenlig og konsekvent på tværs af siderne.

---

## Referencebilleder

### Reference 1: samlet skærmretning
![PayBySharePay UI reference screens](./01-ui-reference-screens.png)

### Reference 2: flow, kort, detaljer og udvikler-login
![PayBySharePay UI reference flow](./02-ui-reference-flow.png)

---

## Projektstruktur (eksisterende filer)

```
src/Frontend.PayBySharePay/src/
├── styles.css                          ← Global styles (lys tema, skal opdateres)
├── app/
│   ├── app.component.html/.scss/.ts    ← App shell / router-outlet
│   ├── layout/
│   │   └── bottom-nav/                 ← Bottom navigation (html/scss/ts)
│   ├── features/
│   │   ├── home/                       ← Forside (.html/.css/.ts)
│   │   ├── login/                      ← Email login (.html/.scss/.ts)
│   │   ├── orders/                     ← Overblik (.html/.scss/.ts)
│   │   ├── order-detail/               ← Ordredetalje (.html/.scss/.ts)
│   │   ├── create-order/               ← Opret gruppebetaling (.html/.scss/.ts)
│   │   ├── find-participants/          ← Find personer & steder (.html/.css/.ts)
│   │   └── messages/                   ← Beskeder (.html/.ts, ingen scss)
│   └── core/
│       ├── models/
│       ├── services/
│       └── interceptors/
```

**Vigtig observation:** `home` og `find-participants` bruger `.css` (ikke `.scss`). Overvej at konvertere til `.scss` for konsistens, men det er valgfrit.

---

## Designbeskrivelse

PayBySharePay skal have et moderne **dark-mode** mobilinterface inspireret af Splitwise, Revolut, Lunar og MobilePay. UI'et skal være mørkt, roligt og professionelt med:

- Bløde kort med subtil border
- Diskrete gradienter
- Tydelig typografi med klart hierarki
- Små status-badges
- God luft mellem elementer
- Fast bundnavigation

---

## Vigtig regel

Lav UI-refactoren i **små, sikre trin**. Efter hvert trin skal `ng build` fortsat lykkes.
Undgå ændringer i business logic, services, API-kald eller routing medmindre det er nødvendigt for UI.

---

# Step 1 – Gennemgå eksisterende projekt

## Opgave

Læs eksisterende `.html`, `.ts`, `.scss`/`.css` filer og find:

- Appens eksisterende sider og komponenter
- Eksisterende global styles i `styles.css`
- Bundnavigationen i `bottom-nav`
- Forside/dashboard: `home.component`
- Overbliksside: `orders.component`
- Opret-side: `create-order.component`
- Find-side: `find-participants.component`
- Beskeder-side: `messages.component`
- Ordredetalje: `order-detail.component`

## Output

Lav først en kort analyse i chatten:

- Hvilke filer skal ændres
- Hvilke styles findes allerede (global `styles.css`, komponent-specifikke `.scss`/`.css`)
- Hvilke CSS-klasser/design-tokens bruges på tværs
- Hvilke komponenter bør genbruges
- Hvilke Angular standalone komponenter bør oprettes som delte UI-komponenter

Foretag **ikke** kodeændringer før analysen er lavet.

---

# Step 2 – Opdater global styles og design tokens

## Mål

Opdater `src/styles.css` (eller konvertér til `styles.scss`) så hele appen får samme visuelle sprog via CSS custom properties.

## Design tokens (CSS custom properties)

Erstat det eksisterende lyse tema med disse dark-mode værdier:

```css
:root {
  /* Baggrunde */
  --color-bg:           #070B14;
  --color-surface:      #101827;
  --color-surface-elevated: #151E2E;
  --color-input-bg:     #121B2A;
  --color-border:       #263247;

  /* Primær accent */
  --color-primary:      #22C55E;
  --color-primary-dark: #15803D;

  /* Sekundære accenter */
  --color-cyan:         #06B6D4;
  --color-purple:       #7C3AED;
  --color-orange:       #F59E0B;
  --color-danger:       #EF4444;

  /* Tekst */
  --color-text-primary:   #FFFFFF;
  --color-text-secondary: #94A3B8;
  --color-text-muted:     #64748B;

  /* Border radius */
  --radius-sm:   8px;
  --radius-md:   14px;
  --radius-lg:   20px;
  --radius-xl:   24px;

  /* Spacing */
  --space-page:    20px;
  --space-card:    16px;
  --space-section: 20px;
  --space-item:    12px;

  /* Typografi */
  --font-page-title:   700 24px/1.2 inherit;
  --font-section-label: 600 12px/1 inherit;
  --font-body:          400 15px/1.5 inherit;
  --font-caption:       400 12px/1.4 inherit;
  --font-amount:        700 18px/1 inherit;
}
```

## Globale utility-klasser

Tilføj disse genbrugelige klasser i `styles.css`/`styles.scss`:

```css
/* Layout */
.page-shell { background: var(--color-bg); min-height: 100dvh; color: var(--color-text-primary); }
.page-content { padding: 0 var(--space-page) var(--space-page); }

/* Cards */
.app-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-card);
}

/* Knapper */
.btn-primary { ... }   /* Grøn fyldt */
.btn-ghost   { ... }   /* Transparent med border */

/* Status badges */
.badge { ... }
.badge--pending  { color: #F59E0B; background: rgba(245,158,11,0.12); }
.badge--accepted { color: #22C55E; background: rgba(34,197,94,0.12); }
.badge--paid     { color: #06B6D4; background: rgba(6,182,212,0.12); }
.badge--declined { color: #EF4444; background: rgba(239,68,68,0.12); }

/* Avatar */
.avatar { border-radius: 50%; display:flex; align-items:center; justify-content:center; font-weight:700; color:#fff; }
.avatar--sm { width:32px; height:32px; font-size:11px; }
.avatar--md { width:44px; height:44px; font-size:15px; }
.avatar--lg { width:56px; height:56px; font-size:18px; }
```

## Opgave

1. Erstat indholdet af `src/styles.css` med ovenstående dark-mode tokens og utility-klasser.
2. Sørg for at `body` og `html` bruger `--color-bg` som baggrund.
3. Kør `ng build` og verificér ingen fejl.

---

# Step 3 – Opret delte UI-komponenter (shared)

## Mål

Undgå at hver side genopfinder sine egne kort, badges og knapper.

## Mappe til delte komponenter

```
src/app/shared/
├── components/
│   ├── app-card/
│   ├── status-badge/
│   ├── avatar/
│   ├── empty-state/
│   └── primary-button/
```

Generér med Angular CLI:
```bash
ng generate component shared/components/app-card --standalone
ng generate component shared/components/status-badge --standalone
ng generate component shared/components/avatar --standalone
ng generate component shared/components/empty-state --standalone
ng generate component shared/components/primary-button --standalone
```

## Komponent-specifikationer

### AppCardComponent
```typescript
// Input: ingen (wrapper via ng-content)
// Brug: <app-card>indhold</app-card>
// Styling: --color-surface, --color-border, --radius-lg, padding
```

### StatusBadgeComponent
```typescript
// Input: status: 'pending' | 'accepted' | 'paid' | 'declined' | string
// Input: label?: string  (override tekst)
// Brug: <app-status-badge status="accepted" />
// Viser: grøn "accepteret", orange "afventer", cyan "betalt", rød "afvist"
```

### AvatarComponent
```typescript
// Input: name: string       (til initialer)
// Input: color?: string     (baggrundsfarve – auto-genereret hvis ikke angivet)
// Input: size?: 'sm'|'md'|'lg'
// Input: icon?: string      (emoji, fx 🍽️ til merchants)
// Brug: <app-avatar name="Person 1" size="md" />
```

### EmptyStateComponent
```typescript
// Input: icon: string      (emoji eller SVG)
// Input: title: string
// Input: subtitle?: string
// Input: actionLabel?: string
// Output: actionClick: EventEmitter<void>
// Brug: <app-empty-state icon="💬" title="Ingen beskeder" subtitle="..." />
```

### PrimaryButtonComponent
```typescript
// Input: label: string
// Input: disabled?: boolean
// Input: loading?: boolean
// Input: variant?: 'primary'|'ghost'|'danger'
// Output: clicked: EventEmitter<void>
// Brug: <app-primary-button label="Opret" [loading]="isSubmitting()" />
```

## Opgave

1. Opret mappen `src/app/shared/components/`.
2. Implementér alle fem komponenter som Angular standalone components.
3. Opret `src/app/shared/index.ts` der eksporterer alle.
4. Kør `ng build` og verificér ingen fejl.

---

# Step 4 – Refaktor bundnavigation

## Fil: `src/app/layout/bottom-nav/bottom-nav.component.html` og `.scss`

## Mål

Bundnavigationen skal se professionel ud med aktiv grøn accent og tydelig ikonplacering.

## Krav

- Mørk baggrund: `var(--color-surface)`
- Subtil top-border: `1px solid var(--color-border)`
- Ikon + tekst per fane
- Aktiv state: ikon og tekst i `var(--color-primary)` (#22C55E)
- Inaktiv state: `var(--color-text-muted)`
- Opret-fanen (`+`) fremhævet med grøn cirkel-baggrund
- Fast placering: `position: fixed; bottom: 0; left: 0; right: 0`
- Safe area: `padding-bottom: env(safe-area-inset-bottom, 8px)`
- Max-width: 390px, centreret

## HTML-struktur (bevar eksisterende routerLink og routerLinkActive):
```html
<nav class="bottom-nav">
  <!-- Forside, Overblik, Opret(+), Brugere, Beskeder -->
  <!-- bevar eksisterende routerLink-værdier -->
</nav>
```

## SCSS-krav
```scss
.bottom-nav {
  position: fixed;
  bottom: 0; left: 50%; transform: translateX(-50%);
  width: 100%; max-width: 390px;
  background: var(--color-surface);
  border-top: 1px solid var(--color-border);
  display: flex;
  padding: 6px 0 env(safe-area-inset-bottom, 8px);
  z-index: 100;
}

.bottom-nav__item {
  flex: 1; display: flex; flex-direction: column;
  align-items: center; gap: 3px;
  color: var(--color-text-muted);
  text-decoration: none; font-size: 10px; font-weight: 500;
  transition: color 0.15s;
  svg { width: 22px; height: 22px; }
}

.bottom-nav__item.active { color: var(--color-primary); }

.bottom-nav__item--create svg {
  background: var(--color-primary);
  border-radius: 50%; padding: 5px;
  color: #000; width: 32px; height: 32px;
}
```

---

# Step 5 – Refaktor forsiden

## Filer: `home.component.html`, `home.component.css` (→ omdøb til `.scss`)

## Mål

Forsiden skal ligne et moderne fintech-dashboard med action cards og diskret dev-login panel.

## HTML-layout

```html
<div class="home">

  <!-- Header -->
  <header class="home__header">
    <div class="home__logo">💳 <span>PayBySharePay</span></div>
    <button class="home__notif">🔔</button>
  </header>

  <!-- Velkomst -->
  <section class="home__welcome">
    <h1>Hej 👋</h1>
    <p>Hvad vil du gøre i dag?</p>
  </section>

  <!-- 2x2 Action cards -->
  <section class="home__cards">
    @for (card of actionCards; track card.route) {
      <a class="action-card" [routerLink]="card.route"
         [style.--accent]="card.accent">
        <div class="action-card__icon" [style.background]="card.iconBg">
          <!-- SVG ikon -->
        </div>
        <span class="action-card__title">{{ card.label }}</span>
        <span class="action-card__sub">{{ card.subtitle }}</span>
      </a>
    }
  </section>

  <!-- Aktiv gruppe sektion (hvis logget ind) -->
  @if (auth.isLoggedIn()) {
    <section class="home__section">
      <h2 class="home__section-title">Dine grupper</h2>
      <!-- Vis seneste 1-3 ordrer som mini-cards -->
    </section>
  }

  <!-- Diskret dev-panel nederst -->
  <section class="home__dev-panel">
    <div class="dev-panel__header">⚙ Udvikler login</div>
    <p class="dev-panel__info">Denne login bruges kun til udvikling.</p>
    <!-- Dropdown + Log ind/Log ud knap -->
  </section>

</div>
```

## Action cards data (opdater i `.ts`):

```typescript
actionCards = [
  { label: 'Overblik',  subtitle: 'Se regninger',          route: '/orders',           accent: '#22C55E', iconBg: 'rgba(34,197,94,0.15)',   icon: 'chart'   },
  { label: 'Opret',     subtitle: 'Ny gruppebetaling',     route: '/orders/create',    accent: '#7C3AED', iconBg: 'rgba(124,58,237,0.15)',  icon: 'plus'    },
  { label: 'Brugere',   subtitle: 'Find personer',         route: '/find-participants',accent: '#06B6D4', iconBg: 'rgba(6,182,212,0.15)',   icon: 'users'   },
  { label: 'Beskeder',  subtitle: 'Dine beskeder',         route: '/messages',         accent: '#F59E0B', iconBg: 'rgba(245,158,11,0.15)',  icon: 'chat'    },
];
```

## CSS-krav

```scss
.home {
  background: var(--color-bg);
  min-height: 100dvh;
  padding-bottom: 80px; /* plads til bottom-nav */
}

.home__cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  padding: 0 var(--space-page);
}

.action-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 18px 14px;
  display: flex; flex-direction: column; gap: 8px;
  text-decoration: none; color: var(--color-text-primary);
  /* Subtil accent-border i bunden */
  border-bottom: 2px solid var(--accent);
}

.home__dev-panel {
  margin: 24px var(--space-page) 0;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-card);
  opacity: 0.75; /* diskret */
}
```

## Opgave

1. Opdater `home.component.html` med ny struktur.
2. Flyt dev-login-sektionen til bunden i et `.home__dev-panel`.
3. Opdater `home.component.css` med ovenstående dark-mode styling.
4. Opdater `actionCards` array i `.ts` med `subtitle` property.
5. Kør `ng build`.

---

# Step 6 – Refaktor Overblik-siden

## Filer: `orders.component.html`, `orders.component.scss`, `orders.component.ts`

## Mål

Gruppebetalinger skal vises som professionelle cards, ikke en teknisk liste.

## HTML-layout

```html
<div class="orders">

  <header class="orders__header">
    <h1>Overblik</h1>
    <button class="orders__filter">Alle ↓</button>
  </header>

  <!-- Ordre-tabs (scroll-row) -->
  <div class="orders__tabs"> ... </div>

  <!-- Aktiv ordre som card -->
  @if (activeOrderVm()) {
    <div class="order-card app-card">
      <!-- Deltager-accordion med avatar, navn, status-badge, order lines -->
    </div>
  }

  <!-- Tom state -->
  @if (!isLoading() && orders().length === 0) {
    <app-empty-state
      icon="📋"
      title="Ingen gruppebetalinger"
      subtitle="Opret din første gruppebetaling"
      actionLabel="Opret nu"
      (actionClick)="goCreate()"
    />
  }

  <!-- Sticky bundknap -->
  <div class="orders__footer">
    <button class="btn-primary" [disabled]="!allPaid()" (click)="finalPay()">
      @if (allPaid()) { Betal ordre til spisestedet }
      @else { Afventer alle deltagere… }
    </button>
  </div>

</div>
```

## Status-badges

Erstat hardkodede chip-klasser med `<app-status-badge [status]="p.status" />` komponenten fra Step 3.

## SCSS-krav

- Brug `var(--color-*)` tokens i stedet for hardkodede hex-farver
- `.orders__header` med flex + space-between
- `.order-card` bruger `.app-card` utility-klassen
- `.orders__footer` fixed i bunden med gradient-overlay

---

# Step 7 – Refaktor Opret gruppebetaling-siden

## Filer: `create-order.component.html`, `create-order.component.scss`

## Mål

Formularen skal føles som et guidet, moderne oprettelsesflow.

## HTML-struktur (bevar eksisterende Angular signals og bindings):

```html
<div class="create">

  <header class="create__header">
    <button class="create__back" (click)="goBack()">‹</button>
    <h1>Ny gruppebetaling</h1>
  </header>

  <!-- Formularfelter i mørke afrundede kort -->
  <div class="form-section">
    <label class="form-label">Titel</label>
    <input class="form-input" ... />
  </div>

  <!-- Kategori-chips -->
  <div class="form-section">
    <label class="form-label">Kategori</label>
    <div class="category-chips">
      @for (cat of categories; track cat.key) {
        <button class="chip" [class.chip--active]="selectedCategory() === cat.key"
                (click)="toggleCategory(cat.key)">
          {{ cat.icon }} {{ cat.label }}
        </button>
      }
    </div>
  </div>

  <!-- Spisestedsliste som card-list -->
  <!-- Deltagerliste med avatar + checkmark -->

  <!-- Sticky opret-knap -->
  <div class="create__footer">
    <app-primary-button
      label="Opret gruppebetaling"
      [disabled]="!canSubmit()"
      [loading]="isSubmitting()"
      (clicked)="submit()"
    />
  </div>
</div>
```

## Kategori-chips styling:

```scss
.category-chips {
  display: flex; flex-wrap: wrap; gap: 8px;
}

.chip {
  padding: 7px 14px;
  border-radius: var(--radius-md);
  border: 1.5px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text-secondary);
  font-size: 14px; cursor: pointer;
  transition: all 0.15s;
}

.chip--active {
  border-color: var(--color-primary);
  background: rgba(34,197,94,0.1);
  color: var(--color-primary);
}
```

## Opgave

1. Opdater styling til at bruge CSS custom properties.
2. Opret kategori-chips med aktiv grøn border.
3. Brug `<app-primary-button>` til submit-knappen.
4. Bevar eksisterende Angular signal-bindings uændret.
5. Kør `ng build`.

---

# Step 8 – Refaktor Beskeder-siden

## Filer: `messages.component.html`, `messages.component.ts`

## Mål

Beskeder-siden må ikke se tom og ufærdig ud.

## HTML (simpel):

```html
<div class="messages">

  <header class="messages__header">
    <h1>Beskeder</h1>
  </header>

  <!-- Empty state -->
  <div class="messages__empty">
    <app-empty-state
      icon="💬"
      title="Ingen beskeder endnu"
      subtitle="Beskeder vises her, når der er aktivitet i dine gruppebetalinger."
    />
  </div>

</div>
```

## SCSS:

```scss
.messages {
  background: var(--color-bg);
  min-height: 100dvh;
  padding-bottom: 80px;
}

.messages__header {
  padding: 20px var(--space-page) 12px;
  h1 { font: var(--font-page-title); color: var(--color-text-primary); }
}

.messages__empty {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
}
```

## TypeScript:

Tilføj `styleUrl: './messages.component.scss'` og importer `EmptyStateComponent`.

---

# Step 9 – Refaktor Find personer & steder

## Filer: `find-participants.component.html`, `find-participants.component.css`

## Mål

Siden skal understøtte både personer og merchants/spisesteder med tabs og professionelt listedesign.

## HTML-struktur:

```html
<div class="find">

  <header class="find__header">
    <h1>Find personer & steder</h1>
  </header>

  <!-- Søgefelt -->
  <div class="find__search">
    <input class="form-input" type="search" placeholder="Søg navn eller sted…" />
  </div>

  <!-- Tabs: Personer | Spisesteder -->
  <div class="find__tabs">
    <button class="tab" [class.tab--active]="activeTab() === 'persons'"
            (click)="setTab('persons')">
      👤 Personer
    </button>
    <button class="tab" [class.tab--active]="activeTab() === 'merchants'"
            (click)="setTab('merchants')">
      🍽️ Spisesteder
    </button>
  </div>

  <!-- Liste -->
  <div class="find__list">
    @for (e of filteredList(); track e.id) {
      <div class="person-row" [class.person-row--selected]="e.selected"
           (click)="toggle(e)">
        <app-avatar [name]="e.displayName" [icon]="e.type === 'Merchant' ? '🍽️' : undefined" />
        <div class="person-row__info">
          <span class="person-row__name">{{ e.displayName }}</span>
          <span class="person-row__sub">{{ e.handle }}</span>
          @if (e.type === 'Merchant') {
            <span class="badge badge--orange">SPISESTED</span>
          }
        </div>
        <div class="person-row__check" [class.person-row__check--on]="e.selected">
          @if (e.selected) { ✓ }
        </div>
      </div>
    }
  </div>

  <!-- CTA knap -->
  @if (selectedCount() > 0) {
    <div class="find__footer">
      <app-primary-button
        [label]="'Tilføj ' + selectedCount() + ' til din liste'"
        (clicked)="addSelected()"
      />
    </div>
  }

</div>
```

## TypeScript-tilføjelser i `.ts`:

```typescript
activeTab = signal<'persons' | 'merchants'>('persons');

filteredList = computed(() =>
  this.entries()
    .filter(e => this.activeTab() === 'merchants' ? e.type === 'Merchant' : e.type === 'Person')
    .filter(e => /* eksisterende søg-filter */)
);

setTab(tab: 'persons' | 'merchants'): void {
  this.activeTab.set(tab);
}
```

## SCSS-krav:

```scss
.find__tabs {
  display: flex; gap: 8px;
  padding: 0 var(--space-page) 12px;
}

.tab {
  flex: 1; padding: 10px;
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-text-muted);
  font-size: 14px; font-weight: 500;
  cursor: pointer; transition: all 0.15s;
}

.tab--active {
  border-color: var(--color-primary);
  background: rgba(34,197,94,0.1);
  color: var(--color-primary);
}
```

---

# Step 10 – Refaktor Ordredetalje-siden

## Filer: `order-detail.component.html`, `order-detail.component.scss`

## Mål

Detaljesiden skal vise deltager-accordion mere professionelt med korrekte status-badges.

## Vigtige ændringer:

1. Brug `<app-status-badge [status]="p.status" />` i stedet for hardkodede chip-klasser.
2. Brug `<app-avatar [name]="p.participantName" size="md" />`.
3. Brug CSS custom properties i stedet for hardkodede farver.
4. Tilføj accordion CSS-transition i stedet for `display: none` → `display: block`:

```scss
.order-lines {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.25s ease;
}

.order-lines--open {
  max-height: 500px; /* tilstrækkeligt stort */
}
```

5. Bevar eksisterende `expandedIds` Set-logik i `.ts`.

---

# Step 11 – Mikrointeraktioner og polish

## Tilføj disse i relevant komponent-scss

```scss
/* Pressed state på cards */
.action-card:active,
.person-row:active,
.participant:active {
  transform: scale(0.98);
  opacity: 0.85;
}

/* Smooth kategori-chip transition */
.chip { transition: border-color 0.15s, background 0.15s, color 0.15s; }

/* Knap loading spinner */
.btn-primary.loading::after {
  content: '';
  width: 16px; height: 16px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
  display: inline-block; margin-left: 8px;
  vertical-align: middle;
}

@keyframes spin { to { transform: rotate(360deg); } }
```

---

# Step 12 – Responsiv mobiltest

## Test-tjekliste (Angular dev server + Chrome DevTools)

Kontrollér ved 360px, 390px og 412px bredde:

- [ ] Ingen tekst klippes uhensigtsmæssigt
- [ ] Bottom-nav skjuler ikke indhold (tilstrækkelig `padding-bottom` på sider)
- [ ] Formularer kan scrolles med keyboard åbent
- [ ] Cards har ens `padding` og `border-radius` på tværs af sider
- [ ] Farver er læsbare i dark mode
- [ ] Action cards er ens store i 2x2 grid
- [ ] Status-badges er synlige i alle tilstande
- [ ] Empty states er centrerede og fylder rimelig plads
- [ ] Accordion-animation er jævn

---

# Step 13 – Oprydning og færdiggørelse

## Opgave

1. Fjern døde CSS-klasser og dubletter
2. Sørg for at alle komponenter bruger `var(--color-*)` og `var(--radius-*)` i stedet for hardkodede værdier
3. Fjern eventuelle `!important`-regler
4. Kør `ng build` og verificér nul fejl
5. Kør `ng build --configuration production` og verificér bundle-størrelse

## Output

Skriv en opsummering med:
- Ændrede filer
- Nye komponenter oprettet
- Skærme refaktoreret
- Eventuel funktionalitet der bør testes manuelt

---

# Prompt til Copilot

Kopiér denne prompt ind i Copilot Agent sammen med denne markdown-fil:

```text
Du skal refaktorere UI/UX i PayBySharePay Angular-appen ud fra filen PayBySharePay-UI-UX-Angular-Guide.md.

Projektet er Angular 19 med standalone components, .html/.scss/.ts filer og Angular signals.
Global styling er i src/styles.css. Komponenter er i src/app/features/ og src/app/layout/.

Målet er ikke at ændre forretningslogik, services, API-kald eller routing. Målet er at løfte UI'et til et professionelt dark-mode design med:
- CSS custom properties (design tokens) i styles.css
- Genbrugelige standalone components i src/app/shared/components/
- Konsistent spacing, farver og border-radius på tværs af alle sider
- Professionel bundnavigation med aktiv grøn accent
- Modern dashboard-forside med diskret dev-panel nederst
- Flot empty state på Beskeder-siden
- Tab-navigation (Person/Merchant) på Find-siden
- Smooth accordion-animation på Overblik og Ordredetalje

Udfør guiden step by step. Efter hvert step skal ng build fortsat lykkes.
Bevar eksisterende Angular signal-bindings, routerLink-værdier og service-kald uændret.
Brug replace_string_in_file og create_file tools – vis ikke store kodeblokke i chatten.
```

---

# Definition of Done

UI-refactoren er færdig, når:

- [ ] `styles.css` indeholder dark-mode CSS custom properties og utility-klasser
- [ ] 5 delte standalone komponenter er oprettet i `src/app/shared/components/`
- [ ] `bottom-nav` er mørk med grøn aktiv accent og safe-area padding
- [ ] `home` ligner et moderne dashboard med 2x2 action cards og diskret dev-panel
- [ ] `orders` viser gruppebetalinger som professionelle cards med status-badges
- [ ] `create-order` har moderne kategori-chips og sticky submit-knap
- [ ] `messages` har en flot empty state via `EmptyStateComponent`
- [ ] `find-participants` har Person/Merchant tab-navigation
- [ ] `order-detail` bruger accordion med CSS-transition
- [ ] Alle sider bruger `var(--color-*)` tokens – ingen hardkodede farver
- [ ] `ng build` og `ng build --configuration production` lykkes uden fejl
