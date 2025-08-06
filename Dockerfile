FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WebAPI/WebAPI/WebAPI.csproj", "WebAPI/WebAPI/"]
COPY ["WebAPI/Infrastructure/Infrastructure.csproj", "WebAPI/Infrastructure/"]
COPY ["WebAPI/Application/Application.csproj", "WebAPI/Application/"]
COPY ["WebAPI/Domain/Domain.csproj", "WebAPI/Domain/"]

# Restore as distinct layers with runtime identifier
RUN dotnet restore "WebAPI/WebAPI/WebAPI.csproj" -r linux-x64

# Copy and build source
COPY . .
WORKDIR "/src/WebAPI/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
# Publish with framework-dependent deployment (lighter image)
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish \
    --no-restore \
    --self-contained false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure for Azure Container Apps environment
ENV ASPNETCORE_URLS=http://+:8081
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Set proper user for container security
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates tzdata \
    && rm -rf /var/lib/apt/lists/* \
    && adduser --disabled-password --gecos "" --home /app --no-create-home --uid 1000 appuser \
    && chown -R appuser:appuser /app
    
USER appuser

ENTRYPOINT ["dotnet", "WebAPI.dll"]
