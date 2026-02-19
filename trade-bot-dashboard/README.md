# AI Trading Bot Dashboard - Nuxt 4 + UI v3

Beautiful, modern trading dashboard built with **Nuxt 4** and **Nuxt UI v3**! 🚀

## ✨ Tech Stack

- **Nuxt 4** (RC) - Latest Vue.js framework with improved performance
- **Nuxt UI v3** (Alpha) - Modern component library with Tailwind CSS
- **Chart.js** - Data visualization
- **Date-fns** - Date utilities
- **TypeScript** - Type safety

---

## 📱 Complete Pages

### ✅ Dashboard (/)
- Portfolio overview with 4 key metrics
- Recent trades preview (last 5)
- Active positions summary
- Latest learning insights
- Real-time data refresh

### ✅ Portfolio (/portfolio)
- Complete portfolio breakdown
- Asset allocation with progress bars
- Performance metrics grid
- Win rate, total trades, avg return
- Best/worst trade tracking
- Reset portfolio (danger zone)

### ✅ Positions (/positions)
- Active positions table
- Real-time P&L tracking
- Stop loss / take profit display
- Days held counter
- Quick sell action buttons
- Empty state when no positions

### ✅ Trade History (/trades)
- Complete trade log with filters
- Search by symbol
- Filter by status (All/Open/Closed)
- Trade details modal
- AI reasoning display
- P&L visualization
- Entry/exit prices

### ✅ AI Analysis (/analysis)
- Stock symbol search
- Real-time quote display
- Technical indicators (RSI, MACD, SMA, Volume)
- AI-powered BUY/SELL/HOLD recommendation
- Confidence score with progress bar
- Risk assessment badges
- Recent news with sentiment analysis
- Sentiment color coding

### ✅ Learning (/learning)
- Run learning review button
- Latest insight overview
- Win rate, avg return, total P&L
- AI reflection with insights
- Success factors (green alerts)
- Failure factors (red alerts)
- Pattern discovery with confidence levels
- Overall performance metrics
- Best trade highlights

### ✅ Execute Trade (/trade)
- Stock symbol input
- Live quote fetching
- BUY/SELL toggle buttons
- Quantity input (auto-calculated option)
- Position cost estimator
- Optional AI recommendation
- AI analysis before execution
- Mismatch warning if AI disagrees
- Automatic redirect after success

---

## 🎨 Design Features

### Nuxt UI v3 Components
- **UCard** - Clean card containers
- **UButton** - Modern buttons with loading states
- **UBadge** - Status indicators with colors
- **UIcon** - Heroicons integration
- **UInput** - Form inputs with icons
- **UFormGroup** - Form layout
- **UCheckbox** - Toggle options
- **UTable** - Data tables with custom cells
- **UModal** - Dialog/modal windows
- **UAlert** - Notifications and alerts
- **UProgress** - Progress bars
- **UAvatar** - Icon avatars
- **UDivider** - Section dividers
- **USelectMenu** - Dropdowns

### Color Scheme
- **Primary**: Green (trading/growth theme)
- **Success**: Green badges/indicators
- **Error**: Red warnings
- **Info**: Blue insights
- **Warning**: Yellow cautions

### Responsive Design
- Mobile-first approach
- Grid layouts adapt to screen size
- Touch-friendly buttons
- Readable typography at all sizes

---

## 🚀 Installation

```bash
cd "C:\Bitcraft Studio\eTrade Bot\trade-bot-dashboard"

# Install dependencies
npm install

# Start development server
npm run dev
```

Dashboard opens at: **http://localhost:3000**

---

## ⚙️ Configuration

**.env file:**
```env
NUXT_PUBLIC_API_URL=http://localhost:3001
```

**nuxt.config.ts:**
- Nuxt 4 compatibility enabled
- Nuxt UI module configured
- Dark mode preference set

---

## 📊 Features by Page

### Dashboard
- 4 stat cards: Total Value, P&L, Positions, Win Rate
- Recent 5 trades with status badges
- Active positions with live P&L
- Latest AI learning insight

### Portfolio
- Total value, cash, positions breakdown
- Asset allocation bars (cash vs positions)
- Performance grid (win rate, total trades, avg return, avg hold)
- Reset portfolio button

