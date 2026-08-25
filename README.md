# RE-CESI

## Deployment

The Azure infrastructure, Terraform plan/apply process, GitHub Actions configuration, and production runbook are documented in [docs/deployment.md](docs/deployment.md).

## Web frontend

A new React + Chakra UI frontend is available in `src/RESR.Frontend`.

Development setup:

1. Run the API with `dotnet run --project src/RESR.WebAPI`.
2. In `src/RESR.Frontend`, copy `.env.example` to `.env` if you need a custom API URL.
3. Install frontend dependencies with `npm install`.
4. Start the frontend with `npm run dev`.

The frontend targets `http://localhost:5270` by default and the API is configured to allow the Vite dev server origin in development.

Current scope:

- Login only
- Session restore from local storage
- Minimal authenticated home screen

Frontend structure:

- `src/app`: app bootstrap, providers, theme, layouts
- `src/features/auth`: auth feature code, hooks, provider, service, page, components
- `src/shared`: shared HTTP client, config, storage, error helpers, UI primitives, shared types
- `src/pages`: top-level screens after authentication
  .
