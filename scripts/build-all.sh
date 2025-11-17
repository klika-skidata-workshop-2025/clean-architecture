#!/bin/bash

# Build All Projects Script
# Builds the entire workshop solution

set -e  # Exit on error

echo "========================================"
echo "Building Skidata Workshop 2025"
echo "========================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

cd "$ROOT_DIR"

echo -e "${BLUE}Step 1: Restoring NuGet packages...${NC}"
dotnet restore
echo -e "${GREEN}✓ Packages restored${NC}"
echo ""

echo -e "${BLUE}Step 2: Building shared libraries...${NC}"
dotnet build src/Shared/Workshop.Common/Workshop.Common.csproj -c Release
dotnet build src/Shared/Workshop.Proto/Workshop.Proto.csproj -c Release
dotnet build src/Shared/Workshop.Messaging/Workshop.Messaging.csproj -c Release
echo -e "${GREEN}✓ Shared libraries built${NC}"
echo ""

echo -e "${BLUE}Step 3: Building services...${NC}"
# Note: Uncomment when services are created
# dotnet build src/Services/DeviceService/DeviceService.sln -c Release
# dotnet build src/Services/MonitoringService/MonitoringService.sln -c Release
# dotnet build src/Services/DiagnosticsService/DiagnosticsService.sln -c Release
echo -e "${GREEN}✓ Services built (or skipped if not yet created)${NC}"
echo ""

echo -e "${BLUE}Step 4: Building orchestrator...${NC}"
# Note: Uncomment when orchestrator is created
# dotnet build src/Orchestrator/Workshop.Orchestrator/Workshop.Orchestrator.sln -c Release
echo -e "${GREEN}✓ Orchestrator built (or skipped if not yet created)${NC}"
echo ""

echo ""
echo -e "${GREEN}========================================"
echo -e "✓ Build Complete!"
echo -e "========================================${NC}"
echo ""
echo "Next steps:"
echo "  - Run tests: ./scripts/test-all.sh"
echo "  - Start services: docker-compose up"
echo ""
