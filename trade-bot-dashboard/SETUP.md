# 🚀 Quick Setup Guide

Get your AI Trading Bot Dashboard running in 5 minutes!

## Prerequisites

✅ Node.js 18+ installed ([Download](https://nodejs.org))  
✅ Backend API running on `http://localhost:3001`  
✅ Terminal/PowerShell access

---

## Step 1: Install Dependencies

```bash
cd "C:\Bitcraft Studio\eTrade Bot\trade-bot-dashboard"
npm install
```

**Wait for installation to complete** (~2-3 minutes)

---

## Step 2: Start Development Server

```bash
npm run dev
```

**Expected output:**
```
Nuxt 4.3.0
  > Local:    http://localhost:3000/
  > Network:  http://192.168.x.x:3000/

✔ Vite client warmed up
✔ Nuxt Nitro server built
```

---

## Step 3: Open Browser

Navigate to: **http://localhost:3000**

You should see:
- Beautiful dashboard with sidebar
- Dark mode enabled by default
- Portfolio stats cards
- Recent trades section

---

## Step 4: Test Backend Connection

The dashboard automatically fetches data from your backend.

**If you see errors:**

1. **Make sure backend is running:**
   ```bash
   cd "C:\Bitcraft Studio\eTrade Bot\trade-bot-engine"
   npm run start:dev
   ```

2. **Verify backend is on port 3001:**
   Check console output should show: `Application is running on: http://localhost:3001`

3. **Check `.env` file:**
   ```env
   NUXT_PUBLIC_API_URL=http://localhost:3001
   ```

---

## Step 5: Explore the Dashboard

### What You'll See:

✅ **Portfolio Stats**
- Total Value: $5,000 (initial capital)
- Total P&L: $0.00 (no trades yet)
- Open Positions: 0
- Win Rate: 0%

✅ **Recent Trades**
- Empty initially
- Will populate after executing trades

✅ **Active Positions**
- Shows current holdings
- Real-time P&L updates

✅ **Learning Insights**
- AI analysis of performance
- Pattern detection

---

## Quick Actions

### Toggle Dark/Light Mode
Click the sun/moon icon in top right corner

### Refresh Data
Click the circular arrow icon to reload all data

### Navigate
Use the sidebar to explore different sections

---

## Next Steps

### Execute Your First Trade

1. Go to backend terminal
2. Execute a buy order via Postman or curl:
   ```bash
   curl -X POST http://localhost:3001/trading/buy \
     -H "Content-Type: application/json" \
     -d '{"symbol": "AAPL"}'
   ```
3. Refresh dashboard to see the new position!

### Explore Features

- **Portfolio** - Detailed breakdown (coming soon)
- **AI Analysis** - Get stock recommendations (coming soon)
- **Learning** - View pattern insights (coming soon)

---

## Troubleshooting

### Dashboard won't start

**Error:** `Cannot find module 'nuxt'`  
**Fix:** Run `npm install` again

**Error:** `Port 3000 already in use`  
**Fix:** Use different port:
```bash
npm run dev -- --port 3001
```

### Can't connect to backend

**Error:** `Failed to fetch` in browser console  
**Fix:**
1. Start backend: `cd trade-bot-engine && npm run start:dev`
2. Check it's on port 3001
3. Look for CORS errors in console

### Blank page

**Fix:**
1. Clear cache: `rm -rf .nuxt`
2. Reinstall: `npm install`
3. Restart: `npm run dev`

---

## Development Tips

### Hot Reload
- Changes to `.vue` files reload automatically
- No need to restart server

### View Console
- Press `F12` in browser
- Check Console tab for errors
- Check Network tab for API calls

### Stop Server
- Press `Ctrl + C` in terminal

---

## File Structure Overview

```
trade-bot-dashboard/
├── assets/css/        # Global styles
├── composables/       # Reusable logic
├── layouts/           # Layout components
├── pages/             # Route pages
├── nuxt.config.ts     # Nuxt configuration
└── .env               # Environment vars
```

---

## What's Next?

### Option 1: Test Current Dashboard
- Execute trades via backend
- Watch them appear on dashboard
- Monitor positions in real-time

### Option 2: Add More Pages
- Portfolio details page
- Trade execution interface
- AI analysis tool
- Learning insights visualizations

---

## Support

**Having issues?**

1. Check backend logs
2. Check browser console (F12)
3. Verify `.env` configuration
4. Restart both backend and frontend

---

**You're all set! Happy trading! 🚀📈**

Built with Nuxt 4.3 + Nuxt UI 3 + xAI Grok
