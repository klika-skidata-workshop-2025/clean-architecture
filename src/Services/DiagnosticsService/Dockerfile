# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet configuration and local packages
COPY ["nuget.config", "."]
COPY ["Directory.Packages.props", "."]
COPY ["local-packages/", "local-packages/"]

COPY ["src/Shared/Workshop.Common/Workshop.Common.csproj", "Shared/Workshop.Common/"]
COPY ["src/Shared/Workshop.Proto/Workshop.Proto.csproj", "Shared/Workshop.Proto/"]
COPY ["src/Shared/Workshop.Messaging/Workshop.Messaging.csproj", "Shared/Workshop.Messaging/"]

COPY ["src/Services/DiagnosticsService/DiagnosticsService.Domain/DiagnosticsService.Domain.csproj", "Services/DiagnosticsService/DiagnosticsService.Domain/"]
COPY ["src/Services/DiagnosticsService/DiagnosticsService.Application/DiagnosticsService.Application.csproj", "Services/DiagnosticsService/DiagnosticsService.Application/"]
COPY ["src/Services/DiagnosticsService/DiagnosticsService.Infrastructure/DiagnosticsService.Infrastructure.csproj", "Services/DiagnosticsService/DiagnosticsService.Infrastructure/"]
COPY ["src/Services/DiagnosticsService/DiagnosticsService.API/DiagnosticsService.API.csproj", "Services/DiagnosticsService/DiagnosticsService.API/"]

RUN dotnet restore "Services/DiagnosticsService/DiagnosticsService.API/DiagnosticsService.API.csproj"

COPY ["src/Shared/", "Shared/"]
COPY ["src/Services/DiagnosticsService/", "Services/DiagnosticsService/"]

WORKDIR "/src/Services/DiagnosticsService/DiagnosticsService.API"
RUN dotnet build "DiagnosticsService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiagnosticsService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DiagnosticsService.API.dll"]
