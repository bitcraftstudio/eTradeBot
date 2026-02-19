<template>
  <div class="space-y-6">
    <!-- Portfolio Summary Card -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-xl font-bold text-gray-900 dark:text-white">Portfolio Summary</h3>
          <UButton
            icon="i-heroicons-arrow-path"
            color="gray"
            variant="ghost"
            :loading="loading"
            @click="fetchData"
          />
        </div>
      </template>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div>
          <p class="text-sm text-gray-500 dark:text-gray-400">Total Value</p>
          <p class="text-3xl font-bold text-gray-900 dark:text-white mt-2">
            {{ formatCurrency(portfolio?.totalValue || 0) }}
          </p>
        </div>
        
        <div>
          <p class="text-sm text-gray-500 dark:text-gray-400">Cash Balance</p>
          <p class="text-3xl font-bold text-gray-900 dark:text-white mt-2">
            {{ formatCurrency(portfolio?.cashBalance || 0) }}
          </p>
        </div>
        
        <div>
          <p class="text-sm text-gray-500 dark:text-gray-400">Position Value</p>
          <p class="text-3xl font-bold text-gray-900 dark:text-white mt-2">
            {{ formatCurrency(portfolio?.positionValue || 0) }}
          </p>
        </div>
        
        <div>
          <p class="text-sm text-gray-500 dark:text-gray-400">Total P&L</p>
          <p class="text-3xl font-bold mt-2" :class="[
            (portfolio?.totalProfitLoss || 0) >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
          ]">
            {{ formatCurrency(portfolio?.totalProfitLoss || 0) }}
          </p>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
            {{ (portfolio?.totalProfitLossPercent || 0).toFixed(2) }}%
          </p>
        </div>
      </div>
    </UCard>

    <!-- Portfolio Breakdown -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Asset Allocation -->
      <UCard>
        <template #header>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Asset Allocation</h3>
        </template>

        <div class="space-y-4">
          <div>
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Cash</span>
              <span class="text-sm font-semibold text-gray-900 dark:text-white">
                {{ ((portfolio?.cashBalance || 0) / (portfolio?.totalValue || 1) * 100).toFixed(1) }}%
              </span>
            </div>
            <UProgress
              :value="(portfolio?.cashBalance || 0) / (portfolio?.totalValue || 1) * 100"
              color="blue"
            />
          </div>

          <div>
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Positions</span>
              <span class="text-sm font-semibold text-gray-900 dark:text-white">
                {{ ((portfolio?.positionValue || 0) / (portfolio?.totalValue || 1) * 100).toFixed(1) }}%
              </span>
            </div>
            <UProgress
              :value="(portfolio?.positionValue || 0) / (portfolio?.totalValue || 1) * 100"
              color="green"
            />
          </div>
        </div>
      </UCard>

      <!-- Performance Metrics -->
      <UCard>
        <template #header>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Performance Metrics</h3>
        </template>

        <div class="grid grid-cols-2 gap-4">
          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50">
            <p class="text-sm text-gray-500 dark:text-gray-400">Win Rate</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
              {{ ((metrics?.winRate || 0) * 100).toFixed(1) }}%
            </p>
          </div>

          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50">
            <p class="text-sm text-gray-500 dark:text-gray-400">Total Trades</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
              {{ metrics?.totalTrades || 0 }}
            </p>
          </div>

          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50">
            <p class="text-sm text-gray-500 dark:text-gray-400">Avg Return</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
              {{ ((metrics?.avgReturn || 0) * 100).toFixed(2) }}%
            </p>
          </div>

          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50">
            <p class="text-sm text-gray-500 dark:text-gray-400">Avg Hold</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
              {{ (metrics?.avgHoldingDays || 0).toFixed(1) }} days
            </p>
          </div>
        </div>
      </UCard>
    </div>

    <!-- Reset Portfolio (Danger Zone) -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-red-600 dark:text-red-400">Danger Zone</h3>
      </template>

      <div class="flex items-center justify-between">
        <div>
          <p class="font-medium text-gray-900 dark:text-white">Reset Portfolio</p>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Reset portfolio to initial $5,000 and clear all trades/positions
          </p>
        </div>
        <UButton
          color="red"
          variant="soft"
          @click="resetPortfolio"
        >
          Reset Portfolio
        </UButton>
      </div>
    </UCard>
  </div>
</template>

<script setup>
const api = useApi()
const toast = useToast()

const loading = ref(false)
const portfolio = ref(null)
const metrics = ref(null)

const fetchData = async () => {
  loading.value = true
  const [portfolioRes, metricsRes] = await Promise.all([
    api.portfolio.getSummary(),
    api.learning.getMetrics(),
  ])

  if (portfolioRes.data) portfolio.value = portfolioRes.data
  if (metricsRes.data) metrics.value = metricsRes.data
  loading.value = false
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

const resetPortfolio = async () => {
  if (confirm('Are you sure? This will delete all trades and reset to $5,000.')) {
    const { error } = await api.portfolio.reset()
    if (!error) {
      toast.add({
        title: 'Portfolio Reset',
        description: 'Portfolio has been reset to initial state',
        color: 'green',
      })
      fetchData()
    }
  }
}

onMounted(() => {
  fetchData()
  window.addEventListener('dashboard-refresh', fetchData)
})

onUnmounted(() => {
  window.removeEventListener('dashboard-refresh', fetchData)
})
</script>
