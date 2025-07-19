# SonarCloud Analysis Script for Windows
# No Docker required - uses cloud-based SonarQube

param(
    [string]$SonarToken,
    [string]$SonarUrl = "https://sonarcloud.io"
)

Write-Host "Starting SonarCloud Analysis..." -ForegroundColor Green

# Check if token is provided
if ([string]::IsNullOrEmpty($SonarToken)) {
    Write-Host "Error: SonarCloud token is required!" -ForegroundColor Red
    Write-Host "Usage: .\sonar-analysis.ps1 -SonarToken 'your-token'" -ForegroundColor Yellow
    Write-Host "Get your token from: https://sonarcloud.io/account/security/" -ForegroundColor Cyan
    exit 1
}

# Check if dotnet-sonarscanner is installed
try {
    dotnet tool list --global | Select-String "dotnet-sonarscanner"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Installing dotnet-sonarscanner..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-sonarscanner
    }
} catch {
    Write-Host "Installing dotnet-sonarscanner..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-sonarscanner
}

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean

# Start SonarCloud analysis
Write-Host "Starting SonarCloud analysis..." -ForegroundColor Yellow
dotnet sonarscanner begin /k:"team03_beee_webapi" /d:sonar.login="$SonarToken" /d:sonar.host.url="$SonarUrl"

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --no-incremental

# Run tests with coverage
Write-Host "Running tests with coverage..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# End SonarCloud analysis
Write-Host "Ending SonarCloud analysis..." -ForegroundColor Yellow
dotnet sonarscanner end /d:sonar.login="$SonarToken"

Write-Host "SonarCloud analysis completed!" -ForegroundColor Green
Write-Host "Check results at: https://sonarcloud.io/dashboard?id=team03_beee_webapi" -ForegroundColor Cyan 