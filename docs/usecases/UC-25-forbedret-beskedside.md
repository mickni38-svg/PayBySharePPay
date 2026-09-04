# UC-25: Forbedret beskedside med merchant-logo, bestillingslink, læst-status og lyd

## Status

Planlagt.

## Formål

Gøre PayNSyncs beskedside mere overskuelig og handlingsorienteret, så en deltager hurtigt kan se nye og tidligere beskeder, identificere hvilken merchant en gruppebestilling tilhører og gå direkte til merchantens bestillingsside.

Beskedsiden skal følge PayNSyncs eksisterende visuelle stil, men uden en særskilt sidetitel med teksten **"Beskeder"**. Farver, baggrunde, tekstfarver, kanter, active states og øvrige visuelle elementer skal følge det theme, som brugeren har valgt.

## Brugerhistorie

Som deltager vil jeg kunne se, hvilke beskeder der er nye, få en tydelig lyd når en ny besked modtages og kunne gå direkte fra en gruppebestillingsbesked til merchantens bestillingsside.

## Aktører

- Primær aktør: indlogget PayNSync-bruger/deltager.
- Sekundær aktør: merchant med ekstern bestillingsside.

## Forudsætninger

- Brugeren er logget ind i PayNSync.
- Brugeren kan modtage beskeder i PayNSync.
- Gruppebestillinger er knyttet til en merchant.
- Merchant kan have et logo og en ekstern bestillings-URL.
- Brugerens valgte PayNSync-theme er tilgængeligt for beskedsiden.

## Trigger

Brugeren modtager en ny besked eller åbner beskedfanen i PayNSync.

## Hovedforløb

1. Brugeren åbner beskedsiden via navigationen nederst.
2. Siden viser ikke en separat overskrift med teksten **"Beskeder"**.
3. Øverst vises filtrene:
   - Alle
   - Bestillinger
   - Beskeder/System efter eksisterende kategorisering
4. Det aktive filter fremhæves med farver fra det aktuelt valgte theme.
5. Beskeder vises som individuelle afrundede cards.
6. Gruppebestillingsbeskeder viser merchantens logo til venstre.
7. En gruppebestillingsbesked viser mindst:
   - gruppebestillingens titel
   - merchantens navn
   - kort beskedtekst
   - dato/tid
   - tydelig handling: **Bestil her**
8. **Bestil her** åbner merchantens registrerede eksterne bestillingsside.
9. Brugeren skal ikke først ind på en separat PayNSync-detaljeside for at komme til merchantens bestillingsside.
10. Når en ny besked modtages, skal den visuelt fremstå som ulæst.
11. Når brugeren åbner eller på anden måde interagerer med beskeden, markeres den som læst.
12. Læste og ulæste beskeder skal kunne skelnes tydeligt visuelt uden at bryde det valgte theme.
13. Når en ny besked modtages, afspilles en kort notifikationslyd, så brugeren får auditiv feedback.
14. Eksisterende almindelige beskeder og systembeskeder fortsætter med at blive vist på samme side.

## UI-krav

- Ingen separat sidetitel med teksten **"Beskeder"**.
- PayNSync-header/logo og eksisterende bottom navigation bibeholdes.
- Beskedfanen i bottom navigation vises som aktiv.
- Beskedlisten skal bestå af separate cards med afrundede hjørner.
- Merchant-logo vises på gruppebestillingsbeskeder i stedet for et generisk madikon.
- Dato/tid placeres tydeligt i cardet uden at konkurrere med beskedens titel.
- **Bestil her** skal være visuelt tydelig som handling.
- Ingen hardcodede dark/light/minimal-farver må bruges specifikt til beskedsiden.
- Alle relevante farver skal komme fra PayNSyncs eksisterende theme-system.
- Ulæste beskeder skal have en tydelig theme-kompatibel markering, fx stærkere tekstvægt, accentkant, indikator/dot eller tilsvarende.
- Læste beskeder skal være tydeligt mindre fremhævede end ulæste beskeder, men stadig fuldt læsbare.
- Layoutet skal fungere på mobil/PWA.

## Læst/ulæst-status

- Nye beskeder oprettes som ulæste.
- Ulæst-status skal være synlig direkte i beskedlisten.
- Når brugeren åbner/interagerer med beskeden, gemmes den som læst.
- Læst-status skal bevares efter refresh og nyt login.
- En læst besked må ikke automatisk blive ulæst igen.
- Filtrering eller theme-skift må ikke ændre beskedens læst-status.

## Notifikationslyd

