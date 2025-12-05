# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet configuration and local packages
COPY ["nuget.config", "."]
COPY ["Directory.Packages.props", "."]
COPY ["local-packages/", "local-packages/"]

# Copy shared libraries
COPY ["src/Shared/Workshop.Common/Workshop.Common.csproj", "Shared/Workshop.Common/"]
COPY ["src/Shared/Workshop.Proto/Workshop.Proto.csproj", "Shared/Workshop.Proto/"]
COPY ["src/Shared/Workshop.Messaging/Workshop.Messaging.csproj", "Shared/Workshop.Messaging/"]

# Copy MonitoringService projects
COPY ["src/Services/MonitoringService/MonitoringService.Domain/MonitoringService.Domain.csproj", "Services/MonitoringService/MonitoringService.Domain/"]
COPY ["src/Services/MonitoringService/MonitoringService.Application/MonitoringService.Application.csproj", "Services/MonitoringService/MonitoringService.Application/"]
COPY ["src/Services/MonitoringService/MonitoringService.Infrastructure/MonitoringService.Infrastructure.csproj", "Services/MonitoringService/MonitoringService.Infrastructure/"]
COPY ["src/Services/MonitoringService/MonitoringService.API/MonitoringService.API.csproj", "Services/MonitoringService/MonitoringService.API/"]

# Restore dependencies
RUN dotnet restore "Services/MonitoringService/MonitoringService.API/MonitoringService.API.csproj"

# Copy source code
COPY ["src/Shared/", "Shared/"]
COPY ["src/Services/MonitoringService/", "Services/MonitoringService/"]

# Build
WORKDIR "/src/Services/MonitoringService/MonitoringService.API"
RUN dotnet build "MonitoringService.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "MonitoringService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MonitoringService.API.dll"]
