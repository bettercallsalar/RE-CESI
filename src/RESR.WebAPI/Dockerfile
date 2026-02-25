# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/RESR.sln ./src/RESR.sln
COPY src/RESR.Models/RESR.Models.csproj ./src/RESR.Models/
COPY src/RESR.Core/RESR.Core.csproj ./src/RESR.Core/
COPY src/RESR.Infrastructure/RESR.Infrastructure.csproj ./src/RESR.Infrastructure/
COPY src/RESR.WebAPI/RESR.WebAPI.csproj ./src/RESR.WebAPI/
COPY tests/RESR.Core.Tests/RESR.Core.Tests.csproj ./tests/RESR.Core.Tests/

RUN dotnet restore ./src/RESR.sln

COPY . .
RUN dotnet publish ./src/RESR.WebAPI/RESR.WebAPI.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RESR.WebAPI.dll"]