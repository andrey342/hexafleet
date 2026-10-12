FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first so the package layer is cached until a project file changes.
COPY .editorconfig Directory.Build.props Directory.Build.targets NuGet.config global.json ./
COPY src/GtMotive.Estimate.Microservice.Domain/GtMotive.Estimate.Microservice.Domain.csproj src/GtMotive.Estimate.Microservice.Domain/
COPY src/GtMotive.Estimate.Microservice.ApplicationCore/GtMotive.Estimate.Microservice.ApplicationCore.csproj src/GtMotive.Estimate.Microservice.ApplicationCore/
COPY src/GtMotive.Estimate.Microservice.Infrastructure/GtMotive.Estimate.Microservice.Infrastructure.csproj src/GtMotive.Estimate.Microservice.Infrastructure/
COPY src/GtMotive.Estimate.Microservice.Api/GtMotive.Estimate.Microservice.Api.csproj src/GtMotive.Estimate.Microservice.Api/
COPY src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj src/GtMotive.Estimate.Microservice.Host/
RUN dotnet restore src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj

COPY src/ src/
RUN dotnet publish src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj \
    -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "GtMotive.Estimate.Microservice.Host.dll"]
