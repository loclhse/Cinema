# SonarCloud Integration Guide

This project has been configured with SonarCloud for static code analysis and quality monitoring.

## Prerequisites

- .NET 8.0 SDK
- PowerShell (for Windows) or Bash (for Linux/Mac)
- SonarCloud account (free tier available)

## Quick Start

### 1. Create SonarCloud Account

1. Go to [SonarCloud](https://sonarcloud.io)
2. Sign up with your GitHub account
3. Create a new organization (or use existing)
4. Create a new project:
   - Project key: `team03_beee_webapi`
   - Project name: `Team03 BEEE WebAPI`

### 2. Get Your Token

1. Go to [SonarCloud Account Security](https://sonarcloud.io/account/security/)
2. Generate a new token
3. Save the token securely

### 3. Update Configuration

Edit `sonar-project.properties` and replace `your-organization-key` with your actual organization key from SonarCloud.

### 4. Run Analysis

#### Windows (PowerShell)
```powershell
# Run the analysis script
.\scripts\sonar-analysis.ps1 -SonarToken "your-sonarcloud-token"
```

#### Linux/Mac (Bash)
```bash
# Make script executable (first time only)
chmod +x scripts/sonar-analysis.sh

# Run the analysis script
./scripts/sonar-analysis.sh "your-sonarcloud-token"
```

#### Manual Analysis
```bash
# Install SonarQube scanner
dotnet tool install --global dotnet-sonarscanner

# Start analysis
dotnet sonarscanner begin /k:"team03_beee_webapi" /d:sonar.login="your-token" /d:sonar.host.url="https://sonarcloud.io"

# Build and test
dotnet build --no-incremental
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# End analysis
dotnet sonarscanner end /d:sonar.login="your-token"
```

## Configuration Files

### sonar-project.properties
Main configuration file for SonarCloud analysis settings.

### GitHub Actions (.github/workflows/sonarqube.yml)
Automated analysis on push and pull requests.

## Features

- **Static Code Analysis**: Detects bugs, vulnerabilities, and code smells
- **Code Coverage**: Tracks test coverage
- **Security Hotspots**: Identifies security issues
- **Technical Debt**: Measures code maintainability
- **Duplications**: Finds code duplications
- **Complexity**: Analyzes code complexity

## Quality Gates

SonarCloud uses quality gates to ensure code quality:

- **Coverage**: Minimum 80% test coverage
- **Duplications**: Maximum 3% duplicated lines
- **Maintainability**: A rating of A
- **Reliability**: A rating of A
- **Security**: A rating of A
- **Security Hotspots**: A rating of A

## Troubleshooting

### Common Issues

1. **Token not working**
   - Ensure token is correct and not expired
   - Check if you have access to the project
   - Verify organization key is correct

2. **Analysis fails**
   - Check your token is correct
   - Verify .NET SDK version matches project
   - Ensure all dependencies are installed

3. **Coverage not showing**
   - Make sure tests are running successfully
   - Check coverage collector is installed
   - Verify test project is included in analysis

### Useful Commands

```bash
# Check if dotnet-sonarscanner is installed
dotnet tool list --global

# Install dotnet-sonarscanner
dotnet tool install --global dotnet-sonarscanner

# Update dotnet-sonarscanner
dotnet tool update --global dotnet-sonarscanner
```

## Integration with CI/CD

The project includes GitHub Actions workflow for automated analysis. To use it:

1. Add `SONAR_TOKEN` secret to your GitHub repository
2. Update the organization name in `.github/workflows/sonarqube.yml`
3. Push to main or develop branch to trigger analysis

## Best Practices

1. **Regular Analysis**: Run analysis before each commit
2. **Quality Gates**: Don't merge code that fails quality gates
3. **Coverage**: Maintain high test coverage
4. **Security**: Address security hotspots promptly
5. **Technical Debt**: Keep technical debt low

## Resources

- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [.NET Scanner Documentation](https://docs.sonarcloud.io/analysis/languages/csharp/)
- [Quality Gates](https://docs.sonarcloud.io/user-guide/quality-gates/)
- [Get Started with SonarCloud](https://sonarcloud.io/documentation/getting-started/) 