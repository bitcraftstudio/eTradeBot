@echo off
echo ============================================
echo  TradeBotEngine - Azure Functions Quick Start
echo ============================================
echo.

:: Check for required tools
where dotnet >nul 2>&1 || (echo ERROR: .NET 8 SDK not found && exit /b 1)
where func >nul 2>&1 || (
    echo Installing Azure Functions Core Tools...
    npm install -g azure-functions-core-tools@4 --unsafe-perm true
)

echo [1/4] Restoring NuGet packages...
cd /d "%~dp0src\TradeBotEngine.Functions"
dotnet restore ..\..\ TradeBotEngine.sln
if ERRORLEVEL 1 (echo ERROR: Package restore failed && exit /b 1)

echo [2/4] Building solution...
dotnet build ..\..\TradeBotEngine.sln -c Debug
if ERRORLEVEL 1 (echo ERROR: Build failed && exit /b 1)

echo [3/4] Running unit tests...
dotnet test ..\..\tests\TradeBotEngine.Tests\TradeBotEngine.Tests.csproj --verbosity minimal
if ERRORLEVEL 1 (echo WARNING: Some tests failed - check output above)

echo [4/4] Starting Azure Functions locally...
echo.
echo  Available endpoints:
echo   POST http://localhost:7071/api/auth/authorize
echo   POST http://localhost:7071/api/auth/complete
echo   GET  http://localhost:7071/api/portfolio/summary
echo   POST http://localhost:7071/api/trading/execute
echo   GET  http://localhost:7071/api/analysis/AAPL
echo   POST http://localhost:7071/api/scheduler/scan
echo.
echo  Make sure local.settings.json has your API keys configured!
echo.
func start --port 7071
