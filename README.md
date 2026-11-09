# RaceDay.Api

ASP.NET Core Web API (Code-First EF Core) for the RaceDay system - Part 2.

## What's implemented so far

- **Project setup**: ASP.NET Core Web API, EF Core (SQL Server), Code-First.
- **Authentication & roles**: register, login, logout via cookie-based session
  authentication. Passwords hashed with `PasswordHasher<User>` (PBKDF2) - never
  stored in original form.
- **User Profile**: `GET /api/users/me`, `PUT /api/users/me` - open to both roles,
  each user can only see/edit their own profile (identity comes from the session,
  not a route parameter).
- **Swagger**: available at `/swagger` with no extra setup - cookie auth works
  with "Try it out" automatically because the browser sends the session cookie.

Events, Categories, Enrolments, Results, and unit tests are the next stages.

## Data model

The EF Core model in `Data/RaceDayDbContext.cs` matches `docs/schema.sql` from
Part 1 exactly: same six tables, same primary/foreign keys, same cardinalities,
same check constraints (`CK_Users_Role`, `CK_Events_EventType`, `CK_Events_Distance`),
and the same unique constraints (`Users.Email`, `UserProfiles.UserId`,
`Enrolments.(ParticipantId, EventId)`, `Results.EnrolmentId`).

`schema.sql` matches `erd.png` exactly - the same six entities, the same primary/foreign
keys, and the same cardinalities (1:1 for Users-UserProfiles and Enrolments-Results;
1:M everywhere else). No deliberate deviations were introduced between the diagram and
the script.

<img width="944" height="471" alt="image" src="https://github.com/user-attachments/assets/657a9763-3270-481e-a8db-0aad4d1769ba" />

## Running locally

1. **Restore packages**
   ```
   dotnet restore
   ```

2. **Set your connection string.** Update `appsettings.json`, or (recommended,
   so you never commit a real connection string) use user secrets:
   ```
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;"
   ```

3. **Create the database via EF Core migrations** (Code-First):
   ```
   dotnet tool install --global dotnet-ef   # once, if not already installed
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the API**
   ```
   dotnet run
   ```
   Then open `https://localhost:<port>/swagger` in a browser.

5. **Try the auth flow in Swagger**
   - `POST /api/auth/register` with a body like:
     ```json
     { "fullName": "Sarah Nkosi", "email": "sarah@raceday.co.za", "password": "Str0ngPassword!", "role": "Organiser" }
     ```
   - `POST /api/auth/login` with the same email/password - Swagger's browser
     session now holds the auth cookie.
   - `GET /api/users/me` - no token to paste in, the cookie is sent automatically.

## Notes

- `Program.cs` exposes `public partial class Program` so the upcoming test
  project can use `WebApplicationFactory<Program>` for integration-style tests.
- Role enforcement for Organiser-only / Participant-only endpoints will be added
  via `[Authorize(Roles = "Organiser")]` etc. as those controllers are built.


## Run the API and MVC together (Windows / VS Code)

The API project is the root `Raceday part 2.csproj` file. The MVC project is `RaceDay.Mvc/RaceDay.Mvc.csproj`. The older `Raceday part 2/` directory is a duplicate/legacy copy and is deliberately excluded from the root API build. Do not run its nested project file.

1. Open the `RACEDAY` folder (the folder containing `Raceday part 2.csproj`) in VS Code.
2. In VS Code, choose **Terminal > Run Task > RaceDay: Start API + MVC**.
3. Keep both PowerShell windows open.
4. API Swagger: `http://localhost:5099/swagger`
5. MVC website: `http://localhost:5080`

The MVC API base URL is configured in `RaceDay.Mvc/appsettings.json` as `http://localhost:5099/`. The API uses the `http` launch profile on port 5099. Ensure SQL Server LocalDB is installed and available; the API uses the `RaceDayDb` LocalDB database and applies EF Core migrations at startup.
