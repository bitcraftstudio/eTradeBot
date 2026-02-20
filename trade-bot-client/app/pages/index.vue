<template>
	<UDashboardPanel id="home">
		<template #header>
			<Topbar :title="'Dashboard'" :description="'Overview of your trading performance'" />
		</template>
		<template #body>
			<div class="space-y-6">
				<!-- Stats Cards -->
				<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
					<StatCard title="Total Value" :value="portfolio?.totalValue || 0"
						:subtitle="`Cash: ${formatCurrency(portfolio?.cashBalance || 0)}`" icon="i-heroicons-banknotes-20-solid"
						icon-color="text-blue-500" />

					<StatCard title="Total P&L" :value="portfolio?.totalProfitLoss || 0"
						:subtitle="`${(portfolio?.totalProfitLossPercent || 0).toFixed(2)}%`" icon="i-heroicons-chart-bar-20-solid"
						icon-color="text-green-500"
						:value-color="(portfolio?.totalProfitLoss || 0) >= 0 ? 'positive' : 'negative'" />

					<StatCard title="Open Positions" :value="portfolio?.openPositions || 0"
						:subtitle="`Value: ${formatCurrency(portfolio?.positionValue || 0)}`" icon="i-heroicons-briefcase-20-solid"
						icon-color="text-purple-500" format="number" />

					<StatCard title="Win Rate" :value="(metrics?.winRate || 0) * 100"
						:subtitle="`${metrics?.totalTrades || 0} trades`" icon="i-heroicons-trophy-20-solid"
						icon-color="text-yellow-500" format="percentage" />
				</div>
				<!-- Recent Trades & Active Positions -->
				<div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
					<!-- Recent Trades -->
					<UCard>
						<template #header>
							<div class="flex items-center justify-between">
								<h3 class="text-lg font-semibold text-gray-900 dark:text-white">Recent Trades</h3>
								<UButton to="/trades" color="neutral" variant="ghost" trailing-icon="i-heroicons-arrow-right-20-solid"
									size="sm">
									View All
								</UButton>
							</div>
						</template>

						<div class="space-y-3">
							<div v-for="trade in recentTrades" :key="trade._id"
								class="flex items-center justify-between p-3 rounded-lg bg-gray-50 dark:bg-gray-800/50">
								<div class="flex items-center gap-3">
									<UAvatar
										:icon="trade.type === 'BUY' ? 'i-heroicons-arrow-trending-up' : 'i-heroicons-arrow-trending-down'"
										size="md" :ui="{
											background: trade.type === 'BUY' ? 'bg-green-100 dark:bg-green-900/20' : 'bg-red-100 dark:bg-red-900/20',
											icon: {
												base: trade.type === 'BUY' ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
											}
										}" />
									<div>
										<p class="font-medium text-gray-900 dark:text-white">{{ trade.symbol }}</p>
										<p class="text-sm text-gray-500 dark:text-gray-400">
											{{ trade.quantity }} shares @ {{ formatCurrency(trade.entryPrice) }}
										</p>
									</div>
								</div>
								<div class="text-right">
									<UBadge :color="trade.status === 'CLOSED' ? 'gray' : 'green'" variant="subtle">
										{{ trade.status }}
									</UBadge>
									<p v-if="trade.outcome" class="text-sm font-medium mt-1" :class="[
										trade.outcome.profitLoss >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
									]">
										{{ formatCurrency(trade.outcome.profitLoss) }}
									</p>
								</div>
							</div>

							<UAlert v-if="!recentTrades || recentTrades.length === 0" icon="i-heroicons-information-circle"
								color="gray" variant="subtle" title="No trades yet" description="Start trading to see results here!" />
						</div>
					</UCard>

					<!-- Active Positions -->
					<UCard>
						<template #header>
							<div class="flex items-center justify-between">
								<h3 class="text-lg font-semibold text-gray-900 dark:text-white">Active Positions</h3>
								<UButton to="/positions" color="neutral" variant="ghost"
									trailing-icon="i-heroicons-arrow-right-20-solid" size="sm">
									View All
								</UButton>
							</div>
						</template>

						<div class="space-y-3">
							<div v-for="position in activePositions" :key="position._id"
								class="flex items-center justify-between p-3 rounded-lg bg-gray-50 dark:bg-gray-800/50">
								<div>
									<p class="font-medium text-gray-900 dark:text-white">{{ position.symbol }}</p>
									<p class="text-sm text-gray-500 dark:text-gray-400">
										{{ position.quantity }} shares @ {{ formatCurrency(position.entryPrice) }}
									</p>
								</div>
								<div class="text-right">
									<p class="font-semibold" :class="[
										position.unrealizedPnL >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
									]">
										{{ formatCurrency(position.unrealizedPnL) }}
									</p>
									<p class="text-sm text-gray-500 dark:text-gray-400">
										{{ position.unrealizedPnLPercent.toFixed(2) }}%
									</p>
								</div>
							</div>

							<UAlert v-if="!activePositions || activePositions.length === 0" icon="i-heroicons-information-circle"
								color="neutral" variant="subtle" title="No active positions"
								description="Execute trades to see positions here" />
						</div>
					</UCard>
				</div>

				<!-- Learning Insights -->
				<UCard v-if="latestInsight">
					<template #header>
						<div class="flex items-center justify-between">
							<div class="flex items-center gap-2">
								<UIcon name="i-heroicons-sparkles-20-solid" class="w-5 h-5 text-primary-500" />
								<h3 class="text-lg font-semibold text-gray-900 dark:text-white">Latest Learning Insights</h3>
							</div>
							<UButton to="/learning" color="neutral" variant="ghost" trailing-icon="i-heroicons-arrow-right-20-solid"
								size="sm">
								View Details
							</UButton>
						</div>
					</template>

					<div class="space-y-4">
						<div class="grid grid-cols-3 gap-4">
							<div class="text-center p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50">
								<p class="text-sm text-gray-500 dark:text-gray-400">Trades Reviewed</p>
								<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">{{ latestInsight.tradesReviewed }}</p>
							</div>
							<div class="text-center p-4 rounded-lg bg-green-50 dark:bg-green-900/20">
								<p class="text-sm text-gray-500 dark:text-gray-400">Win Rate</p>
								<p class="text-2xl font-bold text-green-600 dark:text-green-400 mt-1">
									{{ ((latestInsight.winRate || 0) * 100).toFixed(1) }}%
								</p>
							</div>
							<div class="text-center p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
								<p class="text-sm text-gray-500 dark:text-gray-400">Avg Return</p>
								<p class="text-2xl font-bold text-blue-600 dark:text-blue-400 mt-1">
									{{ ((latestInsight.avgReturn || 0) * 100).toFixed(2) }}%
								</p>
							</div>
						</div>

						<UAlert v-if="latestInsight.aiReflection" icon="i-heroicons-light-bulb" color="primary" variant="subtle"
							:title="'AI Reflection'" :description="latestInsight.aiReflection" />
					</div>
				</UCard>
			</div>
		</template>
	</UDashboardPanel>
