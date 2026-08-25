# syntax=docker/dockerfile:1

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_EnableDiagnostics=0

COPY src/RESR.Models/RESR.Models.csproj ./src/RESR.Models/
COPY src/RESR.Core/RESR.Core.csproj ./src/RESR.Core/
COPY src/RESR.Infrastructure/RESR.Infrastructure.csproj ./src/RESR.Infrastructure/
COPY src/RESR.WebAPI/RESR.WebAPI.csproj ./src/RESR.WebAPI/

RUN dotnet restore ./src/RESR.WebAPI/RESR.WebAPI.csproj /m:1 /p:RestoreDisableParallel=true

COPY . .
RUN dotnet publish ./src/RESR.WebAPI/RESR.WebAPI.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /m:1 \
    /p:BuildInParallel=false \
    /p:UseSharedCompilation=false

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RESR.WebAPI.dll"]
