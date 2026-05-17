# 02 – Arkitektur

## Arkitekturstil

PayBySharePPay er bygget som en **lagdelt N-tier arkitektur** med klar adskillelse mellem:

- **API-lag** (ASP.NET Core Web API)
- **Service-lag** (forretningslogik)
- **Data Access-lag** (EF Core + repositories)
- **Frontend** (Angular SPA + Vanilla MerchantDemo)

---

## Lagdiagram

```mermaid
flowchart TD
	subgraph Frontend
		A[Angular SPA\nFrontend.PayBySharePay]
		B[MerchantDemo\nFrontend.MerchantDemo]
	end

	subgraph API
		C[Api.PayBySharePay\nControllers + Middleware + Auth]
	end

	subgraph ServiceLayer
		D[Service.PayBySharePay\nOrderService, PaymentService, m.fl.]
	end

	subgraph DataLayer
		E[DataStorage.PayBySharePay\nDbContext + Repositories + Migrations]
	end

	subgraph Database
		F[(SQL Server\nAzure SQL i prod)]
	end

	A -- HTTP/JWT --> C
	B -- HTTP --> C
	C --> D
	D --> E
	E --> F
```

---

## Projektafhængigheder

```
Api.PayBySharePay
  ├── Service.PayBySharePay
  │     └── DataStorage.PayBySharePay
  │           └── (EF Core, SQL Server)
  └── (Auth: JWT, Swagger)

Tools.PayBySharePay
  └── DataStorage.PayBySharePay

Tests.PayBySharePay
  └── Service.PayBySharePay
```

---

## Backend/Frontend kommunikation

- Frontend kommunikerer via **HTTPS REST API**.
- JWT-token sendes med som `Authorization: Bearer <token>` header.
- MerchantDemo henter data via **offentligt (unauthenticated) `/api/merchant-orders`** endpoint.
- CORS er konfigureret til specifikke origins (localhost + Azure URLs).

---

## Dataflow – Opret ordre

```mermaid
sequenceDiagram
	participant U as Bruger
	participant FE as Angular Frontend
	participant API as ASP.NET Core API
	participant SVC as OrderService
	participant DB as Database

	U->>FE: Opretter ordre med deltagere
	FE->>API: POST /api/orders (JWT)
	API->>SVC: CreateOrderAsync(dto)
	SVC->>DB: Gemmer Order + OrderParticipants
	SVC->>DB: Sender beskeder til deltagere
	API-->>FE: 201 Created + OrderDto
	FE-->>U: Viser ordredetaljer
```

---

## Dataflow – Deltager betaler via link

```mermaid
sequenceDiagram
	participant D as Deltager
	participant MD as MerchantDemo
	participant API as ASP.NET Core API

	D->>MD: Åbner link med ?token=xxx&api=...
	MD->>API: GET /api/merchant-orders?token=xxx
	API-->>MD: Returnerer ordredetaljer
	D->>MD: Bekræfter betaling
	MD->>API: POST /api/merchant-orders/confirm
	API-->>MD: OK
```

---

## Deployment-arkitektur (Produktion)

```mermaid
flowchart LR
	GH[GitHub\nmain branch] --> Script[deploy-azure.ps1]
	Script --> SWA1[Azure Static Web Apps\nAngular Frontend]
	Script --> SWA2[Azure Static Web Apps\nMerchantDemo]
	Script --> AppService[Azure App Service\nASP.NET Core API]
	AppService --> AzSQL[(Azure SQL Database)]
```

---

## Sikkerhedsarkitektur

- JWT Bearer authentication på alle `/api/*` endpoints undtagen `/api/merchant-orders` og `/api/auth/*`
- CORS whitelist begrænser adgang fra ukendte origins
- HTTPS påkrævet i produktion (HTTP redirect aktiveret)
- Secrets opbevares i Azure App Service **Application Settings** (ikke i kode)

---

## Arkitekturbeslutninger

| Beslutning | Begrundelse | Konsekvens |
|---|---|---|
| Separat MerchantDemo frontend | Deltagere skal ikke logge ind – enkel HTML/JS | Ingen auth, offentlig URL |
| JWT over cookies | Stateless API, nem integration med SPA | Token skal fornyes/håndteres i frontend |
| EF Core Code First | Hurtig udvikling, migrations i kode | Migration-scripts skal køres manuelt ved prod-deploy |
| Tools.PayBySharePay console app | Fleksibelt seed/flush til dev og prod | Kræver DB-adgang fra devmaskine/CI |
| Azure Static Web Apps | Gratis hosting til SPA/static sites | Begrænset server-side funktionalitet |

---

## Se også

- [Projektstruktur](03-projektstruktur.md)
- [Backend](04-backend.md)
- [Authentication og security](08-authentication-security.md)
- [Azure deployment](11-azure-deployment-prod.md)
