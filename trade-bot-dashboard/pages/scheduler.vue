<template>
  <div class="space-y-6">
    <!-- Scheduler Status Overview -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <UIcon name="i-heroicons-clock" class="w-6 h-6 text-primary-500" />
            <div>
              <h3 class="text-xl font-bold text-gray-900 dark:text-white">Scheduler Status</h3>
              <p class="text-sm text-gray-500 dark:text-gray-400">Automated trading and monitoring</p>
            </div>
          </div>
          <div class="flex items-center gap-3">
            <UToggle
              v-model="schedulerEnabled"
              :loading="togglingScheduler"
              @update:model-value="toggleScheduler"
            />
            <UBadge
              :color="schedulerEnabled ? 'green' : 'red'"
              size="lg"
              class="cursor-pointer"
              @click="toggleScheduler(!schedulerEnabled)"
            >
              {{ schedulerEnabled ? 'Running' : 'Stopped' }}
            </UBadge>
          </div>
        </div>
      </template>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50">
          <p class="text-sm text-gray-500 dark:text-gray-400">Total Scans</p>
          <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
            {{ status?.stats?.totalScans || 0 }}
          </p>
        </div>

        <div class="p-4 rounded-lg bg-green-50 dark:bg-green-900/20">
          <p class="text-sm text-gray-500 dark:text-gray-400">Auto Trades</p>
          <p class="text-2xl font-bold text-green-600 dark:text-green-400 mt-1">
            {{ status?.stats?.totalAutoTrades || 0 }}
          </p>
        </div>

        <div class="p-4 rounded-lg bg-yellow-50 dark:bg-yellow-900/20">
          <p class="text-sm text-gray-500 dark:text-gray-400">Total Alerts</p>
          <p class="text-2xl font-bold text-yellow-600 dark:text-yellow-400 mt-1">
            {{ status?.stats?.totalAlerts || 0 }}
          </p>
        </div>

        <div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
          <p class="text-sm text-gray-500 dark:text-gray-400">Next Scan</p>
          <p class="text-sm font-semibold text-blue-600 dark:text-blue-400 mt-1">
            {{ status?.nextScan ? formatDateTime(status.nextScan) : 'Not scheduled' }}
          </p>
        </div>
      </div>
    </UCard>

    <!-- Automation Controls -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Morning Market Scan -->
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-sun" class="w-5 h-5 text-yellow-500" />
              <h4 class="font-semibold text-gray-900 dark:text-white">Morning Scan</h4>
            </div>
            <UToggle
              v-model="config.morningScanEnabled"
              @update:model-value="toggleMorningScan"
            />
          </div>
        </template>

        <div class="space-y-3">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Analyzes watchlist at 9:00 AM EST (Mon-Fri) and identifies trading opportunities
          </p>
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-500 dark:text-gray-400">Schedule:</span>
            <span class="font-medium text-gray-900 dark:text-white">9:00 AM EST</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-500 dark:text-gray-400">Last Scan:</span>
            <span class="font-medium text-gray-900 dark:text-white">
              {{ status?.lastScan ? formatDateTime(status.lastScan) : 'Never' }}
            </span>
          </div>
        </div>
      </UCard>

      <!-- Position Monitoring -->
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-eye" class="w-5 h-5 text-purple-500" />
              <h4 class="font-semibold text-gray-900 dark:text-white">Position Monitor</h4>
            </div>
            <UToggle
              v-model="config.positionMonitoringEnabled"
              @update:model-value="togglePositionMonitoring"
            />
          </div>
        </template>

        <div class="space-y-3">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Monitors stop loss and take profit levels hourly during market hours
          </p>
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-500 dark:text-gray-400">Schedule:</span>
            <span class="font-medium text-gray-900 dark:text-white">10 AM - 4 PM EST</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-500 dark:text-gray-400">Frequency:</span>
            <span class="font-medium text-gray-900 dark:text-white">Every hour</span>
          </div>
        </div>
      </UCard>

      <!-- Daily Learning -->
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-academic-cap" class="w-5 h-5 text-blue-500" />
              <h4 class="font-semibold text-gray-900 dark:text-white">Daily Learning</h4>
            </div>
            <UToggle
              v-model="config.dailyLearningEnabled"
              @update:model-value="toggleDailyLearning"
            />
          </div>
        </template>

        <div class="space-y-3">
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Reviews closed trades and extracts patterns at 5:00 PM EST (Mon-Fri)
          </p>
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-500 dark:text-gray-400">Schedule:</span>
            <span class="font-medium text-gray-900 dark:text-white">5:00 PM EST</span>
          </div>
          <div class="flex items-center justify-between text-sm">
            <span class="text-gray-500 dark:text-gray-400">Last Review:</span>
            <span class="font-medium text-gray-900 dark:text-white">
              {{ status?.stats?.lastLearningReview ? formatDateTime(status.stats.lastLearningReview) : 'Never' }}
            </span>
          </div>
        </div>
      </UCard>
    </div>

    <!-- Configuration -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Configuration</h3>
      </template>

      <div class="space-y-6">
        <!-- Auto-Trade Toggle -->
        <div class="p-4 rounded-lg border-2" :class="[
          config.autoTradeEnabled 
            ? 'border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/20' 
            : 'border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50'
        ]">
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <div class="flex items-center gap-2 mb-2">
                <UIcon 
                  :name="config.autoTradeEnabled ? 'i-heroicons-bolt' : 'i-heroicons-bolt-slash'" 
                  class="w-5 h-5" 
                  :class="config.autoTradeEnabled ? 'text-red-600 dark:text-red-400' : 'text-gray-400'"
                />
                <h4 class="font-semibold" :class="config.autoTradeEnabled ? 'text-red-900 dark:text-red-100' : 'text-gray-900 dark:text-white'">
                  Automatic Trading
                </h4>
              </div>
              <p class="text-sm" :class="config.autoTradeEnabled ? 'text-red-700 dark:text-red-300' : 'text-gray-600 dark:text-gray-400'">
                {{ config.autoTradeEnabled 
                  ? '⚠️ Bot will execute trades automatically based on AI recommendations' 
                  : 'Manual approval required for all trades' }}
              </p>
            </div>
            <UToggle
              v-model="config.autoTradeEnabled"
            />
          </div>
        </div>

        <!-- Watchlist -->
        <UFormGroup label="Watchlist" :hint="`${watchlist.length} symbols`">
          <div class="space-y-3">
            <div class="flex gap-2">
              <UInput
                v-model="newSymbol"
                placeholder="Enter symbol (e.g., AAPL)"
                class="flex-1"
                @keyup.enter="addSymbol"
              />
              <UButton @click="addSymbol" :disabled="!newSymbol.trim()">Add</UButton>
              <UButton 
                v-if="watchlist.length > 0" 
                color="red" 
                variant="soft"
                @click="clearWatchlist"
              >
                Clear All
              </UButton>
            </div>
            
            <!-- Watchlist chips -->
            <div v-if="watchlist.length > 0" class="flex flex-wrap gap-2">
              <div
                v-for="(symbol, index) in watchlist"
                :key="`${symbol}-${index}`"
                class="inline-flex items-center gap-1 px-3 py-1.5 rounded-full bg-primary-100 dark:bg-primary-900 text-primary-800 dark:text-primary-200 text-sm font-medium"
              >
                {{ symbol }}
                <button
                  type="button"
                  class="ml-1 p-0.5 rounded-full hover:bg-primary-200 dark:hover:bg-primary-800 focus:outline-none focus:ring-2 focus:ring-primary-500"
                  @click.stop.prevent="removeSymbol(symbol)"
                >
                  <UIcon name="i-heroicons-x-mark" class="w-4 h-4" />
                </button>
              </div>
            </div>
            
            <!-- Empty state -->
            <div v-else class="text-sm text-gray-500 dark:text-gray-400 italic">
              No symbols in watchlist. Add some to start monitoring.
            </div>
          </div>
        </UFormGroup>

        <!-- Trading Limits -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <UFormGroup label="Max Daily Trades" hint="Maximum auto-trades per day">
            <UInput
              v-model.number="config.maxDailyTrades"
              type="number"
              min="1"
              max="20"
            />
          </UFormGroup>

          <UFormGroup label="Min Confidence" hint="Minimum AI confidence (0-100%)">
            <div class="space-y-2">
              <UInput
                :model-value="(config.minConfidence * 100).toFixed(0)"
                type="number"
                min="0"
                max="100"
                @update:model-value="config.minConfidence = $event / 100"
              />
              <UProgress :value="config.minConfidence * 100" />
            </div>
          </UFormGroup>
        </div>

        <!-- Unsaved changes indicator -->
        <UAlert
          v-if="hasUnsavedChanges"
          icon="i-heroicons-exclamation-triangle"
          color="yellow"
          variant="soft"
          title="Unsaved Changes"
          description="You have unsaved changes. Click 'Save Configuration' to persist them."
        />

        <!-- Save Button -->
        <div class="flex gap-3">
          <UButton
            class="flex-1"
            size="lg"
            :loading="saving"
            :color="hasUnsavedChanges ? 'primary' : 'gray'"
            @click="saveConfig"
          >
            {{ hasUnsavedChanges ? 'Save Configuration' : 'Configuration Saved' }}
          </UButton>
          <UButton
            size="lg"
            color="red"
            variant="outline"
            :loading="resetting"
            @click="resetConfig"
          >
            Reset to Defaults
          </UButton>
        </div>
      </div>
    </UCard>

    <!-- Manual Scan -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Manual Market Scan</h3>
          <UButton
            :loading="scanning"
            @click="triggerScan"
          >
            <UIcon name="i-heroicons-magnifying-glass" class="mr-2" />
            Run Scan Now
          </UButton>
        </div>
      </template>

      <p class="text-gray-600 dark:text-gray-400">
        Manually trigger a market scan to analyze your watchlist and get AI recommendations
      </p>

      <!-- Scan Results -->
      <div v-if="scanResults" class="mt-6 space-y-4">
        <div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
          <div class="flex items-center gap-2 mb-2">
            <UIcon name="i-heroicons-information-circle" class="w-5 h-5 text-blue-600 dark:text-blue-400" />
            <h4 class="font-semibold text-blue-900 dark:text-blue-100">Scan Summary</h4>
          </div>
          <div class="grid grid-cols-3 gap-4 text-sm">
            <div>
              <span class="text-blue-700 dark:text-blue-300">Analyzed:</span>
              <span class="font-bold text-blue-900 dark:text-blue-100 ml-2">
                {{ scanResults.symbolsAnalyzed }}
              </span>
            </div>
            <div>
              <span class="text-blue-700 dark:text-blue-300">Recommendations:</span>
              <span class="font-bold text-blue-900 dark:text-blue-100 ml-2">
                {{ scanResults.recommendations?.length || 0 }}
              </span>
            </div>
            <div>
              <span class="text-blue-700 dark:text-blue-300">Trades Executed:</span>
              <span class="font-bold text-blue-900 dark:text-blue-100 ml-2">
                {{ scanResults.tradesExecuted }}
              </span>
            </div>
          </div>
        </div>

        <!-- Recommendations Table -->
        <UTable
          v-if="scanResults.recommendations?.length > 0"
          :rows="scanResults.recommendations"
          :columns="[
            { key: 'symbol', label: 'Symbol' },
            { key: 'recommendation', label: 'Recommendation' },
            { key: 'confidence', label: 'Confidence' },
            { key: 'price', label: 'Price' },
            { key: 'reasoning', label: 'AI Reasoning' },
          ]"
        >
          <template #recommendation-data="{ row }">
            <UBadge
              :color="getRecommendationColor(row.recommendation)"
              variant="subtle"
            >
              {{ row.recommendation }}
            </UBadge>
          </template>

          <template #confidence-data="{ row }">
            <div>
              <p class="font-semibold text-gray-900 dark:text-white">
                {{ (row.confidence * 100).toFixed(0) }}%
              </p>
              <UProgress :value="row.confidence * 100" size="xs" class="mt-1" />
            </div>
          </template>

          <template #price-data="{ row }">
            <p class="font-medium text-gray-900 dark:text-white">
              {{ formatCurrency(row.price) }}
            </p>
          </template>

          <template #reasoning-data="{ row }">
            <p class="text-sm text-gray-600 dark:text-gray-400 max-w-md truncate">
              {{ row.reasoning }}
            </p>
          </template>
        </UTable>

        <!-- Errors -->
        <UAlert
          v-if="scanResults.errors?.length > 0"
          icon="i-heroicons-exclamation-triangle"
          color="red"
          title="Errors During Scan"
          :description="`${scanResults.errors.length} errors occurred`"
        />
      </div>
    </UCard>
  </div>
