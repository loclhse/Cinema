@echo off
setlocal enabledelayedexpansion

:: === Project Metadata ===
set SONAR_TOKEN=3e6f80b892f80d70a5699197e07fc2abf19d58ce
set ORG=loclhse
set PROJECT_KEY=loclhse_Cinema

:: === Paths ===
set COVERAGE_DIR=ZTest\TestResults
set COVERLET_SETTINGS=coverlet.runsettings

:: === Cleanup ===
echo 🔄 Cleaning old coverage files...
for /R "%COVERAGE_DIR%" %%F in (coverage.cobertura.xml) do del "%%F"

:: === Build ===
echo 🛠️  Building the project...
dotnet build

:: === Run tests and collect coverage using runsettings ===
echo 🧪 Running tests and collecting filtered Cobertura coverage...
dotnet test ZTest\ZTest.csproj --settings %COVERLET_SETTINGS%

:: === Locate latest cobertura file ===
echo 🔍 Locating Cobertura file...
set "LATEST_COBERTURA="

for /R "%COVERAGE_DIR%" %%F in (coverage.cobertura.xml) do (
    set "LATEST_COBERTURA=%%F"
)

if not defined LATEST_COBERTURA (
    echo ❌ ERROR: No coverage.cobertura.xml found!
    pause
    exit /b 1
)

echo ✅ Found: !LATEST_COBERTURA!

:: === Start SonarCloud scan with correct path ===
echo 🚀 Starting SonarCloud scan...
dotnet-sonarscanner begin ^
  /o:"%ORG%" ^
  /k:"%PROJECT_KEY%" ^
  /d:sonar.host.url="https://sonarcloud.io" ^
  /d:sonar.login="%SONAR_TOKEN%" ^
  /d:sonar.cs.coveragePlugin=cobertura ^
  /d:sonar.coverageReportPaths="!LATEST_COBERTURA!"

:: === Rebuild to ensure proper coverage sync ===
dotnet build

:: === Finalize scan ===
echo 📡 Uploading coverage to SonarCloud...
dotnet-sonarscanner end /d:sonar.login="%SONAR_TOKEN%"

echo ✅ DONE!
pause
