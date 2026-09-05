# Implementation Plan — UC-22 Order Hub

## Scope
Implementér Order Hub som backend-capability oven på eksisterende MerchantOrder samt en tabletvenlig Merchant Order App i den eksisterende Angular PWA.

## Data
- Participant.OrderHubEnabled
- MerchantOrder.OrderHubStatus
- MerchantOrder.Note
- MerchantOrder.UpdatedAtUtc

## Backend
- Udvid MerchantOrderRepository med merchant-filtrerede kø/history-opslag.
- Tilføj IOrderHubService/OrderHubService med merchant- og adgangskontrol samt statusmaskine.
- Tilføj autentificeret OrderHubController baseret på JWT participant-id.
- Tilføj settings endpoint til manuel aktivering/deaktivering.
- Bevar ekstern MerchantOrderUrl-flow fra UC-21 som separat destination.

## Frontend
- Ny /order-hub route for Merchant.
- OrderHubService + DTO'er.
- Aktiv ordrekø, historik, statusknapper og lyd-toggle.
- Polling til nye ordrer; DB er source of truth ved reload.
- Genbrug eksisterende PWA/service worker.

## Out of scope
Ingen POS, printer, fakturering, WebSocket eller ny deployment.
