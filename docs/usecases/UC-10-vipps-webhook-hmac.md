# UC-10: Verificér Vipps MobilePay-webhooks med HMAC

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** SECURITY_FIX
- **Størrelse:** Afgrænset ekstern integration med høj sikkerhedsrisiko

## Mål

PayNSync må kun behandle Vipps MobilePay-webhook-events, når requestens payload og afsender er verificeret efter den officielle Webhooks API HMAC-protokol.

## Autoritativ specifikation

Implementeringen skal følge Vipps MobilePays officielle dokumentation:  
`https://developer.vippsmobilepay.com/docs/APIs/webhooks-api/request-authentication/`

Der må ikke opfindes en forenklet HMAC-protokol. Hemmeligheden er webhookens unikke `secret`, som returneres ved registrering; den er ikke automatisk det samme som Vipps client secret.

## Forudsætninger og beslutningsgate

Læs repo-instruktionerne, security-workflowet, eksisterende Vipps controllers/configuration samt den aktuelle officielle dokumentation. Før implementering skal planen dokumentere, om PayNSync bruger Webhooks API-registrering eller et andet callback-produkt. Stop og bed Product Owner om afklaring, hvis de eksisterende callbacks ikke leverer de krævede HMAC-headere.

## Scope

- Ny secret-konfiguration via environment/GitHub Secret; ingen secret i repository eller frontend.
- Verificér rå request-body mod `x-ms-content-sha256`.
- Verificér `Authorization` med HMAC-SHA256 over præcis metode, path+query, `x-ms-date`, host og content hash som i den officielle specifikation.
- Brug konstant-tids-sammenligning.
- Afvis manglende eller ugyldige headers/signaturer, før DTO-deserialisering udløser stateændringer.
- Indfør et dokumenteret tidsvindue for `x-ms-date` for at begrænse replay; stop for beslutning hvis officiel vejledning og eksisterende drift ikke giver et sikkert valg.
- Log årsag uden at logge secret, fuld Authorization-signatur eller følsom payload.
- Bevar idempotent behandling af gyldige events.

## Acceptkriterier

### AC1 – Gyldig webhook

En request med korrekt body-hash, HMAC-signatur og acceptabel dato behandles præcis én gang efter eksisterende regler.

### AC2 – Manipuleret body

Hvis body ændres efter signering, returneres 401/403, og ingen betalings- eller ordrestatus ændres.

### AC3 – Ugyldig eller manglende signatur

Requesten afvises uden databaseændring og uden provider-/merchant-callback.

### AC4 – Secrets

Webhook-secret hentes kun fra sikker serverkonfiguration. Build/deploy må ikke udskrive værdien.

## Test

- Officielt kendt signeringseksempel som deterministisk unit test.
- Gyldig signatur, forkert content hash, forkert signatur, manglende headers og for gammel dato.
- Bevis at service/repository ikke kaldes ved afvisning.
- Ingen live Vipps-kald.

## Ikke en del af use casen

- Ny webhook-registreringsportal.
- Ændring af reserve/capture-logik.
- Merchantens udgående callback-signering.
- Nye kryptografi-dependencies uden særskilt godkendelse.
