# UC-04-S06 — Toggle notifikationer lokalt

**Use Case:** [UC-04 — Opdater Profil](../usecases/UC-04-opdater-profil.md)  
**Type:** Alternativt forløb  
**Status:** ✅ Implementeret (med kendte gaps)  

---

## Beskrivelse

Som en bruger på profilsiden  
Vil jeg kunne slå notifikationer til og fra  
Så jeg selv styrer om jeg ønsker at modtage notifikationer.

---

## Acceptkriterier

- [ ] Brugeren kan se en notifikations-toggle på `/profile`.
- [ ] Toggle inverterer `notificationsEnabled` signal ved tryk.
- [ ] Den nye præference gemmes i `localStorage` under nøglen `sbys_notifications_enabled`.
- [ ] Præferencen bevares ved genindlæsning af siden.

---

## Tekniske detaljer

- **Frontend:** `ProfileComponent.toggleNotifications()` — ingen API-kald
- **Lagring:** `localStorage` (`sbys_notifications_enabled`)

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G4 | Notifikationspræference gemmes kun lokalt — nulstilles ved logout/ryd cache, synkroniseres ikke på tværs af enheder | 🟡 Medium |

---

## Relaterede stories

- [UC-04-S02 — Gem profilændringer](UC-04-S02-gem-profilaendringer.md)
