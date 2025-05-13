# URL Shortener

A full-stack URL shortening application built with React (frontend) and .NET Core (backend). This application allows users to create short, memorable URLs from long ones.

## Features

- Convert long URLs into short, manageable links
- Modern, responsive user interface
- RESTful API backend
- SQLite database for persistent storage

## Prerequisites

- Node.js (v18 or higher)
- .NET 8 SDK
- npm or yarn package manager

## Getting Started

### Backend Setup

1. Navigate to the backend directory:

   ```bash
   cd backend/UrlShortener.API
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Apply database migrations:

   ```bash
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```
   The backend will be available at `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend directory:

   ```bash
   cd frontend
   ```

2. Start the development server:
   ```bash
   npm run dev
   ```
   The frontend will be available at `http://localhost:5173`
