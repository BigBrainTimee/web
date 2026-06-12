# Travel Planner — Planer putovanja

Web aplikacija za planiranje putovanja (mikroservisna arhitektura, React frontend, .NET backend, SQL Server).

## Preduslovi

- Windows 10/11
- Visual Studio 2022 (Azure Service Fabric SDK, ASP.NET)
- .NET 8 SDK
- Node.js 18+
- Docker Desktop
- SQL Server Management Studio (SSMS)

## Pokretanje

### 1. Baza podataka (Docker)

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Str0ng!Pass123" -p 1435:1433 --name web2 -d mcr.microsoft.com/mssql/server:2022-latest
```

Ako kontejner već postoji: `docker start web2`

### 2. Migracija (SSMS)

1. Poveži se na `localhost,1435` (korisnik `sa`, lozinka `Str0ng!Pass123`)
2. Pokreni skriptu `Migrations/001_InitialSchema.sql`

### 3. Backend

1. Otvori `web projekat.sln` u Visual Studio
2. Publish → profil **Local.1Node**
3. Gateway: http://localhost:8080

### 4. Frontend

```powershell
cd frontend
npm install
npm run dev
```

Aplikacija: http://localhost:5173

U `frontend/.env`:

```env
VITE_API_URL=http://localhost:8080
```

## Autori

- Ime Prezime