</template>

<script setup>
import { format } from 'date-fns'

const api = useApi()
const toast = useToast()

const loading = ref(false)
const scanning = ref(false)
const saving = ref(false)
const resetting = ref(false)
const togglingScheduler = ref(false)
const status = ref(null)
const schedulerEnabled = ref(true)

// Separate watchlist as its own reactive array for better tracking
const watchlist = ref([])
const savedWatchlist = ref([])

const config = ref({
  morningScanEnabled: false,
  positionMonitoringEnabled: false,
  dailyLearningEnabled: false,
  autoTradeEnabled: false,
  maxDailyTrades: 5,
  minConfidence: 0.7,
})
const savedConfig = ref(null)

const newSymbol = ref('')
const scanResults = ref(null)

// Track unsaved changes
const hasUnsavedChanges = computed(() => {
  if (!savedConfig.value) return false
  
  // Check watchlist changes
  const watchlistChanged = JSON.stringify([...watchlist.value].sort()) !== JSON.stringify([...savedWatchlist.value].sort())
  
  // Check other config changes
  const configChanged = 
    config.value.autoTradeEnabled !== savedConfig.value.autoTradeEnabled ||
    config.value.maxDailyTrades !== savedConfig.value.maxDailyTrades ||
    config.value.minConfidence !== savedConfig.value.minConfidence
  
  return watchlistChanged || configChanged
})

