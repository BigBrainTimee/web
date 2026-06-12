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

### 1. Baze podataka (Docker + 3 odvojene baze)

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Str0ng!Pass123" -p 1435:1433 --name web2 -d mcr.microsoft.com/mssql/server:2022-latest

Start-Sleep -Seconds 30

docker cp "d:\Moji projekti\web projekat\Migrations\001_AuthDb.sql" web2:/tmp/001_AuthDb.sql
docker cp "d:\Moji projekti\web projekat\Migrations\002_TravelDb.sql" web2:/tmp/002_TravelDb.sql
docker cp "d:\Moji projekti\web projekat\Migrations\003_BudgetDb.sql" web2:/tmp/003_BudgetDb.sql

docker exec web2 /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Str0ng!Pass123" -i /tmp/001_AuthDb.sql
docker exec web2 /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Str0ng!Pass123" -i /tmp/002_TravelDb.sql
docker exec web2 /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Str0ng!Pass123" -i /tmp/003_BudgetDb.sql
```

Ako kontejner već postoji: `docker start web2`

| Servis | Baza | Tabele |
|--------|------|--------|
| AuthService | `AuthDb` | Users |
| TravelService | `TravelDb` | TravelPlans, Destinations, Activities, ChecklistItems, ShareLinks |
| BudgetService | `BudgetDb` | Expenses |

### 2. Backend

1. Otvori `web projekat.sln` u Visual Studio
2. Publish → profil **Local.1Node**
3. Gateway: http://localhost:8080

### 3. Frontend

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
