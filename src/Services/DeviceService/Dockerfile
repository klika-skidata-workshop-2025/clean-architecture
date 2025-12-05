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

# Copy DeviceService projects
COPY ["src/Services/DeviceService/DeviceService.Domain/DeviceService.Domain.csproj", "Services/DeviceService/DeviceService.Domain/"]
COPY ["src/Services/DeviceService/DeviceService.Application/DeviceService.Application.csproj", "Services/DeviceService/DeviceService.Application/"]
COPY ["src/Services/DeviceService/DeviceService.Infrastructure/DeviceService.Infrastructure.csproj", "Services/DeviceService/DeviceService.Infrastructure/"]
COPY ["src/Services/DeviceService/DeviceService.API/DeviceService.API.csproj", "Services/DeviceService/DeviceService.API/"]

# Restore dependencies
RUN dotnet restore "Services/DeviceService/DeviceService.API/DeviceService.API.csproj"

# Copy source code
COPY ["src/Shared/", "Shared/"]
COPY ["src/Services/DeviceService/", "Services/DeviceService/"]

# Build
WORKDIR "/src/Services/DeviceService/DeviceService.API"
RUN dotnet build "DeviceService.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "DeviceService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DeviceService.API.dll"]