// Fetch scheduler status
const fetchStatus = async () => {
  loading.value = true
  const { data } = await api.scheduler.getStatus()
  if (data) {
    status.value = data
    
    // Update scheduler enabled state
    schedulerEnabled.value = data.config?.schedulerEnabled ?? true
    
    // Update config
    config.value = {
      morningScanEnabled: data.config?.morningScanEnabled ?? false,
      positionMonitoringEnabled: data.config?.positionMonitoringEnabled ?? false,
      dailyLearningEnabled: data.config?.dailyLearningEnabled ?? false,
      autoTradeEnabled: data.config?.autoTradeEnabled ?? false,
      maxDailyTrades: data.config?.maxDailyTrades ?? 5,
      minConfidence: data.config?.minConfidence ?? 0.7,
    }
    
    // Update watchlist (make a copy)
    watchlist.value = [...(data.config?.watchlist || [])]
    
    // Save original values for change detection
    savedConfig.value = { ...config.value }
    savedWatchlist.value = [...watchlist.value]
    
    console.log('Loaded watchlist:', watchlist.value)
  }
  loading.value = false
}

// Toggle master scheduler on/off
const toggleScheduler = async (enabled) => {
  togglingScheduler.value = true
  const endpoint = enabled ? api.scheduler.enableScheduler : api.scheduler.disableScheduler
  const { error } = await endpoint()
  
  if (!error) {
    schedulerEnabled.value = enabled
    toast.add({
      title: enabled ? 'Scheduler Started' : 'Scheduler Stopped',
      description: enabled 
        ? 'Automated tasks will now run on schedule' 
        : 'All automated tasks have been paused',
      color: enabled ? 'green' : 'yellow',
    })
  } else {
    // Revert on error
    schedulerEnabled.value = !enabled
    toast.add({
      title: 'Error',
      description: error || 'Failed to toggle scheduler',
      color: 'red',
    })
  }
  togglingScheduler.value = false
}