- Når klienten modtager en ny besked, afspilles en kort notifikationslyd.
- Lyden skal kun afspilles én gang pr. ny besked.
- Genindlæsning af siden må ikke afspille lyd for allerede kendte/læste beskeder.
- Implementeringen skal respektere browserens/enhedens begrænsninger for lyd og autoplay.
- Hvis lyd ikke kan afspilles pga. browser- eller enhedsbegrænsninger, skal beskeden stadig modtages og markeres som ulæst.
- Eksisterende brugerindstillinger for notifikationer skal respekteres, hvis de allerede findes.

## Merchant-logo

- Gruppebestillingsbeskeder viser merchantens logo som primært ikon.
- Logoet skal skaleres uden deformation.
- Hvis merchant ikke har et logo, vises et neutralt fallback merchant-/restaurantikon.
- Der må ikke vælges et generisk pizza-, fisk- eller andet madikon ud fra gruppebestillingens titel.

## Bestil her

- **Bestil her** er et direkte link til merchantens eksterne bestillingsside.
- Linket skal være knyttet til den merchant, som den konkrete gruppebestilling tilhører.
- Brugeren skal kunne forstå, at handlingen fører til merchantens bestillingsside.
- Hvis bestillingen ikke længere er aktiv, må **Bestil her** ikke fremstå som en aktiv handling.

## Alternative flows

### A1 – Merchant mangler logo

Et neutralt fallback-ikon vises. Beskeden og eventuelt bestillingslink fungerer fortsat normalt.

### A2 – Merchant mangler bestillingslink

**Bestil her** skjules eller deaktiveres. Brugeren må ikke sendes til en tom eller ugyldig URL.

### A3 – Bestillingslink kan ikke åbnes

PayNSync viser en forståelig fejlbesked og beholder brugeren i appen.

### A4 – Gruppebestillingen er afsluttet

Beskeden kan fortsat vises, men **Bestil her** vises ikke som aktiv handling.

### A5 – Notifikationslyd kan ikke afspilles

Beskeden modtages stadig normalt og vises som ulæst. Manglende lyd må ikke blokere beskedflowet.

## Acceptkriterier

- **AC1:** Beskedsiden har ingen separat overskrift med teksten **"Beskeder"**.
- **AC2:** Alle farver på beskedsiden følger det theme, brugeren har valgt.
- **AC3:** Der introduceres ikke særskilte hardcodede farver, som bryder Minimal, Dark eller andre themes.
- **AC4:** Gruppebestillinger viser merchantens logo i stedet for et generisk madikon.
- **AC5:** Gruppebestillinger viser gruppebestillingens titel, merchantnavn, relevant tekst og dato/tid.
- **AC6:** En aktiv gruppebestilling med gyldigt merchant-link viser en tydelig **Bestil her**-handling.
- **AC7:** Klik på **Bestil her** åbner den korrekte merchant-bestillingsside.
- **AC8:** Brugeren behøver ikke åbne en ekstra PayNSync-detaljeside for at komme til merchantens bestillingsside.
- **AC9:** En ny besked vises tydeligt som ulæst.
- **AC10:** Når brugeren åbner/interagerer med beskeden, markeres den som læst.
- **AC11:** Læst-status gemmes persistent og overlever refresh og nyt login.
- **AC12:** Læste og ulæste beskeder kan tydeligt skelnes visuelt i alle understøttede themes.
- **AC13:** Der afspilles én kort notifikationslyd, når en ny besked modtages, når browser/enhed tillader det.
- **AC14:** Refresh eller genåbning af appen må ikke afspille lyd igen for allerede kendte beskeder.
- **AC15:** Manglende mulighed for at afspille lyd må ikke forhindre beskeden i at blive vist.
- **AC16:** Merchant uden logo får et neutralt fallback-ikon.
- **AC17:** Manglende eller ugyldig merchant-URL må ikke navigere brugeren til en tom/ugyldig side.
- **AC18:** Afsluttede gruppebestillinger viser ikke **Bestil her** som aktiv handling.
- **AC19:** Filtrene fungerer fortsat korrekt.
- **AC20:** Almindelige beskeder og systembeskeder fortsætter med at fungere.
- **AC21:** Layoutet fungerer på mobil/PWA og passer ind i PayNSyncs eksisterende navigation.

## Ikke i scope

- Redesign af andre hovedsider.
- Ændring af selve merchantens eksterne bestillingsside.
- Push-notifikationer via native iOS/Android som selvstændig feature, medmindre dette allerede findes i den nuværende notifikationsinfrastruktur.