</template>

<script setup lang="ts">
const api = useApi()

// Data
const portfolio = ref()
const recentTrades = ref([])
const activePositions = ref([])
const metrics = ref()
const latestInsight = ref()

// Fetch data
const fetchData = async () => {
	const [portfolioRes, tradesRes, positionsRes, metricsRes, insightRes] = await Promise.all([
		api.portfolio.getSummary(),
		api.trades.getAll(),
		api.positions.getAll(),
		api.learning.getMetrics(),
		api.learning.getLatest(),
	])

	if (portfolioRes.data) portfolio.value = portfolioRes.data
	if (tradesRes.data) recentTrades.value = tradesRes.data.slice(0, 5)
	if (positionsRes.data) activePositions.value = positionsRes.data
	if (metricsRes.data) metrics.value = metricsRes.data
	if (insightRes.data) latestInsight.value = insightRes.data
}

// Format currency
const formatCurrency = (value: any) => {
	return new Intl.NumberFormat('en-US', {
		style: 'currency',
		currency: 'USD',
	}).format(value)
}

// Listen for refresh event
onMounted(() => {
	fetchData()
	window.addEventListener('dashboard-refresh', fetchData)
})

onUnmounted(() => {
	window.removeEventListener('dashboard-refresh', fetchData)
})
</script>