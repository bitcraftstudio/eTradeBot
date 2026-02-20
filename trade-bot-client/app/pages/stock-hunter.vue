<template>
	<UDashboardPanel id="stock-hunter">
		<template #header>
			<Topbar :title="'Stock Hunter'" :description="'Discover winning stocks'" />
		</template>
		<template #body>
			<div class="space-y-6">
				<!-- Hunter Status & Controls -->
				<UCard>
					<template #header>
						<div class="flex items-center justify-between">
							<div class="flex items-center gap-3">
								<UIcon name="i-heroicons-magnifying-glass-circle" class="w-6 h-6 text-primary-500" />
								<div>
									<h3 class="text-xl font-bold text-gray-900 dark:text-white">Stock Hunter</h3>
									<p class="text-sm text-gray-500 dark:text-gray-400">Discover winning stocks across the market</p>
								</div>
							</div>
							<div class="flex items-center gap-2">
								<UBadge :color="status?.apiConfigured ? 'green' : 'yellow'" variant="subtle">
									{{ status?.apiConfigured ? 'TipRanks Connected' : 'Demo Mode' }}
								</UBadge>
								<UButton icon="i-heroicons-arrow-path" color="neutral" variant="ghost" :loading="loading"
									@click="fetchStatus" />
							</div>
						</div>
					</template>

					<div class="grid grid-cols-1 md:grid-cols-4 gap-4">
						<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800/50">
							<p class="text-sm text-gray-500 dark:text-gray-400">Total Found</p>
							<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
								{{ huntResults?.totalFound || 0 }}
							</p>
						</div>

						<div class="p-4 rounded-lg bg-green-50 dark:bg-green-900/20">
							<p class="text-sm text-gray-500 dark:text-gray-400">After Filters</p>
							<p class="text-2xl font-bold text-green-600 dark:text-green-400 mt-1">
								{{ huntResults?.filtered || 0 }}
							</p>
						</div>

						<div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
							<p class="text-sm text-gray-500 dark:text-gray-400">Avg Smart Score</p>
							<p class="text-2xl font-bold text-blue-600 dark:text-blue-400 mt-1">
								{{ huntResults?.summary?.avgSmartScore?.toFixed(1) || '-' }}
							</p>
						</div>

						<div class="p-4 rounded-lg bg-purple-50 dark:bg-purple-900/20">
							<p class="text-sm text-gray-500 dark:text-gray-400">Avg Upside</p>
							<p class="text-2xl font-bold text-purple-600 dark:text-purple-400 mt-1">
								{{ huntResults?.summary?.avgUpside ? `${huntResults.summary.avgUpside.toFixed(1)}%` : '-' }}
							</p>
						</div>
					</div>
				</UCard>

				<!-- Filter Controls -->
				<UCard>
					<template #header>
						<div class="flex items-center justify-between">
							<h3 class="text-lg font-semibold text-gray-900 dark:text-white">Hunt Filters</h3>
							<div class="flex gap-2">
								<UButton color="neutral" variant="ghost" size="sm" @click="resetFilters">
									Reset
								</UButton>
								<UButton :loading="hunting" @click="runHunt">
									<UIcon name="i-heroicons-magnifying-glass" class="mr-2" />
									Run Hunt
								</UButton>
							</div>
						</div>
					</template>

					<div class="space-y-6">
						<!-- Smart Score Range -->
						<div>
							<div class="flex items-center justify-between mb-3">
								<label class="text-sm font-medium text-gray-700 dark:text-gray-300">
									Smart Score Range
								</label>
								<span class="text-sm font-semibold text-primary-600 dark:text-primary-400">
									{{ filters.minSmartScore }} - {{ filters.maxSmartScore }}
								</span>
							</div>
							<div class="grid grid-cols-2 gap-4">
								<UFormGroup label="Minimum">
									<UInput v-model.number="filters.minSmartScore" type="number" min="1" max="10" />
								</UFormGroup>
								<UFormGroup label="Maximum">
									<UInput v-model.number="filters.maxSmartScore" type="number" min="1" max="10" />
								</UFormGroup>
							</div>
							<div class="mt-2 flex gap-2">
								<UBadge @click="filters.minSmartScore = 10; filters.maxSmartScore = 10" class="cursor-pointer"
									color="green">
									Perfect (10)</UBadge>
								<UBadge @click="filters.minSmartScore = 9; filters.maxSmartScore = 10" class="cursor-pointer"
									color="primary">
									Excellent (9-10)</UBadge>
								<UBadge @click="filters.minSmartScore = 8; filters.maxSmartScore = 10" class="cursor-pointer"
									color="blue">Good+
									(8-10)</UBadge>
							</div>
						</div>

						<!-- Upside & Market Cap -->
						<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
							<UFormGroup label="Minimum Upside (%)" hint="Minimum % to analyst price target">
								<UInput v-model.number="filters.minUpside" type="number" min="0" max="100" placeholder="5" />
							</UFormGroup>

							<UFormGroup label="Minimum Market Cap ($B)" hint="Minimum company size">
								<UInput v-model.number="filters.minMarketCapB" type="number" min="0" placeholder="1" />
							</UFormGroup>
						</div>

						<!-- Smart Money Filters -->
						<div class="space-y-3">
							<h4 class="text-sm font-medium text-gray-700 dark:text-gray-300">Smart Money Filters</h4>
							<div class="grid grid-cols-1 md:grid-cols-2 gap-4">
								<div class="flex items-center justify-between p-3 rounded-lg bg-gray-50 dark:bg-gray-800">
									<div class="flex items-center gap-2">
										<UIcon name="i-heroicons-building-office" class="w-5 h-5 text-gray-500" />
										<span class="text-sm text-gray-900 dark:text-white">Hedge Fund Activity</span>
									</div>
									<UToggle v-model="filters.requiredHedgeFundActivity" />
								</div>

								<div class="flex items-center justify-between p-3 rounded-lg bg-gray-50 dark:bg-gray-800">
									<div class="flex items-center gap-2">
										<UIcon name="i-heroicons-user-group" class="w-5 h-5 text-gray-500" />
										<span class="text-sm text-gray-900 dark:text-white">Insider Buying</span>
									</div>
									<UToggle v-model="filters.requiredInsiderBuying" />
								</div>
							</div>
						</div>

						<!-- Advanced Filters -->
						<div class="space-y-3">
							<h4 class="text-sm font-medium text-gray-700 dark:text-gray-300">Advanced Filters</h4>
							<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
								<UFormGroup label="Min Analyst Rating" hint="1-5 scale (4 = Buy)">
									<UInput v-model.number="filters.minAnalystRating" type="number" min="1" max="5" step="0.1"
										placeholder="4.0" />
								</UFormGroup>

								<UFormGroup label="Max Results" hint="Maximum discoveries">
									<UInput v-model.number="filters.limit" type="number" min="1" max="100" placeholder="50" />
								</UFormGroup>
							</div>
						</div>
					</div>
				</UCard>

				<!-- Hunt Results -->
				<div v-if="huntResults?.recommendations?.length > 0" class="space-y-4">
					<!-- Top Pick Banner -->
					<UCard v-if="huntResults.summary?.topPick" class="border-2 border-primary-500">
						<template #header>
							<div class="flex items-center gap-2">
								<UIcon name="i-heroicons-star" class="w-6 h-6 text-yellow-500" />
								<h3 class="text-lg font-semibold text-gray-900 dark:text-white">Top Pick</h3>
							</div>
						</template>

						<div class="grid grid-cols-1 md:grid-cols-3 gap-6">
							<div>
								<p class="text-sm text-gray-500 dark:text-gray-400">Symbol</p>
								<p class="text-3xl font-bold text-gray-900 dark:text-white mt-1">
									{{ huntResults.summary.topPick.symbol }}
								</p>
								<p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
									{{ huntResults.summary.topPick.name }}
								</p>
							</div>

							<div>
								<p class="text-sm text-gray-500 dark:text-gray-400">Smart Score</p>
								<div class="flex items-center gap-2 mt-1">
									<p class="text-3xl font-bold text-primary-600 dark:text-primary-400">
										{{ huntResults.summary.topPick.smartScore }}
									</p>
									<span class="text-lg text-gray-500">/10</span>
								</div>
								<UProgress :value="huntResults.summary.topPick.smartScore * 10" class="mt-2" />
							</div>

							<div>
								<p class="text-sm text-gray-500 dark:text-gray-400">Upside Potential</p>
								<p class="text-3xl font-bold text-green-600 dark:text-green-400 mt-1">
									{{ huntResults.summary.topPick.upside.toFixed(1) }}%
								</p>
								<p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
									Target: {{ formatCurrency(huntResults.summary.topPick.priceTarget) }}
								</p>
							</div>
						</div>

						<div class="mt-4 flex gap-2">
							<UButton color="primary" @click="addToWatchlist(huntResults.summary.topPick.symbol)">
								Add to Watchlist
							</UButton>
							<UButton color="neutral" variant="outline" @click="viewDetails(huntResults.summary.topPick)">
								View Details
							</UButton>
						</div>
					</UCard>

					<!-- All Discoveries -->
					<UCard>
						<template #header>
							<h3 class="text-lg font-semibold text-gray-900 dark:text-white">
								Discovered Stocks ({{ huntResults.recommendations.length }})
							</h3>
						</template>

						<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
							<div v-for="stock in huntResults.recommendations" :key="stock.symbol"
								class="p-4 rounded-lg border border-gray-200 dark:border-gray-700 hover:border-primary-500 dark:hover:border-primary-500 transition-colors cursor-pointer"
								@click="viewDetails(stock)">
								<!-- Header -->
								<div class="flex items-start justify-between mb-3">
									<div>
										<p class="text-lg font-bold text-gray-900 dark:text-white">{{ stock.symbol }}</p>
										<p class="text-sm text-gray-500 dark:text-gray-400 truncate">{{ stock.name }}</p>
									</div>
									<UBadge :color="getSmartScoreColor(stock.smartScore)" size="lg">
										{{ stock.smartScore }}
									</UBadge>
								</div>

								<!-- Metrics -->
								<div class="space-y-2 mb-3">
									<div class="flex justify-between text-sm">
										<span class="text-gray-600 dark:text-gray-400">Price:</span>
										<span class="font-semibold text-gray-900 dark:text-white">
											{{ formatCurrency(stock.currentPrice) }}
										</span>
									</div>
									<div class="flex justify-between text-sm">
										<span class="text-gray-600 dark:text-gray-400">Target:</span>
										<span class="font-semibold text-gray-900 dark:text-white">
											{{ formatCurrency(stock.priceTarget) }}
										</span>
									</div>
									<div class="flex justify-between text-sm">
										<span class="text-gray-600 dark:text-gray-400">Upside:</span>
										<span class="font-semibold text-green-600 dark:text-green-400">
											+{{ stock.upside.toFixed(1) }}%
										</span>
									</div>
								</div>

								<!-- Signals -->
								<div class="flex flex-wrap gap-1 mb-3">
									<UBadge v-if="stock.hedgeFundTrend === 'Increasing'" color="blue" variant="subtle" size="xs">
										🏦 HF Buying
									</UBadge>
									<UBadge v-if="stock.insiderSentiment === 'Positive'" color="purple" variant="subtle" size="xs">
										👔 Insider Buy
									</UBadge>
									<UBadge v-if="stock.analystConsensus.buy > 20" color="primary" variant="subtle" size="xs">
										📊 {{ stock.analystConsensus.buy }} Buys
									</UBadge>
								</div>

								<!-- Action -->
								<UButton block size="sm" variant="outline" @click.stop="addToWatchlist(stock.symbol)">
									Add to Watchlist
								</UButton>
							</div>
						</div>
					</UCard>
				</div>

				<!-- Empty State -->
				<UAlert v-else-if="!hunting && !huntResults" icon="i-heroicons-information-circle"
					title="Ready to hunt for stocks"
					description="Adjust the filters above and click 'Run Hunt' to discover winning stocks!" />

				<!-- Stock Details Modal -->
				<UModal v-model="showDetails">
					<UCard v-if="selectedStock">
						<template #header>
							<div class="flex items-center justify-between">
								<div>
									<h3 class="text-xl font-bold text-gray-900 dark:text-white">{{ selectedStock.symbol }}</h3>
									<p class="text-sm text-gray-500 dark:text-gray-400">{{ selectedStock.name }}</p>
								</div>
								<UButton color="neutral" variant="ghost" icon="i-heroicons-x-mark" @click="showDetails = false" />
							</div>
						</template>

						<div class="space-y-6">
							<!-- Key Metrics -->
							<div class="grid grid-cols-2 gap-4">
								<div class="p-4 rounded-lg bg-primary-50 dark:bg-primary-900/20">
									<p class="text-sm text-gray-600 dark:text-gray-400">Smart Score</p>
									<p class="text-3xl font-bold text-primary-600 dark:text-primary-400 mt-1">
										{{ selectedStock.smartScore }}/10
									</p>
									<UProgress :value="selectedStock.smartScore * 10" class="mt-2" />
								</div>

								<div class="p-4 rounded-lg bg-green-50 dark:bg-green-900/20">
									<p class="text-sm text-gray-600 dark:text-gray-400">Upside</p>
									<p class="text-3xl font-bold text-green-600 dark:text-green-400 mt-1">
										{{ selectedStock.upside.toFixed(1) }}%
									</p>
									<p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
										to {{ formatCurrency(selectedStock.priceTarget) }}
									</p>
								</div>
							</div>

							<!-- Analyst Consensus -->
							<div>
								<h4 class="font-semibold text-gray-900 dark:text-white mb-3">Analyst Consensus</h4>
								<div class="grid grid-cols-3 gap-3">
									<div class="text-center p-3 rounded-lg bg-green-50 dark:bg-green-900/20">
										<p class="text-2xl font-bold text-green-600 dark:text-green-400">
											{{ selectedStock.analystConsensus.buy }}
										</p>
										<p class="text-xs text-gray-600 dark:text-gray-400">Buy</p>
									</div>
									<div class="text-center p-3 rounded-lg bg-yellow-50 dark:bg-yellow-900/20">
										<p class="text-2xl font-bold text-yellow-600 dark:text-yellow-400">
											{{ selectedStock.analystConsensus.hold }}
										</p>
										<p class="text-xs text-gray-600 dark:text-gray-400">Hold</p>
									</div>
									<div class="text-center p-3 rounded-lg bg-red-50 dark:bg-red-900/20">
										<p class="text-2xl font-bold text-red-600 dark:text-red-400">
											{{ selectedStock.analystConsensus.sell }}
										</p>
										<p class="text-xs text-gray-600 dark:text-gray-400">Sell</p>
									</div>
								</div>
							</div>

							<!-- Smart Money -->
							<div>
								<h4 class="font-semibold text-gray-900 dark:text-white mb-3">Smart Money Activity</h4>
								<div class="space-y-2">
									<div class="flex items-center justify-between p-3 rounded-lg bg-gray-50 dark:bg-gray-800">
										<span class="text-sm text-gray-700 dark:text-gray-300">Hedge Fund Trend</span>
										<UBadge :color="getHedgeFundColor(selectedStock.hedgeFundTrend)">
											{{ selectedStock.hedgeFundTrend }}
										</UBadge>
									</div>
									<div class="flex items-center justify-between p-3 rounded-lg bg-gray-50 dark:bg-gray-800">
										<span class="text-sm text-gray-700 dark:text-gray-300">Insider Sentiment</span>
										<UBadge :color="getInsiderColor(selectedStock.insiderSentiment)">
											{{ selectedStock.insiderSentiment }}
										</UBadge>
									</div>
								</div>
							</div>

							<!-- Reasons -->
							<div v-if="selectedStock.reasons?.length > 0">
								<h4 class="font-semibold text-gray-900 dark:text-white mb-3">Why This Stock?</h4>
								<div class="space-y-2">
									<div v-for="(reason, idx) in selectedStock.reasons" :key="idx"
										class="flex items-start gap-2 p-3 rounded-lg bg-blue-50 dark:bg-blue-900/20">
										<UIcon name="i-heroicons-check-circle"
											class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0" />
										<p class="text-sm text-blue-800 dark:text-blue-200">{{ reason }}</p>
									</div>
								</div>
							</div>

							<!-- Actions -->
							<div class="flex gap-2">
								<UButton block @click="addToWatchlist(selectedStock.symbol)">
									Add to Watchlist
								</UButton>
								<UButton block color="neutral" variant="outline" @click="showDetails = false">
									Close
								</UButton>
							</div>
						</div>
					</UCard>
				</UModal>
			</div>
		</template>
	</UDashboardPanel>
