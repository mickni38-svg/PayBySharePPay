# Copilot Claude Sonnet 4.6 Prompt

Læs først de uploadede paybysharepay AI-context dokumenter.

Opgave:
Implementér MVP-flowet for gruppebetaling med dummy merchant/pizza integration.

Flow:
1. Host opretter gruppebetaling
2. Merchant vælges
3. Deltagere vælges
4. ParticipantToken genereres
5. Personlige pizza-links genereres
6. Deltager vælger pizza
7. Deltager klikker “Gruppebetaling”
8. Merchant sender ordredata til paybysharepay API
9. paybysharepay gemmer participant order
10. Når alle deltagere er klar:
    - GroupPayment status = ReadyToPay
11. Host klikker “Betal nu”
12. GroupPayment status = Completed/Paid

Regler:
- Merchant er ikke UserEntity
- Merchant må ikke være GroupPaymentMember
- Brug participantToken/joinToken
- Skriv hele filer
- Ingen pseudo-kode
- Brug eksisterende patterns og namespaces

Start med analyse:
1. Hvad findes allerede?
2. Hvad mangler?
3. Hvilke entities/tabeller mangler?
4. Hvilke endpoints mangler?
5. Hvilken rækkefølge anbefales?

Implementér kun første trin:
- entities
- DbContext ændringer
- DTOs
- første API fundament

Implementér ikke hele UI endnu.

Giv:
- migrationskommandoer
- testtrin
