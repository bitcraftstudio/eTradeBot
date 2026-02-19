# Scheduler Page - Dashboard Guide

Control your automated trading bot from a beautiful UI! 🎛️⏰

## 🎯 What You Can Do

### ✅ View Scheduler Status
- See if automation is running
- Check total scans, auto-trades, and alerts
- View last scan time and next scheduled scan

### ✅ Control Automation
Toggle each feature ON/OFF with a simple switch:

1. **Morning Market Scan** ☀️
   - Runs at 9:00 AM EST (Mon-Fri)
   - Analyzes entire watchlist with AI
   - Finds trading opportunities

2. **Position Monitoring** 👁️
   - Runs every hour (10 AM - 4 PM EST)
   - Monitors stop loss/take profit
   - Auto-sells at triggers (if enabled)

3. **Daily Learning** 🎓
   - Runs at 5:00 PM EST (Mon-Fri)
   - Reviews closed trades
   - Extracts patterns and insights

### ✅ Configure Settings

**Automatic Trading** ⚡
- Toggle auto-trade ON/OFF
- **Warning:** RED when enabled (bot trades automatically!)
- **Safe:** GRAY when disabled (manual approval required)

**Watchlist** 📝
- Add/remove stocks to monitor
- Click X to remove a symbol
- Type symbol and click "Add" to add

**Trading Limits** 🛡️
- **Max Daily Trades:** Limit auto-executions per day
- **Min Confidence:** AI confidence threshold (0-100%)
  - Higher = safer (only high-confidence trades)
  - Lower = more aggressive (more trades, lower quality)

### ✅ Manual Market Scan
- Click "Run Scan Now" to analyze watchlist immediately
- View AI recommendations for each stock
- See confidence scores and reasoning
- Check how many trades were executed

---

## 🎨 Page Layout

### Header
```
Scheduler Status [Running/Stopped badge]

Stats Cards:
┌─────────────┬─────────────┬─────────────┬─────────────┐
│ Total Scans │ Auto Trades │ Total Alerts│  Next Scan  │
│     15      │      12     │      8      │ Feb 13 9AM  │
└─────────────┴─────────────┴─────────────┴─────────────┘
```

### Automation Cards
```
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│ Morning Scan ☀│  │Position Monitor│  │ Daily Learning │
│     [ON/OFF]   │  │    [ON/OFF]    │  │    [ON/OFF]    │
│                │  │                │  │                │
│ 9:00 AM EST   │  │ 10AM-4PM EST   │  │  5:00 PM EST   │
│ Last: Feb 12  │  │ Every hour     │  │ Last: Feb 12   │
└────────────────┘  └────────────────┘  └────────────────┘
```

### Configuration Panel
```
┌─────────────────────────────────────────────────────┐
│ Automatic Trading                        [ON/OFF]   │
│ ⚠️ Bot will execute trades automatically (RED)      │
└─────────────────────────────────────────────────────┘

Watchlist: [AAPL ✕] [MSFT ✕] [GOOGL ✕] [TSLA ✕]
           [Add symbol input] [Add button]

Max Daily Trades: [5] | Min Confidence: [70%] ███████░░░

[Save Configuration]
```

### Manual Scan Panel
```
┌─────────────────────────────────────────────────────┐
│ Manual Market Scan              [Run Scan Now]      │
│                                                      │
│ Scan Summary:                                        │
│ Analyzed: 7 | Recommendations: 7 | Trades Executed: 0│
│                                                      │
│ Recommendations Table:                               │
│ Symbol | Recommendation | Confidence | Price | Reasoning│
│ AAPL   | BUY ✓         | 85% ███   | $178  | Strong... │
│ MSFT   | HOLD ▬        | 65% ██    | $415  | Neutral...│
│ GOOGL  | BUY ✓         | 78% ███   | $142  | Positive..│
└─────────────────────────────────────────────────────┘
```

---

## 🚀 How to Use

### First Time Setup

1. **Navigate to Scheduler**
   - Click "Scheduler" in sidebar
   - See current status (all OFF by default)

2. **Configure Your Watchlist**
   - Add stocks you want to monitor
   - Start with 3-5 stocks for testing
   - Example: AAPL, MSFT, GOOGL

3. **Set Conservative Limits**
   - Max Daily Trades: 1-2
   - Min Confidence: 85%
   - Auto Trading: OFF

4. **Test Manual Scan**
   - Click "Run Scan Now"
   - Review AI recommendations
   - Check confidence scores
   - See reasoning for each stock

5. **Enable Morning Scan** (Optional)
   - Toggle "Morning Scan" ON
   - Bot will scan at 9 AM daily
   - Auto-trade stays OFF (safe!)
   - Review results in dashboard

