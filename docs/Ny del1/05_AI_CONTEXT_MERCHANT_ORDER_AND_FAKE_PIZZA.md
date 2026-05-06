# SBYS AI Context — Merchant Order and Fake Pizza Demo

## Formål
Der skal kunne simuleres et pizzawebsted, hvor en bruger vælger mad og klikker “Betal som gruppe”.

## Vigtigt flow
1. Host opretter gruppebetaling i SBYS
2. Host vælger merchant/spisested
3. SBYS genererer link til merchantens GroupOrderUrl
4. Linket indeholder groupPaymentId og joinToken
5. Bruger åbner pizzaweb
6. Bruger vælger mad
7. Bruger klikker “Betal som gruppe”
8. Merchant sender ordredata til SBYS
9. SBYS gemmer ordredata og viser dem i app/web frontend
10. Merchant sender senere status pr. deltager
11. Når alle er reserveret/betalt, sender SBYS “ready/all authorized” til merchant

## URL-format
Anbefalet format:

```text
{MerchantGroupOrderUrl}?sbysGroup={groupPaymentId}&sbysJoin={joinToken}
```

Eksempel:

```text
https://pizzapalace.dk/order?sbysGroup=12345&sbysJoin=abc123
```

## Merchant sender ved klik på “Betal som gruppe”
Merchant skal sende:

- merchantId
- groupPaymentId
- joinToken
- merchantOrderDraftId
- items
- subtotalAmount
- totalAmount
- currency
- expiresAtUtc
- paymentMode = AuthorizeThenCapture/ManualCapture
- status = Draft/Collecting

## Order items
Hver ordrelinje bør indeholde:
- lineId
- name
- quantity
- unitPrice
- lineTotal

## Payment handling
Model A:
- Merchant laver autorisation/reservation via eget PSP
- Merchant sender status til SBYS
- SBYS viser status
- SBYS frigiver først ordre når alle har betalt/reserveret

## Fake pizza demo
Til MVP kan man lave en fake merchant side eller endpoint, som:
- har hardcoded pizza items
- tager groupPaymentId/joinToken
- sender ordre til SBYS
- simulerer Authorized/Paid status