// Toggle morning scan
const toggleMorningScan = async (enabled) => {
  const endpoint = enabled ? api.scheduler.enableMorningScan : api.scheduler.disableMorningScan
  const { error } = await endpoint()
  
  if (!error) {
    toast.add({
      title: enabled ? 'Morning Scan Enabled' : 'Morning Scan Disabled',
      description: enabled ? 'Market scans will run at 9 AM EST' : 'Morning scans are now disabled',
      color: enabled ? 'green' : 'gray',
    })
  } else {
    config.value.morningScanEnabled = !enabled
  }
}

// Toggle position monitoring
const togglePositionMonitoring = async (enabled) => {
  const endpoint = enabled ? api.scheduler.enablePositionMonitoring : api.scheduler.disablePositionMonitoring
  const { error } = await endpoint()
  
  if (!error) {
    toast.add({
      title: enabled ? 'Position Monitoring Enabled' : 'Position Monitoring Disabled',
      description: enabled ? 'Positions will be monitored hourly' : 'Position monitoring is now disabled',
      color: enabled ? 'green' : 'gray',
    })
  } else {
    config.value.positionMonitoringEnabled = !enabled
  }
}

// Toggle daily learning
const toggleDailyLearning = async (enabled) => {
  const endpoint = enabled ? api.scheduler.enableDailyLearning : api.scheduler.disableDailyLearning
  const { error } = await endpoint()
  
  if (!error) {
    toast.add({
      title: enabled ? 'Daily Learning Enabled' : 'Daily Learning Disabled',
      description: enabled ? 'Learning reviews will run at 5 PM EST' : 'Daily learning is now disabled',
      color: enabled ? 'green' : 'gray',
    })
  } else {
    config.value.dailyLearningEnabled = !enabled
  }
}

