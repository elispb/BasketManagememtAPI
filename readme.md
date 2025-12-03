## Startup Instructions

1. **Run the SQL Server container**  
   - From the repository root, execute:  
     `docker compose -f docker-compose.tests.yml up -d`  
     This starts `mcr.microsoft.com/mssql/server:2022-latest` on port `1433` with the SA credentials the API expects.
   - Wait for the container’s health check to pass (check with `docker ps` to ensure the service is healthy).

2. **Apply the database schema and seed data**  
   - Change into the database project folder:  
     `cd Database\BasketManagementAPI.Database`  
   - Run the DbUp deployment tool that embeds all SQL scripts (creates tables, stored procedures, shipping costs, discount definitions, etc.):  
     `dotnet run --project BasketManagementAPI.Database.csproj`  
   - The tool uses `appsettings.json`’s `DefaultConnection` (`Server=localhost,1433;Database=BasketDb;User Id=sa;Password=Str0ng!Passw0rd;TrustServerCertificate=True;`).  
   - Confirm it prints “Database is up to date.” Scripts `018-Seed-ShippingCosts.sql` and `019-Seed-DiscountDefinitions.sql` insert the requested shipping and discount rows.

3. **Understand the seeded shipping and discount data**
   - `018-Seed-ShippingCosts.sql` seeds four countries with their `CountryCode` enum values and costs in minor units:
     - `United Kingdom` → `CountryCode.UnitedKingdom` (`1`) for `GB`/`UK`
     - `Germany` → `CountryCode.Germany` (`2`)
     - `United States` → `CountryCode.UnitedStates` (`3`)
     - `Australia` → `CountryCode.Australia` (`4`)
     When calling `/api/baskets/{id}/shipping` you can submit either a name (`UnitedKingdom`, `uk`) or the numeric value (`1`) and the service will normalize it before performing the lookup. The policy falls back to a default `1299` if the code is not seeded.
   - `019-Seed-DiscountDefinitions.sql` creates two active discount codes: `VACAY10` (10% travel promo) and `WELCOME15` (15% new customer welcome). Metadata on each row describes the source, so you can start applying those codes via `ApplyDiscount` as soon as the seed runs.

4. **Launch the API and open Swagger**  
   - From the repo root (or `BasketManagementAPI` folder), run:  
     `dotnet run --project BasketManagementAPI\BasketManagementAPI.csproj`  
   - Open `https://localhost:<port>/swagger/index.html` in your browser (port number is shown in the console output). The API uses the same connection string and now talks to the Docker SQL Server.

Let me know if you want a script to automate the setup or help creating launch configurations.

