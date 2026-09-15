FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["sims-stix-ingest/sims-stix-ingest.csproj", "sims-stix-ingest/"]
RUN dotnet restore "sims-stix-ingest/sims-stix-ingest.csproj"
COPY . .
WORKDIR "/src/sims-stix-ingest"
RUN dotnet build "./sims-stix-ingest.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./sims-stix-ingest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sims-stix-ingest.dll"]