</template>

<script setup lang="ts">
const api = useApi()
const toast = useToast()

const loading = ref(false)
const hunting = ref(false)
const status = ref(null)
const huntResults = ref(null)
const showDetails = ref(false)
const selectedStock = ref(null)

const filters = ref({
	minSmartScore: 8,
	maxSmartScore: 10,
	minUpside: 5,
	minMarketCapB: 1,
	minAnalystRating: 4.0,
	requiredHedgeFundActivity: false,
	requiredInsiderBuying: false,
	limit: 50,
})

// Fetch hunter status
const fetchStatus = async () => {
	loading.value = true
	const { data } = await api.stockHunter.getStatus()
	if (data) status.value = data
	loading.value = false
}

// Run hunt
const runHunt = async () => {
	hunting.value = true
	huntResults.value = null

	const huntFilters = {
		minSmartScore: filters.value.minSmartScore,
		maxSmartScore: filters.value.maxSmartScore,
		minUpside: filters.value.minUpside,
		minMarketCap: filters.value.minMarketCapB * 1000000000,
		minAnalystRating: filters.value.minAnalystRating,
		requiredHedgeFundActivity: filters.value.requiredHedgeFundActivity,
		requiredInsiderBuying: filters.value.requiredInsiderBuying,
		limit: filters.value.limit,
	}

	const { data, error } = await api.stockHunter.hunt(huntFilters)

	if (data) {
		huntResults.value = data
		toast.add({
			title: 'Hunt Complete',
			description: `Found ${data.filtered} stocks matching criteria`,
			color: 'green',
		})
	} else {
		toast.add({
			title: 'Hunt Failed',
			description: error || 'Failed to complete hunt',
			color: 'red',
		})
	}

	hunting.value = false
}

