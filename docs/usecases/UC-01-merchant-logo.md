# FEATURE 01: Merchant-logo i database og API

## Mål

En merchant skal kunne gemme og udskifte sit logo. Logoet skal kunne hentes af forsiden og gruppebetalingswizarden uden hardcodede billedreferencer.

## Instruks til Copilot Claude

Analysér først den eksisterende merchant-model, oprettelse, profilredigering, databaseadgang og API-struktur. Implementér derefter kun denne feature ved at udvide den nuværende løsning. Bevar arkitektur og kodestil, tilføj relevante tests, kør testene, og afslut med ændrede filer samt testresultat.

## Afgrænsning

Denne feature omfatter kun datamodel, upload/redigering og levering af merchant-logo. Den må ikke implementere carousel eller ændre wizardens sider.

## Krav

- Logo er obligatorisk ved oprettelse af en ny merchant.
- En eksisterende merchant uden logo skal fortsat kunne vises med initialer som fallback.
- Tilladte formater er PNG, JPEG og WebP.
- Maksimal filstørrelse er 1 MB.
- Format og størrelse valideres både i klienten og backend.
- Logoet vises kvadratisk med bevaret billedforhold og må ikke forvrænges.
- Billeddata og nødvendige metadata gemmes i databasen.
- Ved udskiftning erstattes det tidligere logo uden at efterlade forældreløse data.
- Logoet må ikke kopieres ind i en gruppebetaling. Det hentes gennem relationen til merchanten.

## Foreslået datamodel

Tilpas navnene til den eksisterende model og arkitektur frem for at oprette en parallel merchant-model.

| Felt | Type | Formål |
| --- | --- | --- |
| `LogoImageData` | `byte[]` / BLOB | Billedets binære data |
| `LogoContentType` | `string` | Eksempelvis `image/png` |
| `LogoFileName` | `string` | Oprindeligt filnavn |
| `LogoUpdatedAtUtc` | `DateTime?` | Cache-versionering |

## API

Genbrug eksisterende merchant-controller/service, hvis det passer med arkitekturen. Et muligt endpoint er:

```http
GET /api/merchants/{merchantId}/logo
```

Endpointet skal:

- validere adgang efter løsningens eksisterende regler;
- returnere korrekt `Content-Type`;
- understøtte browser-cache og cache-invalidering ved ændring;
- returnere en kontrolleret fallback eller et klart resultat for ældre merchants uden logo.

## Datakilder og eksisterende funktionalitet

- Merchantens rigtige ID og eksisterende profil skal benyttes.
- Der må ikke oprettes hardcodede merchants, testlogoer eller statiske billedstier i produktionskoden.
- Eksisterende merchant-oprettelse og profilredigering skal udvides; der må ikke bygges et parallelt flow.

## Acceptkriterier

### AC1 – Upload

**Givet** en ny merchant  
**Når** et gyldigt logo på højst 1 MB uploades  
**Så** gemmes logo og metadata på merchantens eksisterende post.

### AC2 – Validering

**Givet** en ugyldig MIME-type eller en fil over 1 MB  
**Når** upload forsøges  
**Så** afvises filen med en forståelig fejl uden delvis lagring.

### AC3 – Hent logo

**Givet** en merchant med logo  
**Når** logo-endpointet kaldes  
**Så** returneres billedet med korrekt indholdstype og cache-header.

### AC4 – Eksisterende merchant

**Givet** en ældre merchant uden logo  
**Når** merchanten vises  
**Så** kan klienten vise initialer som fallback uden at fejle.

### AC5 – Udskiftning

**Givet** en merchant med et eksisterende logo  
**Når** et nyt gyldigt logo gemmes  
**Så** vises det nye logo, og det gamle efterlades ikke som forældreløse data.

## Test

- Upload af PNG, JPEG og WebP.
- Ugyldig MIME-type, tom fil og fil over 1 MB.
- Merchant uden logo.
- Udskiftning af logo og cache-invalidering.
- Autorisation på hentning og opdatering.

## Ikke en del af denne feature

- Forsidens merchant-carousel.
- Wizardens trin.
- Ændring af betaling, reservation eller capture.
