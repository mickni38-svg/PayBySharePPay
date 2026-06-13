# UC-04-S05 — Beskyt profil-endpoints med autorisation

**Use Case:** [UC-04 — Opdater Profil](../usecases/UC-04-opdater-profil.md)  
**Type:** Gap-story (G1 + G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som en bruger med en konto  
Vil jeg at kun jeg selv kan hente og opdatere mine profiloplysninger  
Så andre brugere ikke kan læse eller ændre min profil.

---

## Baggrund

`GET /api/participants/{id}` og `PUT /api/participants/{id}/profile` har ingen `[Authorize]`-attribut. Enhver — herunder ikke-autentificerede klienter — kan hente og opdatere vilkårlige profiler. Derudover valideres det ikke at JWT'ens `sub`-claim matcher `{id}` i URL'en, så en logget-ind bruger kan opdatere andres profiler (G1 + G2 i UC-04).

---

## Acceptkriterier

- [ ] `GET /api/participants/{id}` kræver et gyldigt JWT (`[Authorize]`).
- [ ] `PUT /api/participants/{id}/profile` kræver et gyldigt JWT (`[Authorize]`).
- [ ] `PUT`-endpoint validerer at JWT'ens `sub`-claim (brugerens ID) matcher `{id}` i URL'en.
- [ ] Kald med forkert `{id}` returnerer `HTTP 403 Forbidden`.
- [ ] Uautentificerede kald returnerer `HTTP 401 Unauthorized`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `ParticipantsController` | Tilføj `[Authorize]` på klasse- eller metode-niveau |
| `ParticipantsController.UpdateProfile()` | Valider `User.FindFirst("sub").Value == id.ToString()` |

---

## Relaterede stories

- [UC-04-S01 — Hent og vis profil](UC-04-S01-hent-og-vis-profil.md)
- [UC-04-S02 — Gem profilændringer](UC-04-S02-gem-profilaendringer.md)
