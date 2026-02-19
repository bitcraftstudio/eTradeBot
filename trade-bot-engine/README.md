# TradeBotEngine - C# Azure Functions Port

AI-driven stock trading engine ported from NestJS to **C# Azure Functions v4** (.NET 8).

## Architecture Overview

```
TradeBotEngine/
├── src/
│   ├── TradeBotEngine.Core/           # Domain models & interfaces
│   │   ├── Models/                    # Trade, Position, RiskProfile, ETrade, etc.
│   │   └── Interfaces/                # IServices.cs, IPositionMonitorService.cs
│   │
│   ├── TradeBotEngine.Infrastructure/ # All service implementations
│   │   ├── Services/
│   │   │   ├── ETradeService.cs       # eTrade OAuth 1.0a + order execution
│   │   │   ├── AIAnalysisService.cs   # Claude / OpenAI / xAI analysis
│   │   │   ├── MarketDataService.cs   # Yahoo Finance OHLCV + indicators
│   │   │   ├── RiskManagementService.cs # Position sizing & risk controls
│   │   │   ├── TradingService.cs      # Buy/sell orchestration
│   │   │   ├── PortfolioService.cs    # Portfolio state & eTrade sync
│   │   │   ├── PositionMonitorService.cs # Stop loss / take profit / trailing stop
│   │   │   ├── LearningService.cs     # AI-powered trade review & patterns
│   │   │   ├── StockHunterService.cs  # Stock discovery & scoring
│   │   │   └── NewsService.cs        # Alpha Vantage news + mock fallback
│   │   └── Repositories/
│   │       └── CosmosRepositories.cs  # Cosmos DB: trades, positions, learning, stocks
│   │
│   └── TradeBotEngine.Functions/      # Azure Function entry points
│       ├── Program.cs                 # DI setup & host configuration
│       ├── host.json                  # Function app configuration
│       ├── local.settings.json        # Local dev configuration (NOT committed)
│       └── Functions/
│           ├── TradingFunctions.cs    # HTTP: execute, portfolio, positions
│           ├── SchedulerFunctions.cs  # Timer: morning scan, monitor, learning
│           ├── AIAnalysisFunctions.cs # HTTP: analyze, quote, indicators, risk
│           ├── AuthFunctions.cs       # HTTP: eTrade OAuth flow & accounts
│           └── StockHunterFunctions.cs# HTTP: hunt, discovered, learning insights
│
└── tests/
    └── TradeBotEngine.Tests/          # xUnit tests
```

## Key Differences from NestJS (`trade-bot-engine_old`)

| Feature | NestJS | C# Azure Functions |
|---------|--------|--------------------|
| Scheduling | `@Cron()` decorators | Azure Timer Triggers |
| HTTP API | NestJS Controllers | Azure HTTP Triggers |
| Database | MongoDB | Azure Cosmos DB |
| DI | NestJS modules | `Microsoft.Extensions.DI` |
| Config | `.env` | `local.settings.json` / App Settings |
| Deployment | PM2 / Docker | Azure Function App |

## API Reference

### Authentication (eTrade OAuth 1.0a)
```
GET  /api/auth/authorize          → Get authorization URL (step 1)
POST /api/auth/complete           → { "verifier": "..." } (step 2)
POST /api/auth/refresh            → Refresh access token
GET  /api/auth/status             → Check auth state
GET  /api/accounts                → List eTrade accounts
GET  /api/accounts/{id}/balance   → Account balance
GET  /api/accounts/{id}/orders    → Order history
```

### Trading
```
POST /api/trading/execute         → Execute trade: { symbol, type, quantity? }
GET  /api/trading/trades          → All trades
GET  /api/trading/trades/open     → Open trades
GET  /api/trading/trades/closed   → Closed trades
GET  /api/trading/trades/{id}     → Specific trade
```

### Portfolio
```
GET  /api/portfolio/summary       → Portfolio value, P&L, positions
GET  /api/portfolio/positions     → Open positions
POST /api/portfolio/sync          → Sync with live eTrade account
```

### AI Analysis
```
GET  /api/analysis/{symbol}             → Full AI analysis + recommendation
GET  /api/analysis/{symbol}/quote       → Real-time quote
GET  /api/analysis/{symbol}/indicators  → RSI, MACD, Bollinger, SMA, EMA
GET  /api/analysis/{symbol}/risk        → Position size calculator
GET  /api/analysis/multi?symbols=X,Y,Z  → Analyze multiple symbols
```

### Stock Discovery
```
POST /api/stocks/hunt             → Hunt for top stocks (optional filters body)
GET  /api/stocks/discovered       → All discovered stocks
POST /api/stocks/{symbol}/watchlist → Add to watchlist
GET  /api/stocks/config           → Hunter configuration
PUT  /api/stocks/config           → Update hunter configuration
```

