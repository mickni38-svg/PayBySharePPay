#   paybysharepay AI Context — Dummy Pizza Website

## Formål
Dummy pizza-websiden simulerer Pizzaria Roma.

## Funktioner
- læser URL query params
- viser simpel menu
- deltager vælger varer
- klik på “Gruppebetaling”

## Når “Gruppebetaling” klikkes
Dummy merchant sender ordredata til paybysharepay API.

paybysharepay skal gemme:
- groupPaymentId
- participant/member
- merchantId
- order items
- totalAmount
- status
- createdAtUtc

## MVP
Ingen rigtig PSP endnu.
Kun persistence + orchestration/status.
