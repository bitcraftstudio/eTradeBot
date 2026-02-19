<template>
  <div class="space-y-6">
    <!-- Filters -->
    <UCard>
      <div class="flex flex-wrap gap-4">
        <USelectMenu
          v-model="filter"
          :options="filterOptions"
          placeholder="Filter by status"
          class="w-48"
        />
        <UInput
          v-model="search"
          icon="i-heroicons-magnifying-glass"
          placeholder="Search symbol..."
          class="w-64"
        />
      </div>
    </UCard>

    <!-- Trade History Table -->
    <UCard>
      <template #header>
        <h3 class="text-xl font-bold text-gray-900 dark:text-white">
          Trade History ({{ filteredTrades.length }})
        </h3>
      </template>

      <UTable
        v-if="filteredTrades.length > 0"
        :rows="filteredTrades"
        :columns="columns"
      >
        <template #tradeId-data="{ row }">
          <div>
            <p class="font-medium text-gray-900 dark:text-white">{{ row.tradeId }}</p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              {{ formatDate(row.entryDate) }}
            </p>
          </div>
        </template>

        <template #symbol-data="{ row }">
          <div class="flex items-center gap-2">
            <UAvatar
              :icon="row.type === 'BUY' ? 'i-heroicons-arrow-trending-up' : 'i-heroicons-arrow-trending-down'"
              size="sm"
              :ui="{ 
                background: row.type === 'BUY' ? 'bg-green-100 dark:bg-green-900/20' : 'bg-red-100 dark:bg-red-900/20',
                icon: { 
                  base: row.type === 'BUY' ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
                }
              }"
            />
            <div>
              <p class="font-semibold text-gray-900 dark:text-white">{{ row.symbol }}</p>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                {{ row.quantity }} shares
              </p>
            </div>
          </div>
        </template>

        <template #entry-data="{ row }">
          <div>
            <p class="text-gray-900 dark:text-white">{{ formatCurrency(row.entryPrice) }}</p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              {{ formatCurrency(row.entryPrice * row.quantity) }} total
            </p>
          </div>
        </template>

        <template #exit-data="{ row }">
          <div v-if="row.exitPrice">
            <p class="text-gray-900 dark:text-white">{{ formatCurrency(row.exitPrice) }}</p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              {{ formatDate(row.exitDate) }}
            </p>
          </div>
          <span v-else class="text-gray-400 dark:text-gray-600">-</span>
        </template>

        <template #outcome-data="{ row }">
          <div v-if="row.outcome">
            <p class="font-semibold" :class="[
              row.outcome.profitLoss >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
            ]">
              {{ formatCurrency(row.outcome.profitLoss) }}
            </p>
            <p class="text-sm" :class="[
              row.outcome.profitLoss >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
            ]">
              {{ row.outcome.profitLossPercent.toFixed(2) }}%
            </p>
          </div>
          <span v-else class="text-gray-400 dark:text-gray-600">Open</span>
        </template>

        <template #status-data="{ row }">
          <UBadge
            :color="row.status === 'CLOSED' ? 'gray' : row.status === 'OPEN' ? 'green' : 'yellow'"
            variant="subtle"
          >
            {{ row.status }}
          </UBadge>
        </template>

        <template #actions-data="{ row }">
          <UButton
            icon="i-heroicons-eye"
            color="gray"
            variant="ghost"
            size="sm"
            @click="viewTradeDetails(row)"
          />
        </template>
      </UTable>

      <UAlert
        v-else
        icon="i-heroicons-information-circle"
        color="gray"
        variant="subtle"
        title="No trades found"
        description="Execute trades to see history here"
      />
    </UCard>

    <!-- Trade Details Modal -->
    <UModal v-model="showDetails">
      <UCard v-if="selectedTrade">
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold">Trade Details: {{ selectedTrade.tradeId }}</h3>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-heroicons-x-mark"
              @click="showDetails = false"
            />
          </div>
        </template>

        <div class="space-y-4">
          <!-- Basic Info -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Symbol</p>
              <p class="font-semibold text-gray-900 dark:text-white">{{ selectedTrade.symbol }}</p>
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Type</p>
              <UBadge :color="selectedTrade.type === 'BUY' ? 'green' : 'red'">
                {{ selectedTrade.type }}
              </UBadge>
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Quantity</p>
              <p class="font-semibold text-gray-900 dark:text-white">{{ selectedTrade.quantity }} shares</p>
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Status</p>
              <UBadge :color="selectedTrade.status === 'CLOSED' ? 'gray' : 'green'">
                {{ selectedTrade.status }}
              </UBadge>
            </div>
          </div>

          <UDivider />

          <!-- AI Reasoning -->
          <div v-if="selectedTrade.aiReasoning" class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
            <p class="text-sm font-medium text-blue-900 dark:text-blue-300 mb-2">AI Reasoning</p>
            <p class="text-sm text-blue-800 dark:text-blue-200">{{ selectedTrade.aiReasoning.reasoning }}</p>
            <div class="flex items-center gap-4 mt-3">
              <div>
                <span class="text-xs text-blue-700 dark:text-blue-300">Confidence: </span>
                <span class="font-semibold text-blue-900 dark:text-blue-100">
                  {{ (selectedTrade.aiReasoning.confidence * 100).toFixed(0) }}%
                </span>
              </div>
              <div>
                <span class="text-xs text-blue-700 dark:text-blue-300">Risk: </span>
                <UBadge :color="getRiskColor(selectedTrade.aiReasoning.riskAssessment)" variant="subtle">
                  {{ selectedTrade.aiReasoning.riskAssessment }}
                </UBadge>
              </div>
            </div>
          </div>

          <!-- Outcome -->
          <div v-if="selectedTrade.outcome">
            <p class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Outcome</p>
            <div class="grid grid-cols-2 gap-4">
              <div class="p-3 rounded-lg bg-gray-50 dark:bg-gray-800/50">
                <p class="text-xs text-gray-500 dark:text-gray-400">Profit/Loss</p>
                <p class="text-lg font-bold" :class="[
                  selectedTrade.outcome.profitLoss >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
                ]">
                  {{ formatCurrency(selectedTrade.outcome.profitLoss) }}
                </p>
              </div>
              <div class="p-3 rounded-lg bg-gray-50 dark:bg-gray-800/50">
                <p class="text-xs text-gray-500 dark:text-gray-400">Holding Days</p>
                <p class="text-lg font-bold text-gray-900 dark:text-white">
                  {{ selectedTrade.outcome.holdingDays }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </UCard>
    </UModal>
  </div>
</template>

<script setup>
import { format } from 'date-fns'

const api = useApi()

const loading = ref(false)
const trades = ref([])
const filter = ref('all')
const search = ref('')
const showDetails = ref(false)
const selectedTrade = ref(null)

const filterOptions = [
  { label: 'All Trades', value: 'all' },
  { label: 'Open', value: 'OPEN' },
  { label: 'Closed', value: 'CLOSED' },
]

const columns = [
  { key: 'tradeId', label: 'Trade ID' },
  { key: 'symbol', label: 'Symbol' },
  { key: 'entry', label: 'Entry' },
  { key: 'exit', label: 'Exit' },
  { key: 'outcome', label: 'P&L' },
  { key: 'status', label: 'Status' },
  { key: 'actions', label: '' },
]

const filteredTrades = computed(() => {
  let result = trades.value

  // Filter by status
  if (filter.value !== 'all') {
    result = result.filter(t => t.status === filter.value)
  }

  // Search by symbol
  if (search.value) {
    result = result.filter(t =>
      t.symbol.toLowerCase().includes(search.value.toLowerCase())
    )
  }

  return result
})

const fetchTrades = async () => {
  loading.value = true
  const { data } = await api.trades.getAll()
  if (data) trades.value = data
  loading.value = false
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

const viewTradeDetails = (trade) => {
  selectedTrade.value = trade
  showDetails.value = true
}

const getRiskColor = (risk) => {
  const colors = {
    'LOW': 'green',
    'MODERATE': 'yellow',
    'HIGH': 'orange',
    'VERY_HIGH': 'red',
  }
  return colors[risk] || 'gray'
}

onMounted(() => {
  fetchTrades()
  window.addEventListener('dashboard-refresh', fetchTrades)
})

onUnmounted(() => {
  window.removeEventListener('dashboard-refresh', fetchTrades)
})
</script>