6. **Enable Position Monitoring** (Optional)
   - Toggle "Position Monitoring" ON
   - Bot monitors positions hourly
   - Sends alerts to console

7. **Enable Daily Learning** (Optional)
   - Toggle "Daily Learning" ON
   - Bot learns at 5 PM daily
   - Check Learning page for insights

### After Testing (Enable Auto-Trade)

8. **Enable Automatic Trading** ⚠️
   - Review manual scan results first
   - Make sure limits are conservative
   - Toggle "Automatic Trading" ON
   - Card turns RED (warning!)
   - Click "Save Configuration"

9. **Monitor Closely**
   - Check dashboard daily
   - Review executed trades
   - Adjust watchlist as needed
   - Tweak confidence threshold

---

## 🎯 Example Workflows

### Conservative Testing
```
1. Add watchlist: AAPL, MSFT
2. Max trades: 1
3. Min confidence: 85%
4. Auto-trade: OFF
5. Enable morning scan: ON
6. Check results daily
```

### Aggressive Trading
```
1. Add watchlist: AAPL, MSFT, GOOGL, TSLA, NVDA
2. Max trades: 5
3. Min confidence: 75%
4. Auto-trade: ON
5. Enable all automation: ON
6. Monitor closely!
```

### Manual Control
```
1. Add watchlist: Your picks
2. Keep all automation: OFF
3. Use "Run Scan Now" when you want
4. Review AI recommendations
5. Execute trades manually
```

---

## 🎨 Visual Indicators

### Status Badges
- 🟢 **Green "Running"** - Scheduler active
- 🔴 **Red "Stopped"** - Scheduler inactive

### Toggle Switches
- ✅ **ON (Green)** - Feature enabled
- ⚪ **OFF (Gray)** - Feature disabled

### Auto-Trade Warning
- 🔴 **RED Card** - Auto-trade ON (danger!)
- ⚪ **GRAY Card** - Auto-trade OFF (safe)

### Recommendation Colors
- 🟢 **Green BUY** - AI recommends buying
- 🔴 **Red SELL** - AI recommends selling
- 🟡 **Yellow HOLD** - AI recommends holding

### Confidence Levels
- **85-100%**: Very high confidence ████████░░
- **70-84%**: High confidence ███████░░░
- **60-69%**: Medium confidence █████░░░░░
- **<60%**: Low confidence ███░░░░░░░

---

## 💡 Pro Tips

### Start Safe
- Begin with automation OFF
- Test manual scans first
- Review AI quality
- Understand the system

### Watchlist Management
- Start with 3-5 well-known stocks
- Don't add too many at once
- Remove underperformers
- Focus on high-confidence stocks

### Confidence Threshold
- **85%+**: Very safe, fewer trades
- **75%+**: Balanced approach
- **70%+**: More trades, some risk
- **<70%**: Not recommended

### Daily Routine
1. **Morning (9:15 AM)**: Check scan results
2. **Afternoon (2:00 PM)**: Monitor positions
3. **Evening (5:15 PM)**: Review learning insights
4. **Weekly**: Adjust watchlist and settings

### Monitor These
- Total auto-trades (are they profitable?)
- Win rate in Learning page
- Pattern insights
- Position alerts

---

## ⚠️ Safety Reminders

### Before Enabling Auto-Trade
- [ ] Test manual scans thoroughly
- [ ] Review AI recommendations quality
- [ ] Set conservative max trades (1-2)
- [ ] Set high min confidence (85%+)
- [ ] Add only trusted stocks to watchlist
- [ ] Understand the risks

### While Auto-Trading
- [ ] Check dashboard daily
- [ ] Review all executed trades
- [ ] Monitor win rate
- [ ] Adjust settings as needed
- [ ] Disable if performance drops

### Red Flags
- 🚩 Win rate below 50%
- 🚩 Multiple losing trades in a row
- 🚩 AI confidence often wrong
- 🚩 Unexpected trade executions
- 🚩 System errors or crashes

**If you see red flags: DISABLE auto-trade immediately!**

---

## 🔧 Troubleshooting

### "Scheduler Status" shows all zeros
- Backend might not be running
- Check backend console for errors
- Refresh the page

### Toggles don't save
- Check browser console for errors
- Make sure backend is on port 3001
- Try clicking "Save Configuration"

### "Run Scan Now" fails
- Watchlist might be empty
- Backend might be overloaded
- Check backend console logs

### Next scan time is wrong
- Timezone might be off
- Check backend server time
- Scheduled for Eastern Time (EST)

---

## 📱 Mobile Experience

The scheduler page is fully responsive:
- Cards stack on mobile
- Toggles are touch-friendly
- Table scrolls horizontally
- All features accessible

---

**You now have full control over your AI trading bot! 🤖📈**

Navigate to `/scheduler` to start! 🚀
