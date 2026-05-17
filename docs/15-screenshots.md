# 15 – Screenshots

> 📸 **Screenshots er ikke taget automatisk.** Nedenfor er en liste over hvilke skærme der skal dokumenteres, og hvor billeder skal gemmes.

Gem alle screenshots i mappen: `docs/images/`

---

## TODO: Screenshots der mangler

### 1. Login-side
> TODO: Tag screenshot af login-siden og gem som `docs/images/01-login.png`

![Login](images/01-login.png)

*Login-siden – brugeren logger ind med email og password.*

---

### 2. Dashboard / Forside (Home)
> TODO: Tag screenshot af dashboard efter login og gem som `docs/images/02-home.png`

![Dashboard](images/02-home.png)

*Dashboard viser overblik over aktive ordrer, beskeder og genveje.*

---

### 3. Opret ordre
> TODO: Tag screenshot af "Opret ordre"-formularen og gem som `docs/images/03-create-order.png`

![Opret ordre](images/03-create-order.png)

*Formularen til at oprette en ny ordre med titel, kategori, besked og deltagere.*

---

### 4. Ordreoversigt
> TODO: Tag screenshot af listen over ordrer og gem som `docs/images/04-orders.png`

![Ordreoversigt](images/04-orders.png)

*Oversigt over alle ordrer brugeren har oprettet eller deltager i.*

---

### 5. Ordredetaljer
> TODO: Tag screenshot af en ordres detaljevisning og gem som `docs/images/05-order-detail.png`

![Ordredetaljer](images/05-order-detail.png)

*Detaljeret visning af en ordre: deltagere, betalingsstatus og overblik.*

---

### 6. Beskeder
> TODO: Tag screenshot af beskedindbakken og gem som `docs/images/06-messages.png`

![Beskeder](images/06-messages.png)

*Beskedindbakken – notifikationer og links til ordrer.*

---

### 7. Find deltagere
> TODO: Tag screenshot af søgefunktionen og gem som `docs/images/07-find-participants.png`

![Find deltagere](images/07-find-participants.png)

*Søg og tilføj deltagere til en ordre.*

---

### 8. Afventende deltagere
> TODO: Tag screenshot af oversigten over afventende betalinger og gem som `docs/images/08-pending-participants.png`

![Afventende deltagere](images/08-pending-participants.png)

*Oversigt over deltagere der endnu ikke har betalt.*

---

### 9. MerchantDemo – deltager-betalingsside
> TODO: Tag screenshot af MerchantDemo-siden (åbn med et test-token) og gem som `docs/images/09-merchant-demo.png`

![MerchantDemo](images/09-merchant-demo.png)

*Siden en deltager ser, når de modtager et betalingslink – viser ordreindhold og bekræft-knap.*

---

### 10. Seneste aktivitet
> TODO: Tag screenshot af aktivitets-oversigten og gem som `docs/images/10-activity.png`

![Aktivitet](images/10-activity.png)

*Oversigt over seneste aktivitet i løsningen.*

---

## Hvordan tager man screenshots?

1. Start løsningen lokalt (se [Lokal udvikling](10-lokal-udvikling.md))
2. Log ind med et testbrugernavn (seed data)
3. Naviger til den ønskede side
4. Tag screenshot med Windows: `Win + Shift + S` eller `Snipping Tool`
5. Gem billedet i `docs/images/` med det angivne filnavn
6. Fjern TODO-kommentaren fra denne fil

---

## Produktionsscreenshots

Produktions-URL'er til screenshots:

| Side | URL |
|---|---|
| Angular Frontend | `https://icy-water-0750d2703.7.azurestaticapps.net` |
| MerchantDemo | `https://ashy-bay-0e753db03.7.azurestaticapps.net?token=<token>&api=https://paybysharepay-api-win.azurewebsites.net` |
