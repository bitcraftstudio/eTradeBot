# Stock Hunter Dashboard Page - User Guide

Your Stock Hunter dashboard is ready! 🎯

---

## 🚀 Access the Page

**URL:** http://localhost:3000/stock-hunter

**Navigation:** Click **"Stock Hunter"** in the sidebar (7th item)

---

## 🎨 Page Layout

### **Header Section**
```
┌─────────────────────────────────────────────────────────┐
│ 🔍 Stock Hunter                    [Demo Mode] [Refresh]│
│ Discover winning stocks across the market               │
├─────────────────────────────────────────────────────────┤
│ Total Found │ After Filters │ Avg Smart Score │ Avg Upside│
│     47      │      23       │      9.2        │   12.5%   │
└─────────────────────────────────────────────────────────┘
```

### **Filter Controls**
```
┌─────────────────────────────────────────────────────────┐
│ Hunt Filters                        [Reset] [Run Hunt]  │
├─────────────────────────────────────────────────────────┤
│ Smart Score Range: 8 - 10                               │
│ [Min: 8] [Max: 10]                                      │
│ [Perfect (10)] [Excellent (9-10)] [Good+ (8-10)]       │
│                                                          │
│ Minimum Upside (%): [5]    Min Market Cap ($B): [1]    │
│                                                          │
│ Smart Money Filters:                                    │
│ [Hedge Fund Activity: OFF] [Insider Buying: OFF]       │
│                                                          │
│ Advanced Filters:                                        │
│ Min Analyst Rating: [4.0]  Max Results: [50]           │
└─────────────────────────────────────────────────────────┘
```

### **Top Pick Banner**
```
┌─────────────────────────────────────────────────────────┐
│ ⭐ Top Pick                                              │
├─────────────────────────────────────────────────────────┤
│ Symbol         │ Smart Score    │ Upside Potential      │
│ NVDA           │ 10 / 10        │ 8.2%                  │
│ NVIDIA Corp    │ ██████████     │ Target: $950          │
│                                                          │
│ [Add to Watchlist] [View Details]                       │
└─────────────────────────────────────────────────────────┘
```

### **All Discoveries (Card Grid)**
```
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ NVDA      10 │ │ META       9 │ │ AVGO       9 │
│ NVIDIA Corp  │ │ Meta Plat..  │ │ Broadcom..   │
│              │ │              │ │              │
│ Price: $878  │ │ Price: $512  │ │ Price: $1680 │
│ Target: $950 │ │ Target: $580 │ │ Target: $1850│
│ Upside: 8.2% │ │ Upside: 13.3%│ │ Upside: 10.1%│
│              │ │              │ │              │
│ 🏦 HF Buying │ │ 🏦 HF Buying │ │ 👔 Insider   │
│ 👔 Insider   │ │ 📊 52 Buys   │ │ 📊 38 Buys   │
│ 📊 45 Buys   │ │              │ │              │
│              │ │              │ │              │
│ [Add to Watch│ │ [Add to Watch│ │ [Add to Watch│
└──────────────┘ └──────────────┘ └──────────────┘
```

---

## 🎯 How to Use

### **Step 1: Access the Page**
```
1. Start dashboard: npm run dev
2. Navigate to: http://localhost:3000/stock-hunter
3. Or click "Stock Hunter" in sidebar
```

### **Step 2: Adjust Filters**

**Quick Presets (Smart Score):**
- Click **"Perfect (10)"** → Only 10/10 stocks
- Click **"Excellent (9-10)"** → 9-10 scores
- Click **"Good+ (8-10)"** → 8-10 scores (default)

**Manual Adjustments:**
```
Smart Score Range:
  Min: [1-10]  (default: 8)
  Max: [1-10]  (default: 10)

Minimum Upside:
  [0-100]% (default: 5%)

Market Cap:
  [$B] (default: $1B minimum)

Analyst Rating:
  [1-5 scale] (default: 4.0 = Buy)

Max Results:
  [1-100] (default: 50)
```

**Smart Money Toggles:**
- Toggle **ON** "Hedge Fund Activity" → Only stocks hedge funds are buying
- Toggle **ON** "Insider Buying" → Only stocks with insider purchases

### **Step 3: Run Hunt**
```
1. Click "Run Hunt" button
2. Wait for results (2-5 seconds)
3. View discovered stocks
```

### **Step 4: Review Results**

**Top Pick Card:**
- Shows #1 ranked stock
- Smart Score, Upside %, Price Target
- One-click add to watchlist

**Stock Cards:**
- Click any card to see full details
- Shows price, target, upside
- Visual badges for signals (HF Buying, Insider Buy, Analyst Buys)

**Detail Modal:**
- Click "View Details" or click any stock card
- See analyst consensus breakdown
- View smart money activity
- Read AI-generated reasons
- Add to watchlist from modal

### **Step 5: Add to Watchlist**
```
1. Click "Add to Watchlist" on any stock
2. Stock automatically added to Scheduler watchlist
3. Morning scan will analyze it tomorrow
4. Toast confirmation appears
```

---

## 🎨 Visual Elements

### **Smart Score Badges**
- 🟢 **Green (9-10)**: Excellent picks
- 🔵 **Blue (8)**: Good picks
- 🟡 **Yellow (7)**: Neutral
- ⚪ **Gray (<7)**: Not recommended

### **Signal Badges**
- 🏦 **HF Buying**: Hedge funds accumulating
- 👔 **Insider Buy**: Executives buying stock
- 📊 **X Buys**: Number of analyst buy ratings

### **Status Badges**
- 🟢 **TipRanks Connected**: Real API active
- 🟡 **Demo Mode**: Using demo data (5 stocks)

---

## 📊 Example Workflows

