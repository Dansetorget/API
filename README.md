# Quiz API

Backend API for a quiz application built with ASP.NET Core 8.0. The service exposes endpoints for registering users, authenticating with JWT, creating and managing quizzes, and submitting quiz answers. Entity Framework Core is used for data access while Mapster handles DTO ↔ entity mapping.

## Tech Stack

- ASP.NET Core 8
- Entity Framework Core with SQLite
- ASP.NET Identity (separate Auth DB)
- JWT authentication & authorization
- Mapster for object mapping

## How to run

1. **Configure environment**
   - Update connection strings and JWT settings in `QuizApi/appsettings.json` or `appsettings.Development.json`.
2. **Run database migrations**
   ```bash
   dotnet ef database update --project QuizApi/QuizApi.csproj
   ```
4. **Start the API**
   ```bash
   dotnet run --project QuizApi/QuizApi.csproj
   ```
   Swagger UI is available in development at `https://localhost:7054/swagger`.

## Seeding Data

When running in the Development environment, `DBInit.Seed` resets the quiz database and:

- Creates a default `Admin` (`dansetorget@gmail.com`, password `Dansetorget123!`).
- Inserts sample quizzes owned by that user.

## Key Endpoints

- `POST /api/Auth/register` – Register a new user.
- `POST /api/Auth/login` – Obtain JWT token using email/password.
- `GET /api/Quiz` – List quizzes.
- `GET /api/Quiz/{id}` – Retrieve a quiz with questions and answers.
- `POST /api/Quiz` _(auth required)_ – Create a quiz (owner set from JWT).
- `PATCH /api/Quiz/{id}` _(auth required)_ – Update a quiz.
- `DELETE /api/Quiz/{id}` _(auth required)_ – Delete a quiz.
- `POST /api/Quiz/submit` – Submit quiz answers and receive a scored result.

Authorization headers should use the `Bearer <token>` convention.

## Mapping & DTOs

`QuizApi/Config/MapsterConfig.cs` contains the Mapster configuration for translating between domain models (`Quiz`, `Question`, `Answer`) and DTOs (`QuizDTO`, `QuizResultDTO`, etc.). The configuration is activated in `Program.cs`.

## Testing

Unit tests reside in `QuizApi.Tests`. Run them with:

```bash
dotnet test
```

## Logging

Application logs written while running locally are stored in `QuizApi/APILogs`. Review those files for troubleshooting.
