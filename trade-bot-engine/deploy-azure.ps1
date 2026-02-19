#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Deploy TradeBotEngine to Azure Function App

.DESCRIPTION
  Builds, tests, and deploys the trading engine to Azure Functions.
  Run this from the trade-bot-engine directory.

.PARAMETER ResourceGroup
  Azure resource group name

.PARAMETER FunctionAppName
  Azure Function App name

.PARAMETER SubscriptionId
  Azure subscription ID (optional)
#>

param(
    [string]$ResourceGroup = "rg-tradebot",
    [string]$FunctionAppName = "tradebot-engine",
    [string]$SubscriptionId = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

Write-Host "`n=== TradeBotEngine Azure Deployment ===" -ForegroundColor Cyan

# ── Prerequisites ─────────────────────────────────────────────────────────────
if (-not (Get-Command "az" -ErrorAction SilentlyContinue)) {
    throw "Azure CLI not found. Install from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
}
if (-not (Get-Command "func" -ErrorAction SilentlyContinue)) {
    throw "Azure Functions Core Tools not found. Run: npm i -g azure-functions-core-tools@4"
}

if ($SubscriptionId) {
    Write-Host "Setting subscription: $SubscriptionId"
    az account set --subscription $SubscriptionId
}

# ── Build & Test ──────────────────────────────────────────────────────────────
Write-Host "`n[1/4] Building solution..." -ForegroundColor Yellow
dotnet build "$scriptDir\TradeBotEngine.sln" -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

Write-Host "`n[2/4] Running tests..." -ForegroundColor Yellow
dotnet test "$scriptDir\tests\TradeBotEngine.Tests\TradeBotEngine.Tests.csproj" --verbosity minimal
if ($LASTEXITCODE -ne 0) { Write-Warning "Tests failed - review before deploying to production!" }

# ── Deploy ────────────────────────────────────────────────────────────────────
Write-Host "`n[3/4] Publishing Function App..." -ForegroundColor Yellow
Set-Location "$scriptDir\src\TradeBotEngine.Functions"
func azure functionapp publish $FunctionAppName --dotnet-isolated
if ($LASTEXITCODE -ne 0) { throw "Deployment failed" }

Write-Host "`n[4/4] Verifying deployment..." -ForegroundColor Yellow
$status = az functionapp show --name $FunctionAppName --resource-group $ResourceGroup --query "state" -o tsv
Write-Host "Function App status: $status"

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
Write-Host "Endpoint: https://$FunctionAppName.azurewebsites.net/api/"
Write-Host "`nRemember to configure Application Settings in Azure Portal:"
Write-Host "  ETrade:ConsumerKey, ETrade:ConsumerSecret"
Write-Host "  AI:Provider, AI:ApiKey, AI:Model"
Write-Host "  Cosmos:ConnectionString, Cosmos:DatabaseName"
Write-Host "  Trading:DefaultRiskProfile, Trading:AutoTradeEnabled"