### **Conservative Value Hunter**
```
1. Set Smart Score: 9-10 (click "Excellent")
2. Set Min Upside: 15%
3. Toggle ON: Hedge Fund Activity
4. Toggle ON: Insider Buying
5. Click "Run Hunt"

Result: Only the safest, highest-conviction picks
```

### **Growth Hunter**
```
1. Set Smart Score: 8-10 (click "Good+")
2. Set Min Upside: 20%
3. Set Min Analyst Rating: 4.5
4. Click "Run Hunt"

Result: High-growth opportunities
```

### **Smart Money Tracker**
```
1. Set Smart Score: 8-10
2. Toggle ON: Hedge Fund Activity
3. Toggle ON: Insider Buying
4. Click "Run Hunt"

Result: Stocks both smart money AND insiders are buying
```

### **Quick Scan**
```
1. Keep default settings
2. Click "Run Hunt"

Result: All quality stocks (Smart Score 8+)
```

---

## 🔧 Features

### ✅ **Interactive Filters**
- Real-time filter updates
- Quick preset buttons
- Visual sliders and toggles
- Reset to defaults

### ✅ **Rich Stock Cards**
- Symbol, name, scores
- Price, target, upside
- Signal badges
- One-click actions

### ✅ **Detailed View Modal**
- Full analyst breakdown
- Smart money activity
- AI reasoning
- Multiple actions

### ✅ **Watchlist Integration**
- Add directly to scheduler
- One-click operation
- Duplicate detection
- Success notifications

### ✅ **Responsive Design**
- Desktop: 3-column grid
- Tablet: 2-column grid
- Mobile: Single column
- Touch-friendly controls

---

## 🎯 Filter Recommendations

### **For Day Trading**
```
Smart Score: 9-10
Min Upside: 5-10%
Hedge Fund Activity: ON
Results: Quick movers with momentum
```

### **For Swing Trading**
```
Smart Score: 8-10
Min Upside: 15-25%
Insider Buying: ON
Results: Medium-term opportunities
```

### **For Long-Term Investing**
```
Smart Score: 9-10
Min Upside: 20%+
Hedge Fund Activity: ON
Insider Buying: ON
Min Market Cap: 5B+
Results: Quality long-term holds
```

---

## 💡 Pro Tips

### **Interpret Smart Scores**
- **10**: Perfect score → Highest conviction
- **9**: Outperform → Strong buy
- **8**: Good → Buy
- **7**: Neutral → Hold
- **<7**: Underperform → Avoid

### **Read the Signals**
- **HF Buying + Insider Buy**: Very bullish
- **High analyst buys (30+)**: Strong consensus
- **Upside >20%**: High potential
- **All three together**: Jackpot! 🎰

### **Use Filters Strategically**
- Start broad, then narrow
- Test different combinations
- Track which filters work best
- Adjust based on market conditions

### **Combine with Other Tools**
1. Hunt stocks here
2. Add to scheduler watchlist
3. Let morning scan analyze
4. Check AI Analysis page
5. Execute trade if confirmed

---

## 🐛 Troubleshooting

### **"No results found"**
- Filters too strict
- Try: Click "Reset" and "Run Hunt"
- Lower Smart Score minimum
- Reduce upside requirement

### **"Demo Mode" showing**
- TipRanks API key not set
- You'll get 5 demo stocks
- This is normal for testing!
- Add API key to `.env` for real data

### **Can't add to watchlist**
- Backend might be down
- Check: http://localhost:3001/scheduler/status
- Restart backend if needed

### **Stocks not showing**
- Click "Run Hunt" first
- Filters might exclude all stocks
- Try resetting filters

---

## 🎨 UI Color Guide

### **Score Colors**
- 🟢 Green: Perfect/Excellent (9-10)
- 🔵 Blue: Good (8)
- 🟡 Yellow: Neutral (7)
- ⚪ Gray: Low (<7)

### **Trend Colors**
- 🟢 Green: Increasing/Positive
- 🔴 Red: Decreasing/Negative
- ⚪ Gray: Stable/Neutral

### **Action Colors**
- 🔵 Primary: Main actions (Hunt, Add)
- ⚪ Gray: Secondary actions (Reset, Close)

---

## 📱 Mobile Experience

**Optimized for mobile:**
- ✅ Filters stack vertically
- ✅ Cards in single column
- ✅ Touch-friendly buttons
- ✅ Swipe-friendly modal
- ✅ Responsive typography

---

## 🔮 Coming Soon

- [ ] Save custom filter presets
- [ ] Compare multiple stocks side-by-side
- [ ] Track hunt performance over time
- [ ] Email notifications for hunts
- [ ] Export results to CSV
- [ ] Backtest hunter picks

---

## 🚀 Quick Start Commands

```bash
# Start dashboard
cd trade-bot-dashboard
npm run dev

# Open Stock Hunter
# Navigate to: http://localhost:3000/stock-hunter

# Or use direct link:
open http://localhost:3000/stock-hunter
```

---

## 📊 What You Can Do

✅ **Adjust Smart Score** (1-10 range, presets available)  
✅ **Set upside targets** (% to analyst price target)  
✅ **Filter by market cap** (exclude small caps)  
✅ **Require smart money** (hedge funds, insiders)  
✅ **Set analyst minimums** (1-5 rating scale)  
✅ **Limit results** (1-100 stocks)  
✅ **View detailed analysis** (click any stock)  
✅ **Add to watchlist** (one-click integration)  
✅ **Reset filters** (back to defaults)  
✅ **Real-time hunting** (instant results)  

---

**Your Stock Hunter dashboard is live and ready! 🎯🚀**

Navigate to http://localhost:3000/stock-hunter and start discovering winners!
