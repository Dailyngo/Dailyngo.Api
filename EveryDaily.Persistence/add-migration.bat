@echo off
set "projectPath=..\EveryDaily.Api"
set /p "migrationName=Enter migration name: "
set "contextName=AppDbContext"

echo Running migrations...
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef --startup-project %projectPath% migrations add %migrationName% --context %contextName%

echo Migrations completed.
pause