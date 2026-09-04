# UC-23: Merchant-leveringskanaler

## Status

Planlagt.

## Formål

Lade merchant vælge, hvordan færdige PayNSync-ordrer skal modtages.

## Brugerhistorie

Som merchant vil jeg vælge ordrelevering via Order Hub, e-mail eller mit eksisterende system, så PayNSync passer til min drift.

## Funktionelt scope

- Indstillinger for Order Hub, e-mail og API/POS.
- Mulighed for at aktivere mere end én kanal.
- Læsbar e-mail med samlet ordre og betalingsstatus.
- E-mail kan anvendes som fallback til en primær kanal.

## Acceptkriterier

- Ordren leveres gennem merchantens aktive kanaler.
- E-mailen indeholder de nødvendige køkken- og leveringsoplysninger.
- Merchantens kanalindstillinger kan ændres uden at påvirke eksisterende ordrer.

## Ikke i scope

- SMS og tredjepartsleveringstjenester.
- Avancerede notifikationsregler.

