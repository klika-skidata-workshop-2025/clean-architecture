# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/Workshop.Common/Workshop.Common.csproj", "Shared/Workshop.Common/"]
COPY ["src/Shared/Workshop.Proto/Workshop.Proto.csproj", "Shared/Workshop.Proto/"]

COPY ["src/Orchestrator/Workshop.Orchestrator/Workshop.Orchestrator.csproj", "Orchestrator/Workshop.Orchestrator/"]

RUN dotnet restore "Orchestrator/Workshop.Orchestrator/Workshop.Orchestrator.csproj"

COPY ["src/Shared/", "Shared/"]
COPY ["src/Orchestrator/Workshop.Orchestrator/", "Orchestrator/Workshop.Orchestrator/"]

WORKDIR "/src/Orchestrator/Workshop.Orchestrator"
RUN dotnet build "Workshop.Orchestrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Workshop.Orchestrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Workshop.Orchestrator.dll"]
