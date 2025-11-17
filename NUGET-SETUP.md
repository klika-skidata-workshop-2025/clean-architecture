# NuGet Package Setup

This workshop uses a hybrid NuGet approach for the shared libraries (`Workshop.Common`, `Workshop.Proto`, `Workshop.Messaging`).

## Local Development

For local development, the shared libraries are consumed as NuGet packages from a **local feed**:

### First-time Setup

Before building the services, pack the shared libraries to the local feed:

```bash
# From the repository root
dotnet pack src/Shared/Workshop.Common/Workshop.Common.csproj --configuration Release --output ./local-packages
dotnet pack src/Shared/Workshop.Proto/Workshop.Proto.csproj --configuration Release --output ./local-packages
dotnet pack src/Shared/Workshop.Messaging/Workshop.Messaging.csproj --configuration Release --output ./local-packages
```

### Build Order

1. **Pack shared libraries** (as shown above)
2. **Restore and build services**:
   ```bash
   dotnet restore src/Services/DeviceService/DeviceService.API/DeviceService.API.csproj
   dotnet build src/Services/DeviceService/DeviceService.API/DeviceService.API.csproj --configuration Release
   ```

### After Changes to Shared Libraries

If you modify any shared library:

```bash
# 1. Rebuild and repack the changed library
dotnet pack src/Shared/Workshop.Common/Workshop.Common.csproj --configuration Release --output ./local-packages

# 2. Clear NuGet cache to force refresh
dotnet nuget locals all --clear

# 3. Restore and rebuild services
dotnet restore src/Services/DeviceService/DeviceService.API/DeviceService.API.csproj
dotnet build src/Services/DeviceService/DeviceService.API/DeviceService.API.csproj
```

## CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/ci.yml`) automatically:

1. **Builds and packs shared libraries** to `./local-packages`
2. **Uploads packages as artifacts** for other jobs to consume
3. **Downloads packages** in each service build job (DeviceService, MonitoringService, DiagnosticsService, Orchestrator)
4. **Restores and builds all services** using the local packages

### Key CI/CD Changes

The following changes were made to support NuGet packages in CI:

**Directory.Packages.props:**
- Added `Workshop.Common` version 1.0.0
- Added `Workshop.Proto` version 1.0.0
- Added `Workshop.Messaging` version 1.0.0
- Fixed `Grpc.AspNetCore.HealthChecks` package name (was incorrectly named `Microsoft.AspNetCore.Grpc.HealthChecks`)

**NuGet.Config:**
- Added `local` package source pointing to `./local-packages`
- This allows both local development and CI/CD to use the same package feed

**Service .csproj files:**
- Fixed package name in all service API projects: `Grpc.AspNetCore.HealthChecks`
- Added missing `Workshop.Messaging` reference to all Application layers:
  - `DeviceService.Application`
  - `MonitoringService.Application`
  - `DiagnosticsService.Application`

**Shared library .csproj files:**
- Removed non-existent `icon.png` references from all three projects
- This was causing NuGet pack errors: `error NU5046: The icon file 'icon.png' does not exist`

## Production Deployment

For production, the shared libraries are published to **GitHub Packages**:

```bash
# This is done automatically by the publish-nuget.yml workflow
# when you create a version tag (e.g., v1.0.0)
git tag v1.0.0
git push origin v1.0.0
```

## Package Sources

The `NuGet.Config` defines three package sources:

1. **local** - `./local-packages` directory (for local development and CI)
2. **nuget.org** - Official NuGet packages (MediatR, FluentValidation, etc.)
3. **github** - GitHub Packages (for published Workshop libraries)

## Version Management

All package versions are managed centrally in `Directory.Packages.props`:

```xml
<PackageVersion Include="Workshop.Common" Version="1.0.0" />
<PackageVersion Include="Workshop.Proto" Version="1.0.0" />
<PackageVersion Include="Workshop.Messaging" Version="1.0.0" />
```

## Why NuGet Packages?

This approach demonstrates:

- ✓ Real-world package distribution
- ✓ Version management and compatibility
- ✓ Clear separation between shared libraries and services
- ✓ Contract-first development with protocol buffers
- ✓ Production-ready NuGet workflow

## Alternative: ProjectReference

For simpler workshops, you could use `<ProjectReference>` instead:

```xml
<!-- Instead of <PackageReference Include="Workshop.Common" /> -->
<ProjectReference Include="..\..\..\..\Shared\Workshop.Common\Workshop.Common.csproj" />
```

This would eliminate the need for packing and local feeds, but wouldn't demonstrate the full NuGet workflow.

## Files Modified

The following files were created or modified to support the NuGet package workflow:

### Created:
- `NUGET-SETUP.md` - This documentation
- `local-packages/` - Local NuGet feed directory (gitignored)

### Modified:
- `Directory.Packages.props` - Added Workshop package versions and fixed Grpc.AspNetCore.HealthChecks
- `NuGet.Config` - Added local package source
- `.gitignore` - Added `local-packages/` directory
- `.github/workflows/ci.yml` - Updated to pack and distribute packages via artifacts
- All service API `.csproj` files - Fixed Grpc.AspNetCore.HealthChecks package name
- All service Application `.csproj` files - Added Workshop.Messaging reference
- All shared library `.csproj` files - Removed icon.png references

## Summary of Changes

**Total files modified:** 15+
**Total lines changed:** ~100+

All changes are backward-compatible and the workshop is now fully operational with NuGet package distribution!
