# Deploy til Simply.com (paynsync.dk)

## Serverinfo
| Felt | Værdi |
|------|-------|
| Hosting | Simply Enterprise Suite (Windows IIS) |
| Webserver | nt31.unoeuro.com (IP: 93.191.156.14) |
| Domæne | paynsync.dk |
| API-subdomain | api.paynsync.dk |
| Merchant-subdomain | merchant.paynsync.dk |
| MySQL server | mysql73.unoeuro.com (bruges ikke endnu) |

---

## Oversigt

| Del | Teknologi | FTP-placering |
|-----|-----------|---------------|
| API | .NET 9 self-contained (win-x64) via IIS/ANCM | `/api.paynsync.dk/` |
| Frontend | Angular 19 (statiske filer) | `/www.paynsync.dk/` |
| MerchantDemo | Statisk HTML | `/merchant.paynsync.dk/` |

---

## ⚠️ Vigtig forudsætning: ASP.NET Core Module (ANCM)

API'en kræver **AspNetCoreModuleV2** installeret på IIS-serveren.  
Simply Enterprise Suite (Windows) understøtter primært .NET Framework 4.8.

**Gør følgende inden første deploy:**
1. Kontakt Simply support: *"Understøtter jeres Enterprise Suite (Windows) ASP.NET Core Module v2 (ANCM) til .NET 9?"*
2. Alternativt: Test ved at uploade og se om `web.config`-konfigurationen virker.

Hvis ANCM ikke er tilgængeligt, kan frontend og MerchantDemo stadig deployes som statiske filer, men API'en skal hostes et andet sted (fx Hetzner VPS ~€4/md, Railway eller Render).

---

## 1. Opret subdomæner i Simply-panelet

Inden deploy skal du oprette subdomæner i Simply-kontrolpanelet:
1. Gå til **Website → Subdomæner**
2. Opret `api.paynsync.dk` → peg på den korrekte mappe
3. Opret `merchant.paynsync.dk` → peg på den korrekte mappe
4. Notér de FTP-mappestier Simply tildeler (bruges i workflow'et)

**Tjek FTP-struktur** med en FTP-klient (fx FileZilla):
- Server: `nt31.unoeuro.com`
- Bekræft at `/api.paynsync.dk/`, `/www.paynsync.dk/` og `/merchant.paynsync.dk/` eksisterer

Opdater `server-dir` i `.github/workflows/deploy-simply.yml` hvis stierne er anderledes.

---

## 2. GitHub Secrets

Tilføj følgende under **GitHub → Settings → Secrets → Actions**:

| Secret | Beskrivelse | Hvor finder du det |
|--------|-------------|-------------------|
| `SIMPLY_FTP_USERNAME` | FTP-brugernavn | Simply panel → "Loginoplysninger" |
| `SIMPLY_FTP_PASSWORD` | FTP-adgangskode | Simply panel → "Loginoplysninger" |
| `SIMPLY_DB_CONNECTION_STRING` | SQL Server forbindelsestreng | Din externe SQL Server-host |
| `SIMPLY_JWT_KEY` | JWT signing key (min. 32 tegn) | Vælg selv, fx `pwsh -c "[System.Web.Security.Membership]::GeneratePassword(48,8)"` |

---

## 3. Deploy via GitHub Actions

Kør workflow manuelt:  
**GitHub → Actions → Deploy til Simply.com (paynsync.dk) → Run workflow**

Workflowet:
1. Bygger API som self-contained **win-x64** (ingen .NET krævet på serveren)
2. Injicerer `ConnectionStrings` og `Jwt:Key` i `appsettings.Simply.json` via `jq`
3. Bygger Angular med `--configuration simply`
4. Uploader alle filer via FTP til nt31.unoeuro.com

---

## 4. Database

Simply tilbyder MySQL (`mysql73.unoeuro.com`), men appen bruger SQL Server (EF Core).  
Muligheder:
- **Behold ekstern SQL Server** (fx Azure SQL Basic ~$5/md) — anbefales nu
- **Skift til MySQL**: Kræver Pomelo.EntityFrameworkCore.MySql + ny migration (fremtidigt step)