// Save configuration
const saveConfig = async () => {
  saving.value = true
  
  console.log('Saving watchlist:', watchlist.value)
  
  const { data, error } = await api.scheduler.updateConfig({
    watchlist: watchlist.value,
    maxDailyTrades: config.value.maxDailyTrades,
    minConfidence: config.value.minConfidence,
    autoTradeEnabled: config.value.autoTradeEnabled,
  })

  if (!error && data) {
    // Update from response
    if (data.config) {
      watchlist.value = [...(data.config.watchlist || [])]
      savedWatchlist.value = [...watchlist.value]
      savedConfig.value = { ...config.value }
    }
    
    toast.add({
      title: 'Configuration Saved',
      description: `Watchlist now has ${watchlist.value.length} symbols`,
      color: 'green',
    })
    
    console.log('Saved watchlist:', watchlist.value)
  } else {
    toast.add({
      title: 'Error',
      description: error || 'Failed to save configuration',
      color: 'red',
    })
  }
  saving.value = false
}

// Add symbol to watchlist
const addSymbol = () => {
  const symbol = newSymbol.value.toUpperCase().trim()
  if (symbol && !watchlist.value.includes(symbol)) {
    watchlist.value = [...watchlist.value, symbol]
    newSymbol.value = ''
    console.log('Added symbol, watchlist now:', watchlist.value)
  }
}

// Remove symbol from watchlist
const removeSymbol = (symbolToRemove) => {
  console.log('Removing symbol:', symbolToRemove)
  console.log('Before:', watchlist.value)
  watchlist.value = watchlist.value.filter(s => s !== symbolToRemove)
  console.log('After:', watchlist.value)
}

// Clear entire watchlist
const clearWatchlist = () => {
  console.log('Clearing watchlist')
  watchlist.value = []
}

// Reset configuration to defaults
const resetConfig = async () => {
  resetting.value = true
  
  const { data, error } = await api.scheduler.resetConfig()
  
  if (!error && data) {
    // Update from response
    if (data.config) {
      schedulerEnabled.value = data.config.schedulerEnabled ?? true
      config.value = {
        morningScanEnabled: data.config.morningScanEnabled ?? false,
        positionMonitoringEnabled: data.config.positionMonitoringEnabled ?? false,
        dailyLearningEnabled: data.config.dailyLearningEnabled ?? false,
        autoTradeEnabled: data.config.autoTradeEnabled ?? false,
        maxDailyTrades: data.config.maxDailyTrades ?? 5,
        minConfidence: data.config.minConfidence ?? 0.7,
      }
      watchlist.value = [...(data.config.watchlist || [])]
      savedWatchlist.value = [...watchlist.value]
      savedConfig.value = { ...config.value }
    }
    
    toast.add({
      title: 'Configuration Reset',
      description: 'Settings have been reset to defaults',
      color: 'green',
    })
  } else {
    toast.add({
      title: 'Error',
      description: error || 'Failed to reset configuration',
      color: 'red',
    })
  }
  resetting.value = false
}

// Trigger manual scan
const triggerScan = async () => {
  scanning.value = true
  scanResults.value = null
  
  const { data, error } = await api.scheduler.triggerScan()
  
  if (data) {
    scanResults.value = data
    toast.add({
      title: 'Market Scan Complete',
      description: `Analyzed ${data.symbolsAnalyzed} symbols, executed ${data.tradesExecuted} trades`,
      color: 'green',
    })
  } else {
    toast.add({
      title: 'Error',
      description: error || 'Failed to run market scan',
      color: 'red',
    })
  }
  
  scanning.value = false
}

// Format currency
const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

// Format date/time
const formatDateTime = (date) => {
  return format(new Date(date), 'MMM dd, yyyy h:mm a')
}

// Get recommendation color
const getRecommendationColor = (rec) => {
  const colors = {
    'BUY': 'green',
    'SELL': 'red',
    'HOLD': 'yellow',
  }
  return colors[rec] || 'gray'
}

// Fetch data on mount
onMounted(() => {
  fetchStatus()
  window.addEventListener('dashboard-refresh', fetchStatus)
})

onUnmounted(() => {
  window.removeEventListener('dashboard-refresh', fetchStatus)
})
</script>
