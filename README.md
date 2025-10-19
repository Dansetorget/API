# QuizApp

A quiz application built with ASP.NET Core 8.0 MVC. This is a preliminary project that will be developed further.

## What it does

Create and take quizzes with multiple questions and answers. You can add, edit and delete quizzes through a simple web interface.

## How to run it

You need .NET 8.0 SDK installed on your computer.

1. Open terminal and navigate to the project folder
2. Run: `dotnet run`
3. Open your browser and go to http://localhost:5243

The database will be created automatically when you first run the application.

## Project structure

The application uses MVC pattern with separate layers for models, controllers and views. Data is stored in SQLite database using Entity Framework Core. The code follows repository pattern for clean data access.

## Technology

- .NET 8.0
- ASP.NET Core MVC
- Entity Framework Core
- SQLite database
- Bootstrap for styling

