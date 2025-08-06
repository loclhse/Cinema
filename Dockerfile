# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files first
COPY ["WebAPI/WebAPI/WebAPI.csproj", "WebAPI/"]
COPY ["WebAPI/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["WebAPI/Application/Application.csproj", "Application/"]
COPY ["WebAPI/Domain/Domain.csproj", "Domain/"]

# Restore packages for all projects
RUN dotnet restore "WebAPI/WebAPI.csproj"

# Copy all source code
COPY ./WebAPI .

# Build the main project
WORKDIR "/src/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
WORKDIR "/src/WebAPI"
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]
