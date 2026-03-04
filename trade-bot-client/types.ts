// Type definitions for Trade Bot Client

export interface AnalystConsensus {
  averageRating: number;
  strongBuy: number;
  buy: number;
  hold: number;
  sell: number;
  strongSell: number;
}

export interface DiscoveredStock {
  id: string;
  symbol: string;
  name: string;
  smartScore: number;
  rating: string;
  currentPrice: number;
  priceTarget: number;
  upside: number;
  analystConsensus?: AnalystConsensus;
  hedgeFundTrend?: string;
  insiderSentiment?: string;
  newsSentiment: number;
  technicalScore: number;
  fundamentalScore: number;
  discoveredAt: string;
  lastUpdated: string;
  reasons: string[];
  addedToWatchlist: boolean;
  tradeExecuted: boolean;
  tradeId?: string;
}

export interface StockHunterFilters {
  minSmartScore: number;
  maxSmartScore: number;
  minUpside: number;
  minMarketCap: number;
  minAnalystRating: number;
  requiredHedgeFundActivity: boolean;
  requiredInsiderBuying: boolean;
  limit: number;
}

export interface StockHunterResult {
  totalFound: number;
  filtered: number;
  summary?: {
    topPick?: DiscoveredStock;
    avgSmartScore?: number;
    avgUpside?: number;
  };
  recommendations: DiscoveredStock[];
}

export interface StockHunterStatus {
  enabled: boolean;
  totalDiscovered: number;
  watchlistCount: number;
  lastHuntTime?: string;
  config: StockHunterConfig;
}

export interface StockHunterConfig {
  enabled: boolean;
  maxDiscoveries: number;
  filters: StockHunterFilters;
}

// Scheduler types
export interface SchedulerConfig {
  watchlist: string[];
  enabled: boolean;
  morningScanEnabled: boolean;
  positionMonitoringEnabled: boolean;
  dailyLearningEnabled: boolean;
}

export interface SchedulerStatus {
  enabled: boolean;
  morningScanEnabled: boolean;
  positionMonitoringEnabled: boolean;
  dailyLearningEnabled: boolean;
  config: SchedulerConfig;
  lastRun?: string;
}

// Portfolio types
export interface PositionSummary {
  symbol: string;
  quantity: number;
  avgPrice: number;
  currentPrice: number;
  currentValue: number;
  totalPnL: number;
  totalPnLPercent: number;
}

export interface PortfolioSummary {
  totalValue: number;
  cashBalance: number;
  investedValue: number;
  totalPnL: number;
  totalPnLPercent: number;
  openPositions: number;
  positions: PositionSummary[];
  lastUpdated: string;
}

// Trade types
export interface Trade {
  id: string;
  symbol: string;
  type: 'Buy' | 'Sell';
  quantity: number;
  price: number;
  totalValue: number;
  executedAt: string;
  status: string;
}

// Market data types
export interface StockQuote {
  symbol: string;
  price: number;
  previousClose: number;
  change: number;
  changePercent: number;
  open: number;
  high: number;
  low: number;
  volume: number;
  marketCap?: number;
  timestamp: string;
}

// API Response types
export interface ApiResponse<T> {
  data?: T;
  error?: string;
}