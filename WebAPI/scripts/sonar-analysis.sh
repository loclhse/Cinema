#!/bin/bash

# SonarCloud Analysis Script for Linux/Mac
# No Docker required - uses cloud-based SonarQube

SONAR_TOKEN=${1}
SONAR_URL=${2:-"https://sonarcloud.io"}

echo "Starting SonarCloud Analysis..."

# Check if token is provided
if [ -z "$SONAR_TOKEN" ]; then
    echo "Error: SonarCloud token is required!"
    echo "Usage: ./sonar-analysis.sh 'your-token'"
    echo "Get your token from: https://sonarcloud.io/account/security/"
    exit 1
fi

# Check if dotnet-sonarscanner is installed
if ! dotnet tool list --global | grep -q "dotnet-sonarscanner"; then
    echo "Installing dotnet-sonarscanner..."
    dotnet tool install --global dotnet-sonarscanner
fi

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean

# Start SonarCloud analysis
echo "Starting SonarCloud analysis..."
dotnet sonarscanner begin /k:"team03_beee_webapi" /d:sonar.login="$SONAR_TOKEN" /d:sonar.host.url="$SONAR_URL"

# Build the solution
echo "Building solution..."
dotnet build --no-incremental

# Run tests with coverage
echo "Running tests with coverage..."
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# End SonarCloud analysis
echo "Ending SonarCloud analysis..."
dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"

echo "SonarCloud analysis completed!"
echo "Check results at: https://sonarcloud.io/dashboard?id=team03_beee_webapi" 