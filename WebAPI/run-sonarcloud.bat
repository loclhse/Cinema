@echo off

:: Set your project metadata
set SONAR_TOKEN=3e6f80b892f80d70a5699197e07fc2abf19d58ce
set ORG=loclhse
set PROJECT_KEY=loclhse_Cinema

:: Run SonarCloud begin step
dotnet-sonarscanner begin ^
  /o:"%ORG%" ^
  /k:"%PROJECT_KEY%" ^
  /d:sonar.host.url="https://sonarcloud.io" ^
  /d:sonar.login="%SONAR_TOKEN%" ^
  /d:sonar.cs.opencover.reportsPaths="WebAPI/TestResults/coverage.opencover.xml"

:: Build and test with coverage
dotnet build
dotnet test ^
  /p:CollectCoverage=true ^
  /p:CoverletOutputFormat=opencover ^
  /p:CoverletOutput=WebAPI/TestResults/coverage.opencover.xml

:: End SonarCloud scan
dotnet-sonarscanner end /d:sonar.login="%SONAR_TOKEN%"
