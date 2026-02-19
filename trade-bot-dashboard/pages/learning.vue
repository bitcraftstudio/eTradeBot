<template>
  <div class="space-y-6">
    <!-- Run Learning Review -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-xl font-bold">Learning Review</h3>
          <UButton
            :loading="running"
            @click="runLearningReview"
          >
            Run Learning Review
          </UButton>
        </div>
      </template>

      <p class="text-gray-600 dark:text-gray-400">
        Analyze recent trades to extract patterns and insights. The AI will review your trading history
        and provide recommendations for improvement.
      </p>
    </UCard>

    <!-- Latest Insight -->
    <UCard v-if="latestInsight">
      <template #header>
        <div class="flex items-center gap-2">
          <UIcon name="i-heroicons-sparkles" class="w-5 h-5 text-primary-500" />
          <h3 class="text-lg font-semibold">Latest Learning Insight</h3>
        </div>
      </template>

      <div class="space-y-6">
        <!-- Overview Stats -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
            <p class="text-sm text-gray-500 dark:text-gray-400">Trades Reviewed</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
              {{ latestInsight.tradesReviewed }}
            </p>
          </div>

          <div class="p-4 rounded-lg bg-green-50 dark:bg-green-900/20">
            <p class="text-sm text-gray-500 dark:text-gray-400">Win Rate</p>
            <p class="text-2xl font-bold text-green-600 dark:text-green-400 mt-1">
              {{ ((latestInsight.winRate || 0) * 100).toFixed(1) }}%
            </p>
          </div>

          <div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
            <p class="text-sm text-gray-500 dark:text-gray-400">Avg Return</p>
            <p class="text-2xl font-bold text-blue-600 dark:text-blue-400 mt-1">
              {{ ((latestInsight.avgReturn || 0) * 100).toFixed(2) }}%
            </p>
          </div>

          <div class="p-4 rounded-lg bg-purple-50 dark:bg-purple-900/20">
            <p class="text-sm text-gray-500 dark:text-gray-400">Total P&L</p>
            <p class="text-2xl font-bold mt-1" :class="[
              latestInsight.totalProfitLoss >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
            ]">
              {{ formatCurrency(latestInsight.totalProfitLoss) }}
            </p>
          </div>
        </div>

        <!-- AI Reflection -->
        <div v-if="latestInsight.aiReflection" class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
          <div class="flex items-start gap-3">
            <UIcon name="i-heroicons-light-bulb" class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5" />
            <div class="flex-1">
              <p class="text-sm font-medium text-blue-900 dark:text-blue-300 mb-2">AI Reflection</p>
              <p class="text-blue-800 dark:text-blue-200">{{ latestInsight.aiReflection }}</p>
            </div>
          </div>
        </div>

        <!-- Success Factors -->
        <div v-if="latestInsight.successFactors?.length > 0">
          <h4 class="font-semibold text-gray-900 dark:text-white mb-3">Success Factors</h4>
          <div class="space-y-2">
            <div
              v-for="(factor, idx) in latestInsight.successFactors"
              :key="idx"
              class="flex items-start gap-3 p-3 rounded-lg bg-green-50 dark:bg-green-900/20"
            >
              <UIcon name="i-heroicons-check-circle" class="w-5 h-5 text-green-600 dark:text-green-400 mt-0.5" />
              <p class="text-green-800 dark:text-green-200">{{ factor }}</p>
            </div>
          </div>
        </div>

        <!-- Failure Factors -->
        <div v-if="latestInsight.failureFactors?.length > 0">
          <h4 class="font-semibold text-gray-900 dark:text-white mb-3">Areas for Improvement</h4>
          <div class="space-y-2">
            <div
              v-for="(factor, idx) in latestInsight.failureFactors"
              :key="idx"
              class="flex items-start gap-3 p-3 rounded-lg bg-red-50 dark:bg-red-900/20"
            >
              <UIcon name="i-heroicons-x-circle" class="w-5 h-5 text-red-600 dark:text-red-400 mt-0.5" />
              <p class="text-red-800 dark:text-red-200">{{ factor }}</p>
            </div>
          </div>
        </div>

        <!-- Patterns -->
        <div v-if="latestInsight.patterns?.length > 0">
          <h4 class="font-semibold text-gray-900 dark:text-white mb-3">Discovered Patterns</h4>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div
              v-for="pattern in latestInsight.patterns.slice(0, 6)"
              :key="pattern.pattern"
              class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800"
            >
              <div class="flex items-start justify-between mb-2">
                <p class="font-medium text-gray-900 dark:text-white text-sm">
                  {{ formatPatternName(pattern.pattern) }}
                </p>
                <UBadge
                  :color="getConfidenceColor(pattern.confidence)"
                  variant="subtle"
                  size="sm"
                >
                  {{ pattern.confidence }}
                </UBadge>
              </div>
              <p class="text-xs text-gray-600 dark:text-gray-400 mb-2">
                {{ pattern.description }}
              </p>
              <div class="flex items-center justify-between text-xs">
                <span class="text-gray-500 dark:text-gray-400">
                  {{ pattern.occurrences }} occurrences
                </span>
                <span class="font-semibold" :class="[
                  pattern.successRate >= 0.6 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
                ]">
                  {{ (pattern.successRate * 100).toFixed(0) }}% success
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </UCard>

    <!-- Performance Metrics -->
    <UCard v-if="metrics">
      <template #header>
        <h3 class="text-lg font-semibold">Overall Performance Metrics</h3>
      </template>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div class="space-y-4">
          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
            <p class="text-sm text-gray-500 dark:text-gray-400">Total Trades</p>
            <p class="text-3xl font-bold text-gray-900 dark:text-white mt-1">
              {{ metrics.totalTrades }}
            </p>
          </div>

          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
            <p class="text-sm text-gray-500 dark:text-gray-400">Win Rate</p>
            <p class="text-3xl font-bold text-gray-900 dark:text-white mt-1">
              {{ (metrics.winRate * 100).toFixed(1) }}%
            </p>
            <UProgress :value="metrics.winRate * 100" class="mt-2" />
          </div>

          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
            <p class="text-sm text-gray-500 dark:text-gray-400">Average Return</p>
            <p class="text-3xl font-bold mt-1" :class="[
              metrics.avgReturn >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
            ]">
              {{ (metrics.avgReturn * 100).toFixed(2) }}%
            </p>
          </div>
        </div>

        <div class="space-y-4">
          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
            <p class="text-sm text-gray-500 dark:text-gray-400">Total P&L</p>
            <p class="text-3xl font-bold mt-1" :class="[
              metrics.totalProfitLoss >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
            ]">
              {{ formatCurrency(metrics.totalProfitLoss) }}
            </p>
          </div>

          <div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
            <p class="text-sm text-gray-500 dark:text-gray-400">Avg Holding Period</p>
            <p class="text-3xl font-bold text-gray-900 dark:text-white mt-1">
              {{ metrics.avgHoldingDays.toFixed(1) }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">days</p>
          </div>

          <div class="p-4 rounded-lg bg-green-50 dark:bg-green-900/20">
            <p class="text-sm text-gray-500 dark:text-gray-400">Best Trade</p>
            <p class="text-xl font-bold text-green-600 dark:text-green-400 mt-1">
              {{ metrics.bestTrade.symbol }}
            </p>
            <p class="text-sm text-green-700 dark:text-green-300">
              +{{ metrics.bestTrade.return.toFixed(2) }}%
            </p>
          </div>
        </div>
      </div>
    </UCard>
  </div>
</template>

<script setup>
const api = useApi()
const toast = useToast()

const running = ref(false)
const latestInsight = ref(null)
const metrics = ref(null)

const fetchData = async () => {
  const [insightRes, metricsRes] = await Promise.all([
    api.learning.getLatest(),
    api.learning.getMetrics(),
  ])

  if (insightRes.data) latestInsight.value = insightRes.data
  if (metricsRes.data) metrics.value = metricsRes.data
}

const runLearningReview = async () => {
  running.value = true
  const { data, error } = await api.learning.performReview(7)

  if (data) {
    toast.add({
      title: 'Learning Review Complete',
      description: `Analyzed ${data.tradesReviewed} trades`,
      color: 'green',
    })
    fetchData()
  } else {
    toast.add({
      title: 'Error',
      description: error || 'Failed to perform learning review',
      color: 'red',
    })
  }

  running.value = false
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

const formatPatternName = (pattern) => {
  return pattern.replace(/_/g, ' ').toLowerCase()
    .split(' ')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')
}

const getConfidenceColor = (confidence) => {
  const colors = {
    'HIGH': 'green',
    'MEDIUM': 'yellow',
    'LOW': 'gray',
  }
  return colors[confidence] || 'gray'
}

onMounted(() => {
  fetchData()
  window.addEventListener('dashboard-refresh', fetchData)
})

onUnmounted(() => {
  window.removeEventListener('dashboard-refresh', fetchData)
})
</script>