### Learning & Performance
```
GET  /api/learning/insights        → All AI learning insights
GET  /api/learning/insights/latest → Latest insight
GET  /api/learning/performance     → Win rate, avg return, best/worst trades
GET  /api/learning/strategy        → Current strategy weights
```

### Scheduler (Manual Triggers)
```
POST /api/scheduler/scan    → Run market scan now
POST /api/scheduler/monitor → Run position check now
POST /api/scheduler/learn   → Run learning review now
```

## Risk Profiles

| Profile | Max Risk/Trade | Max Position | Stop Loss | Min R:R | Max Positions |
|---------|---------------|--------------|-----------|---------|---------------|
| Conservative | 1% | 10% | 2% | 3:1 | 3 |
| Moderate | 2% | 15% | 3% | 2:1 | 5 |
| Aggressive | 5% | 25% | 5% | 1.5:1 | 7 |
| VeryAggressive | 10% | 40% | 8% | 1:1 | 10 |

## Automated Scheduling (UTC Times)

| Function | Cron (UTC) | ET Time | Purpose |
|----------|-----------|---------|---------|
| MorningScan | `0 30 14 * * 1-5` | 9:30 AM | Watchlist scan + auto-trade |
| PositionMonitor | `0 */30 15-21 * * 1-5` | 10AM-4PM | Stop/target checks |
| DailyPositionSnapshot | `0 30 21 * * 1-5` | 4:30 PM | EOD P&L snapshots |
| DailyLearningReview | `0 30 22 * * 1-5` | 5:30 PM | AI pattern extraction |
| WeeklyStockHunt | `0 0 13 * * 1` | 8:00 AM Mon | New stock discovery |

## Local Development

### Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools v4: `npm i -g azure-functions-core-tools@4`
- Azure Cosmos DB Emulator (or a real Cosmos DB instance)

### Setup
```bash
cd src/TradeBotEngine.Functions

# Copy and configure settings
# Edit local.settings.json with your API keys

# Start Cosmos DB Emulator, then:
func start
```

### Environment Variables (local.settings.json)

```json
{
  "ETrade:ConsumerKey": "...",        ← From eTrade Developer account
  "ETrade:ConsumerSecret": "...",
  "ETrade:UseSandbox": "true",        ← Set false for live trading

  "AI:Provider": "anthropic",         ← anthropic | openai | xai
  "AI:ApiKey": "sk-ant-...",
  "AI:Model": "claude-opus-4-5-20251101",

  "Cosmos:ConnectionString": "...",
  "Cosmos:DatabaseName": "TradeBotDB",

  "Trading:DefaultRiskProfile": "Moderate",
  "Trading:AutoTradeEnabled": "false", ← ALWAYS start false!
  "Trading:InitialCapital": "10000",

  "News:AlphaVantageApiKey": "..."    ← Optional, uses mock news if absent
}
```

## eTrade OAuth Flow

eTrade uses OAuth 1.0a (not 1.0a Bearer). Tokens expire at midnight ET daily.

1. `GET /api/auth/authorize` → Returns authorization URL
2. Open URL in browser → Log in to eTrade → Authorize → Copy verifier code
3. `POST /api/auth/complete { "verifier": "XXXXX" }` → Stores access token
4. All subsequent API calls use stored token automatically
5. Token renewal: `POST /api/auth/refresh` (or happens automatically)

**Sandbox vs Live:** Set `ETrade:UseSandbox: "true"` for testing with fake money.
When ready for live trading, set `"false"` and be very careful with `AutoTradeEnabled`.

## Deployment to Azure

```bash
# Create resources
az group create -n rg-tradebot -l eastus
az functionapp create -g rg-tradebot -n tradebot-engine \
  --runtime dotnet-isolated --runtime-version 8 \
  --storage-account tradebotstorage --consumption-plan-location eastus

# Set app settings
az functionapp config appsettings set -g rg-tradebot -n tradebot-engine \
  --settings "ETrade:ConsumerKey=..." "AI:ApiKey=..." "Cosmos:ConnectionString=..."

# Deploy
dotnet publish src/TradeBotEngine.Functions -c Release
func azure functionapp publish tradebot-engine
```

## AI Provider Support

The `AIAnalysisService` supports three providers, configurable via `AI:Provider`:

| Provider | Config Value | Recommended Model |
|----------|-------------|-------------------|
| Anthropic (Claude) | `anthropic` | `claude-opus-4-5-20251101` |
| OpenAI | `openai` | `gpt-4o` |
| xAI (Grok) | `xai` | `grok-beta` |

All providers use the same prompt structure and return standardized JSON responses.