### Positions
- Sortable table of active positions
- Current price updates
- Unrealized P&L ($ and %)
- Stop loss & take profit levels
- Days held counter
- One-click sell buttons

### Trade History
- All trades in sortable table
- Filter: All/Open/Closed
- Search by symbol
- Trade details modal with:
  - Basic info (symbol, type, quantity, status)
  - Entry/exit prices and dates
  - AI reasoning and confidence
  - Risk assessment
  - P&L outcome

### AI Analysis
- Symbol search with live quotes
- Technical indicators:
  - Current price
  - RSI with oversold/overbought labels
  - MACD (value + signal)
  - Volume
  - 20-day & 50-day SMA
- AI recommendation (BUY/SELL/HOLD)
- Confidence percentage with progress bar
- Risk level badge
- Recent news headlines with sentiment

### Learning
- Run learning review (last 7 days)
- Latest insight card:
  - Trades reviewed count
  - Win rate percentage
  - Average return
  - Total P&L
  - AI reflection text
- Success factors (green)
- Failure factors (red)
- Pattern grid (up to 6):
  - Pattern name
  - Confidence badge
  - Description
  - Occurrences count
  - Success rate percentage
- Overall metrics:
  - Total trades
  - Win rate with progress bar
  - Avg return
  - Total P&L
  - Avg holding period
  - Best trade

### Execute Trade
- Symbol input (auto-uppercase)
- Get quote button
- Live quote display (price, change, %)
- BUY/SELL toggle
- Quantity input (optional)
- Position cost estimator
- AI recommendation toggle
- AI analysis card:
  - Recommendation badge
  - Reasoning text
  - Confidence score
  - Risk level
  - Mismatch warning
- Execute button
- Auto-redirect to trades page

---

## 🎯 Key Functions

### API Integration
All pages use `useApi()` composable:
```js
const api = useApi()
const { data, error } = await api.portfolio.getSummary()
```

### Currency Formatting
```js
const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}
```

### Toast Notifications
```js
const toast = useToast()
toast.add({
  title: 'Success',
  description: 'Trade executed',
  color: 'green',
})
```

### Auto-refresh
All pages listen for dashboard refresh events:
```js
onMounted(() => {
  fetchData()
  window.addEventListener('dashboard-refresh', fetchData)
})
```

---

## 🌓 Dark Mode

Dark mode enabled by default with smooth transitions. Toggle via button in header (sun/moon icon).

---

## 📱 Mobile Support

All pages are fully responsive:
- Stacked layouts on mobile
- Touch-friendly buttons
- Simplified tables
- Modal dialogs
- Readable text sizes

---

## 🚀 Quick Start

1. **Start Backend:**
   ```bash
   cd ../trade-bot-engine
   npm run start:dev
   ```

2. **Start Dashboard:**
   ```bash
   npm run dev
   ```

3. **Open Browser:**
   Navigate to `http://localhost:3000`

4. **Explore:**
   - View portfolio
   - Analyze stocks
   - Execute trades
   - Monitor positions
   - Review learning insights

---

## 📝 All Files Created

**Configuration:**
- ✅ package.json (Nuxt 4 + UI v3)
- ✅ nuxt.config.ts (Nuxt 4 compatibility)
- ✅ .env (API URL)
- ✅ app.vue (root component)
- ✅ .gitignore

**Layout:**
- ✅ layouts/default.vue (sidebar navigation)

**Pages:**
- ✅ pages/index.vue (Dashboard)
- ✅ pages/portfolio.vue (Portfolio details)
- ✅ pages/positions.vue (Active positions)
- ✅ pages/trades.vue (Trade history)
- ✅ pages/analysis.vue (AI stock analysis)
- ✅ pages/learning.vue (Learning insights)
- ✅ pages/trade.vue (Execute trades)

**Composables:**
- ✅ composables/useApi.ts (API wrapper)

---

## 💡 Next Steps

- Install dependencies: `npm install`
- Start dev server: `npm run dev`
- Execute test trades
- View AI analysis
- Review learning patterns

---

**Built with Nuxt 4 RC + Nuxt UI v3 Alpha**

Happy Trading! 🚀📈
