# UC-03-S04 — Redirect til oprindelig URL efter re-login

**Use Case:** [UC-03 — Log ud](../usecases/UC-03-log-ud.md)  
**Type:** Gap-story (G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟢 Lav  

---

## Beskrivelse

Som en bruger der automatisk er blevet logget ud midt i en session  
Vil jeg efter re-login blive sendt tilbage til den side jeg var på  
Så jeg ikke mister mit arbejde og kan fortsætte der, hvor jeg slap.

---

## Baggrund

Når `apiInterceptor` udløser automatisk logout ved 401, gemmes den aktuelle URL ikke. Efter re-login havner brugeren altid på `/home`, uanset hvilken side de var på (G2 i UC-03).

---

## Acceptkriterier

- [ ] Når `apiInterceptor` omdirigerer til `/login`, gemmes den aktuelle URL (f.eks. i `sessionStorage` eller som query-parameter).
- [ ] Efter succesfuldt login navigeres brugeren til den gemte URL i stedet for `/home`.
- [ ] Hvis ingen gemt URL findes, bruges `/home` som fallback.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `apiInterceptor` | Gem `router.url` inden navigation til `/login` |
| `AuthService.login()` / login-komponent | Læs gemt URL og naviger hertil efter login |

---

## Relaterede stories

- [UC-03-S02 — Automatisk logout ved udløbet token](UC-03-S02-automatisk-logout-ved-udloebet-token.md)
