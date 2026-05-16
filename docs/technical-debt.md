# Teknisk gæld

## Sikkerhed

| Problem | Placering | Alvorlighed |
|---------|-----------|-------------|
| Ingen password-validering ved login | `AuthController.Login` | 🔴 Høj |
| JWT-nøgle placeholder i `appsettings.json` | `appsettings.json` | 🔴 Høj |
| JWT-nøgle og -konfiguration mangler i `appsettings.Production.json` | `appsettings.Production.json` | 🔴 Høj |
| Ingen rate limiting på login-endpoint | `AuthController` | 🟡 Medium |
| `appsettings.json` committed til Git med lokal connection string | Repo root | 🟡 Medium |
| `JoinToken` genereres men valideres/bruges ikke | `Order.JoinToken` | 🟡 Medium |

## Midlertidige løsninger

| Beskrivelse | Placering |
|-------------|-----------|
| Login uden password er bevidst MVP-valg, men udskyder reel auth | `AuthController` + kommentar i kode |
| Aktivitetsfeed bygges client-side fra ordredata i stedet for en rigtig aktivitetslog | `ActivityService` |
| `unreadCount` baseret på 24-timers vindue fremfor reel læst/ulæst-tracking | `ActivityService.buildFeed()` |
| Dev-login konti (`test1@dev.dk` osv.) seedet i prod | `Tools/Program.cs` SeedAsync |

## Manglende validering

| Beskrivelse | Placering |
|-------------|-----------|
| Ingen validering af email-format ved login/registrering (server-side) | `AuthController` |
| Ingen `[Required]`/`[StringLength]` data annotations på API request-DTOs | `Api/DTOs/*.cs` |
| Ingen frontend-validering ved oprettelse af ordre | `CreateOrderComponent` |

## Testdækning

- Testprojektet er tomt – se [testing.md](testing.md)

## Duplikering og uklarhed

| Beskrivelse | Placering |
|-------------|-----------|
| To `StatusCard` interface-definitioner eksisterede tidligere i `home.component.ts` (rettet, men indikerer manuelt koderodet) | `home.component.ts` |
| `orderId: null` blev brugt på `StatusCard`-objekter der ikke har `orderId` i interfacet – indikerer copy/paste | `home.component.ts` |
| `ActivityService` og aktivitetsfeed er udelukkende client-side logik, men placeret som en "service" der minder om en backend-service | `activity.service.ts` |

## Manglende features/flows

| Beskrivelse | Konsekvens |
|-------------|-----------|
| `JoinToken` på `Order` genereres men bruges ikke | Invitations-flow via link fungerer ikke |
| Ingen email-udsendelse | Påmindelser kan ikke sendes til deltagere |
| Ingen betalingsgateway | Betalinger registreres manuelt – ikke automatisk |
| Merchant-UI mangler | Merchants kan kun administreres via API/tools |

## Performance

| Beskrivelse | Alvorlighed |
|-------------|-------------|
| Frontend henter alle ordrer for en bruger og beregner aktivitetsfeed client-side – skalerer dårligt ved mange ordrer | 🟡 Medium |
| Ingen paginering på ordreliste | 🟡 Medium |
| `orders.component.scss` er stor (budget hævet til 20kB) | 🟢 Lav |

## Navngivning

- `UnitTest1.cs` er et autogenereret placeholderfilnavn
- `sbys_token` / `sbys_user` som localStorage-nøgler er uforklarede forkortelser

## Forslag til prioriteret oprydning

1. Implementer reel authentication (password + hashing)
2. Flyt JWT-nøgle til Azure Key Vault / miljøvariabel
3. Tilføj server-side validering (data annotations) på API request-DTOs
4. Skriv unit tests for service-laget
5. Implementer `JoinToken`-baseret invitations-flow
6. Lav backend aktivitetslog i stedet for client-side mapping
