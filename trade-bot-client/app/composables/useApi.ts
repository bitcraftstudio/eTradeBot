export const useApi = () => {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiUrl
  const { startLoading, stopLoading } = useLoading()

  const fetchWithError = async (url: string, options: any = {}, loadingMessage?: string) => {
    // Only show loading for non-silent requests
    const showLoading = options.silent !== true

    if (showLoading && loadingMessage) {
      startLoading(loadingMessage)
    }

    try {
      const response = await $fetch(url, {
        baseURL,
        ...options,
      })
      return { data: response, error: null }
    } catch (error: any) {
      console.error('API Error:', error)
      return { data: null, error: error.data || error.message }
    } finally {
      if (showLoading && loadingMessage) {
        stopLoading()
      }
    }
  }

  return {
    // Portfolio
    portfolio: {
      getSummary: () => fetchWithError('/portfolio/summary', {}, 'Loading portfolio...'),
      getPositions: () => fetchWithError('/portfolio/positions', {}, 'Loading positions...'),
      sync: () => fetchWithError('/portfolio/sync', { method: 'POST' }, 'Syncing portfolio...'),
    },

    // Positions
    positions: {
      getAll: () => fetchWithError('/portfolio/positions', {}, 'Loading positions...'),
      update: () => fetchWithError('/portfolio/sync', { method: 'POST' }, 'Updating positions...'),
      getSellSignals: () => fetchWithError('/portfolio/positions/sell-signals', { silent: true }),
      getAnalysis: (symbol: string) => fetchWithError(`/portfolio/positions/${symbol}/analysis`, {}, `Analyzing ${symbol}...`),
    },

    // Trades
    trades: {
      getAll: () => fetchWithError('/trading/trades', {}, 'Loading trades...'),
      getOpen: () => fetchWithError('/trading/trades/open', {}, 'Loading open trades...'),
      getClosed: () => fetchWithError('/trading/trades/closed', {}, 'Loading closed trades...'),
      getById: (tradeId: string) => fetchWithError(`/trading/trades/${tradeId}`, {}, 'Loading trade details...'),
      execute: (payload: any) => fetchWithError('/trading/execute', {
        method: 'POST',
        body: payload,
      }, 'Executing trade...'),
      buy: (payload: any) => fetchWithError('/trading/execute', {
        method: 'POST',
        body: { ...payload, type: 'Buy' },
      }, 'Placing buy order...'),
      sell: (payload: any) => fetchWithError('/trading/execute', {
        method: 'POST',
        body: { ...payload, type: 'Sell' },
      }, 'Placing sell order...'),
    },

    // Market Data / Quotes
    market: {
      getQuote: (symbol: string) => fetchWithError(`/analysis/${symbol}/quote`, {}, `Getting ${symbol} quote...`),
      getIndicators: (symbol: string) => fetchWithError(`/analysis/${symbol}/indicators`, {}, `Loading ${symbol} indicators...`),
      getRisk: (symbol: string, params: any = {}) => {
        const query = new URLSearchParams(params).toString()
        return fetchWithError(`/analysis/${symbol}/risk${query ? `?${query}` : ''}`, {}, 'Calculating risk...')
      },
    },

    // AI Analysis
    ai: {
      analyzeStock: (symbol: string) => fetchWithError(`/analysis/${symbol}`, {}, `Analyzing ${symbol} with AI...`),
      analyzeMultiple: (symbols: string[]) => fetchWithError(`/analysis/multi?symbols=${symbols.join(',')}`, {}, 'Running AI analysis...'),
    },

    // Accounts
    accounts: {
      getAll: () => fetchWithError('/accounts', {}, 'Loading accounts...'),
      getBalance: (accountId: string) => fetchWithError(`/accounts/${accountId}/balance`, {}, 'Loading balance...'),
      getOrders: (accountId: string) => fetchWithError(`/accounts/${accountId}/orders`, {}, 'Loading orders...'),
    },

    // Learning
    learning: {
      getInsights: () => fetchWithError('/learning/insights', {}, 'Loading insights...'),
      getLatest: () => fetchWithError('/learning/insights/latest', { silent: true }),
      getPerformance: () => fetchWithError('/learning/performance', {}, 'Loading performance data...'),
      getStrategy: () => fetchWithError('/learning/strategy', { silent: true }),
      performReview: (days = 7) => fetchWithError(`/learning/review?days=${days}`, { method: 'POST' }, 'Running learning review...'),
      getMetrics: () => fetchWithError('/learning/metrics', {}, 'Loading metrics...'),
    },

    // Scheduler
    scheduler: {
      getStatus: () => fetchWithError('/scheduler/status', { silent: true }),
      getConfig: () => fetchWithError('/scheduler/config', { silent: true }),
      updateConfig: (config: any) => fetchWithError('/scheduler/config', {
        method: 'PATCH',
        body: config,
      }, 'Saving configuration...'),
      resetConfig: () => fetchWithError('/scheduler/config', { method: 'DELETE' }, 'Resetting configuration...'),
      scan: () => fetchWithError('/scheduler/scan', { method: 'POST' }, 'Running market scan...'),
      monitor: () => fetchWithError('/scheduler/monitor', { method: 'POST' }, 'Checking positions...'),
      learn: () => fetchWithError('/scheduler/learn', { method: 'POST' }, 'Running learning review...'),
      triggerScan: () => fetchWithError('/scheduler/scan', { method: 'POST' }, 'Running market scan...'),
      // Master scheduler toggle
      enableScheduler: () => fetchWithError('/scheduler/enable', { method: 'POST' }, 'Starting scheduler...'),
      disableScheduler: () => fetchWithError('/scheduler/disable', { method: 'POST' }, 'Stopping scheduler...'),
      // Individual feature toggles
      enableMorningScan: () => fetchWithError('/scheduler/enable/morning-scan', { method: 'POST' }),
      disableMorningScan: () => fetchWithError('/scheduler/disable/morning-scan', { method: 'POST' }),
      enablePositionMonitoring: () => fetchWithError('/scheduler/enable/position-monitoring', { method: 'POST' }),
      disablePositionMonitoring: () => fetchWithError('/scheduler/disable/position-monitoring', { method: 'POST' }),
      enableDailyLearning: () => fetchWithError('/scheduler/enable/daily-learning', { method: 'POST' }),
      disableDailyLearning: () => fetchWithError('/scheduler/disable/daily-learning', { method: 'POST' }),
    },

    // Stock Hunter
    stockHunter: {
      getStatus: () => fetchWithError('/stocks/status', { silent: true }),
      getConfig: () => fetchWithError('/stocks/config', { silent: true }),
      updateConfig: (config: any) => fetchWithError('/stocks/config', {
        method: 'PUT',
        body: config,
      }, 'Saving configuration...'),
      hunt: (filters: any) => fetchWithError('/stocks/hunt', {
        method: 'POST',
        body: filters,
      }, 'Hunting for stocks...'),
      getDiscovered: () => fetchWithError('/stocks/discovered', {}, 'Loading discovered stocks...'),
      addToWatchlist: (symbol: string) => fetchWithError(`/stocks/${symbol}/watchlist`, { method: 'POST' }, `Adding ${symbol} to watchlist...`),
      quickHunt: (params: any) => {
        const query = new URLSearchParams(params).toString()
        return fetchWithError(`/stocks/hunt?${query}`, {}, 'Quick hunting...')
      },
    },

    // Health
    health: {
      check: () => fetchWithError('/health', { silent: true }),
      config: () => fetchWithError('/health/config', { silent: true }),
    },
  }
}