// Reset filters
const resetFilters = () => {
	filters.value = {
		minSmartScore: 8,
		maxSmartScore: 10,
		minUpside: 5,
		minMarketCapB: 1,
		minAnalystRating: 4.0,
		requiredHedgeFundActivity: false,
		requiredInsiderBuying: false,
		limit: 50,
	}
}

// View stock details
const viewDetails = (stock) => {
	selectedStock.value = stock
	showDetails.value = true
}

// Add to watchlist
const addToWatchlist = async (symbol) => {
	// Get current scheduler config
	const { data: schedulerStatus } = await api.scheduler.getStatus()

	if (schedulerStatus) {
		const currentWatchlist = schedulerStatus.config.watchlist || []

		if (currentWatchlist.includes(symbol)) {
			toast.add({
				title: 'Already in Watchlist',
				description: `${symbol} is already in your watchlist`,
				color: 'yellow',
			})
			return
		}

		const newWatchlist = [...currentWatchlist, symbol]

		const { error } = await api.scheduler.updateConfig({
			watchlist: newWatchlist,
		})

		if (!error) {
			toast.add({
				title: 'Added to Watchlist',
				description: `${symbol} has been added to your scheduler watchlist`,
				color: 'green',
			})
		} else {
			toast.add({
				title: 'Error',
				description: 'Failed to add to watchlist',
				color: 'red',
			})
		}
	}
}

// Format currency
const formatCurrency = (value) => {
	return new Intl.NumberFormat('en-US', {
		style: 'currency',
		currency: 'USD',
	}).format(value)
}

// Get smart score color
const getSmartScoreColor = (score) => {
	if (score >= 9) return 'green'
	if (score >= 8) return 'primary'
	if (score >= 7) return 'yellow'
	return 'gray'
}

// Get hedge fund color
const getHedgeFundColor = (trend) => {
	if (trend === 'Increasing') return 'green'
	if (trend === 'Decreasing') return 'red'
	return 'gray'
}

// Get insider color
const getInsiderColor = (sentiment) => {
	if (sentiment === 'Positive') return 'green'
	if (sentiment === 'Negative') return 'red'
	return 'gray'
}

// Fetch data on mount
onMounted(() => {
	fetchStatus()
})
</script>