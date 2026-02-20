<template>
	<UDashboardPanel id="positions">
		<template #header>
			<Topbar :title="'Positions'" :description="'Active positions and monitoring'" />
		</template>
		<template #body>
			<div class="space-y-6">
				<!-- Header Stats -->
				<div class="grid grid-cols-1 md:grid-cols-4 gap-4">
					<UCard>
						<div class="flex items-center justify-between">
							<div>
								<p class="text-sm text-gray-500 dark:text-gray-400">Open Positions</p>
								<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
									{{ positions.length }}
								</p>
							</div>
							<UIcon name="i-heroicons-briefcase" class="w-8 h-8 text-blue-500" />
						</div>
					</UCard>

					<UCard>
						<div class="flex items-center justify-between">
							<div>
								<p class="text-sm text-gray-500 dark:text-gray-400">Sell Signals</p>
								<p class="text-2xl font-bold text-orange-600 dark:text-orange-400 mt-1">
									{{ sellSignals?.totalSignals || 0 }}
								</p>
							</div>
							<UIcon name="i-heroicons-bell-alert" class="w-8 h-8 text-orange-500" />
						</div>
					</UCard>

					<UCard>
						<div class="flex items-center justify-between">
							<div>
								<p class="text-sm text-gray-500 dark:text-gray-400">Total P&L</p>
								<p :class="[
									'text-2xl font-bold mt-1',
									totalPnL >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
								]">
									{{ formatCurrency(totalPnL) }}
								</p>
							</div>
							<UIcon :name="totalPnL >= 0 ? 'i-heroicons-arrow-trending-up' : 'i-heroicons-arrow-trending-down'"
								:class="totalPnL >= 0 ? 'text-green-500' : 'text-red-500'" class="w-8 h-8" />
						</div>
					</UCard>

					<UCard>
						<div class="flex items-center justify-between">
							<div>
								<p class="text-sm text-gray-500 dark:text-gray-400">Avg Gain</p>
								<p :class="[
									'text-2xl font-bold mt-1',
									avgGain >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
								]">
									{{ avgGain >= 0 ? '+' : '' }}{{ avgGain.toFixed(2) }}%
								</p>
							</div>
							<UIcon name="i-heroicons-chart-bar" class="w-8 h-8 text-purple-500" />
						</div>
					</UCard>
				</div>

				<!-- Actions Bar -->
				<UCard>
					<div class="flex items-center justify-between">
						<div class="flex items-center gap-3">
							<UIcon name="i-heroicons-cog-6-tooth" class="w-5 h-5 text-gray-500" />
							<span class="text-sm font-medium text-gray-700 dark:text-gray-300">Position
								Monitoring</span>
						</div>
						<div class="flex gap-2">
							<UButton color="gray" variant="outline" :loading="updating" @click="updatePositions">
								<UIcon name="i-heroicons-arrow-path" class="mr-2" />
								Update Prices
							</UButton>
							<UButton color="primary" :loading="checkingSignals" @click="checkSellSignals">
								<UIcon name="i-heroicons-magnifying-glass" class="mr-2" />
								Check Sell Signals
							</UButton>
						</div>
					</div>
				</UCard>

				<!-- Sell Signals Alert -->
				<UAlert v-if="sellSignals && sellSignals.totalSignals > 0" icon="i-heroicons-bell-alert" color="orange"
					variant="soft" title="Sell Signals Detected"
					:description="`${sellSignals.totalSignals} position(s) meet sell criteria. Review recommendations below.`" />

				<!-- Positions List -->
				<div v-if="positions.length > 0" class="space-y-4">
					<div v-for="position in positions" :key="position.symbol" class="relative">
						<UCard>
							<!-- Sell Signal Banner -->
							<div v-if="getSignalForPosition(position.symbol)"
								class="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-orange-500 to-red-500" />

							<div class="space-y-4">
								<!-- Header -->
								<div class="flex items-start justify-between">
									<div>
										<div class="flex items-center gap-3">
											<h3 class="text-xl font-bold text-gray-900 dark:text-white">
												{{ position.symbol }}
											</h3>
											<UBadge v-if="getSignalForPosition(position.symbol)" color="orange" size="lg">
												SELL SIGNAL
											</UBadge>
											<UBadge v-else :color="position.unrealizedPnLPercent >= 5 ? 'green' : 'gray'" size="lg">
												{{ position.unrealizedPnLPercent >= 5 ? 'AT TARGET' : 'HOLDING' }}
											</UBadge>
										</div>
										<p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
											{{ position.quantity }} shares · Held {{ position.daysHeld }} days
										</p>
									</div>

									<div class="text-right">
										<p class="text-sm text-gray-500 dark:text-gray-400">Unrealized P&L</p>
										<p :class="[
											'text-2xl font-bold',
											position.unrealizedPnL >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
										]">
											{{ formatCurrency(position.unrealizedPnL) }}
										</p>
										<p :class="[
											'text-sm font-semibold',
											position.unrealizedPnLPercent >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
										]">
											{{ position.unrealizedPnLPercent >= 0 ? '+' : '' }}{{
												position.unrealizedPnLPercent.toFixed(2) }}%
										</p>
									</div>
								</div>

								<!-- Price Info -->
								<div class="grid grid-cols-2 md:grid-cols-4 gap-4">
									<div>
										<p class="text-xs text-gray-500 dark:text-gray-400">Entry Price</p>
										<p class="text-lg font-semibold text-gray-900 dark:text-white">
											{{ formatCurrency(position.entryPrice) }}
										</p>
									</div>
									<div>
										<p class="text-xs text-gray-500 dark:text-gray-400">Current Price</p>
										<p class="text-lg font-semibold text-gray-900 dark:text-white">
											{{ formatCurrency(position.currentPrice) }}
										</p>
									</div>
									<div>
										<p class="text-xs text-gray-500 dark:text-gray-400">Peak Price</p>
										<p class="text-lg font-semibold text-blue-600 dark:text-blue-400">
											{{ formatCurrency(position.peakPrice || position.currentPrice) }}
										</p>
									</div>
									<div>
										<p class="text-xs text-gray-500 dark:text-gray-400">Stop Loss</p>
										<p class="text-lg font-semibold text-red-600 dark:text-red-400">
											{{ formatCurrency(position.stopLoss) }}
										</p>
									</div>
								</div>

								<!-- Sell Signal Details -->
								<div v-if="getSignalForPosition(position.symbol)"
									class="p-4 rounded-lg bg-orange-50 dark:bg-orange-900/20 border border-orange-200 dark:border-orange-800">
									<div class="flex items-start gap-3">
										<UIcon name="i-heroicons-exclamation-triangle"
											class="w-5 h-5 text-orange-600 dark:text-orange-400 mt-0.5" />
										<div class="flex-1">
											<h4 class="font-semibold text-orange-900 dark:text-orange-100 mb-1">
												Sell Recommendation
											</h4>
											<p class="text-sm text-orange-800 dark:text-orange-200 mb-2">
												{{ getSignalForPosition(position.symbol).reason }}
											</p>
											<div class="flex flex-wrap gap-4 text-xs">
												<div>
													<span class="text-orange-700 dark:text-orange-300">Confidence:</span>
													<span class="font-semibold text-orange-900 dark:text-orange-100 ml-1">
														{{ (getSignalForPosition(position.symbol).confidence *
															100).toFixed(0) }}%
													</span>
												</div>
												<div>
													<span class="text-orange-700 dark:text-orange-300">Current
														Gain:</span>
													<span class="font-semibold text-orange-900 dark:text-orange-100 ml-1">
														{{ getSignalForPosition(position.symbol).currentGain.toFixed(2)
														}}%
													</span>
												</div>
												<div>
													<span class="text-orange-700 dark:text-orange-300">Peak Gain:</span>
													<span class="font-semibold text-orange-900 dark:text-orange-100 ml-1">
														{{ getSignalForPosition(position.symbol).peakGain.toFixed(2) }}%
													</span>
												</div>
												<div>
													<span class="text-orange-700 dark:text-orange-300">Weekly
														Trend:</span>
													<UBadge
														:color="getSignalForPosition(position.symbol).weeklyTrend === 'UP' ? 'green' : getSignalForPosition(position.symbol).weeklyTrend === 'DOWN' ? 'red' : 'yellow'"
														size="xs" class="ml-1">
														{{ getSignalForPosition(position.symbol).weeklyTrend }}
													</UBadge>
												</div>
											</div>
										</div>
									</div>
								</div>

								<!-- Actions -->
								<div class="flex gap-2">
									<UButton color="primary" variant="outline" @click="viewAnalysis(position.symbol)">
										<UIcon name="i-heroicons-chart-bar" class="mr-2" />
										View Analysis
									</UButton>
									<UButton :color="getSignalForPosition(position.symbol) ? 'orange' : 'red'"
										@click="sellPosition(position)">
										<UIcon name="i-heroicons-arrow-right-on-rectangle" class="mr-2" />
										Sell Position
									</UButton>
								</div>
							</div>
						</UCard>
					</div>
				</div>

				<!-- Empty State -->
				<UAlert v-else icon="i-heroicons-information-circle" title="No Open Positions"
					description="You don't have any open positions. Use the Execute Trade page to buy stocks." />

				<!-- Analysis Modal -->
				<UModal v-model="showAnalysisModal">
					<UCard v-if="selectedAnalysis">
						<template #header>
							<div class="flex items-center justify-between">
								<div>
									<h3 class="text-xl font-bold text-gray-900 dark:text-white">
										{{ selectedAnalysis.symbol }} - Detailed Analysis
									</h3>
									<p class="text-sm text-gray-500 dark:text-gray-400">
										Intelligent sell strategy analysis
									</p>
								</div>
								<UButton color="gray" variant="ghost" icon="i-heroicons-x-mark" @click="showAnalysisModal = false" />
							</div>
						</template>

						<div class="space-y-6">
							<!-- Position Summary -->
							<div class="grid grid-cols-2 gap-4">
								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-600 dark:text-gray-400">Entry Price</p>
									<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
										{{ formatCurrency(selectedAnalysis.position.entryPrice) }}
									</p>
								</div>
								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-600 dark:text-gray-400">Current Price</p>
									<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
										{{ formatCurrency(selectedAnalysis.position.currentPrice) }}
									</p>
								</div>
								<div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
									<p class="text-sm text-gray-600 dark:text-gray-400">Peak Price</p>
									<p class="text-2xl font-bold text-blue-600 dark:text-blue-400 mt-1">
										{{ formatCurrency(selectedAnalysis.position.peakPrice) }}
									</p>
								</div>
								<div :class="[
									'p-4 rounded-lg',
									selectedAnalysis.position.unrealizedPnL >= 0 ? 'bg-green-50 dark:bg-green-900/20' : 'bg-red-50 dark:bg-red-900/20'
								]">
									<p class="text-sm text-gray-600 dark:text-gray-400">Unrealized P&L</p>
									<p :class="[
										'text-2xl font-bold mt-1',
										selectedAnalysis.position.unrealizedPnL >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
									]">
										{{ formatCurrency(selectedAnalysis.position.unrealizedPnL) }}
									</p>
									<p :class="[
										'text-sm font-semibold',
										selectedAnalysis.position.unrealizedPnLPercent >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
									]">
										{{ selectedAnalysis.position.unrealizedPnLPercent >= 0 ? '+' : '' }}{{
											selectedAnalysis.position.unrealizedPnLPercent.toFixed(2) }}%
									</p>
								</div>
							</div>

							<!-- Sell Signal Analysis -->
							<div>
								<h4 class="font-semibold text-gray-900 dark:text-white mb-3">Sell Signal Analysis</h4>
								<div :class="[
									'p-4 rounded-lg',
									selectedAnalysis.sellSignal.shouldSell ? 'bg-orange-50 dark:bg-orange-900/20 border border-orange-200 dark:border-orange-800' : 'bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800'
								]">
									<div class="flex items-center gap-2 mb-2">
										<UBadge :color="selectedAnalysis.sellSignal.shouldSell ? 'orange' : 'green'" size="lg">
											{{ selectedAnalysis.sellSignal.shouldSell ? 'SELL SIGNAL' : 'HOLD' }}
										</UBadge>
										<span :class="[
											'text-sm font-semibold',
											selectedAnalysis.sellSignal.shouldSell ? 'text-orange-900 dark:text-orange-100' : 'text-green-900 dark:text-green-100'
										]">
											Confidence: {{ (selectedAnalysis.sellSignal.confidence * 100).toFixed(0) }}%
										</span>
									</div>
									<p :class="[
										'text-sm',
										selectedAnalysis.sellSignal.shouldSell ? 'text-orange-800 dark:text-orange-200' : 'text-green-800 dark:text-green-200'
									]">
										{{ selectedAnalysis.sellSignal.reason }}
									</p>
									<div class="grid grid-cols-3 gap-4 mt-3 text-xs">
										<div>
											<span
												:class="selectedAnalysis.sellSignal.shouldSell ? 'text-orange-700 dark:text-orange-300' : 'text-green-700 dark:text-green-300'">
												Current Gain:
											</span>
											<span :class="[
												'font-semibold ml-1',
												selectedAnalysis.sellSignal.shouldSell ? 'text-orange-900 dark:text-orange-100' : 'text-green-900 dark:text-green-100'
											]">
												{{ selectedAnalysis.sellSignal.currentGain.toFixed(2) }}%
											</span>
										</div>
										<div>
											<span
												:class="selectedAnalysis.sellSignal.shouldSell ? 'text-orange-700 dark:text-orange-300' : 'text-green-700 dark:text-green-300'">
												Peak Gain:
											</span>
											<span :class="[
												'font-semibold ml-1',
												selectedAnalysis.sellSignal.shouldSell ? 'text-orange-900 dark:text-orange-100' : 'text-green-900 dark:text-green-100'
											]">
												{{ selectedAnalysis.sellSignal.peakGain.toFixed(2) }}%
											</span>
										</div>
										<div>
											<span
												:class="selectedAnalysis.sellSignal.shouldSell ? 'text-orange-700 dark:text-orange-300' : 'text-green-700 dark:text-green-300'">
												Weekly Trend:
											</span>
											<UBadge
												:color="selectedAnalysis.sellSignal.weeklyTrend === 'UP' ? 'green' : selectedAnalysis.sellSignal.weeklyTrend === 'DOWN' ? 'red' : 'yellow'"
												size="xs" class="ml-1">
												{{ selectedAnalysis.sellSignal.weeklyTrend }}
											</UBadge>
										</div>
									</div>
								</div>
							</div>

							<!-- Daily Snapshots Chart -->
							<div v-if="selectedAnalysis.dailySnapshots && selectedAnalysis.dailySnapshots.length > 0">
								<h4 class="font-semibold text-gray-900 dark:text-white mb-3">Price History</h4>
								<div class="space-y-2">
									<div v-for="(snapshot, idx) in selectedAnalysis.dailySnapshots.slice().reverse()" :key="idx"
										class="flex items-center justify-between p-3 rounded-lg bg-gray-50 dark:bg-gray-800">
										<div>
											<p class="text-sm font-medium text-gray-900 dark:text-white">
												{{ formatDate(snapshot.date) }}
											</p>
											<p class="text-xs text-gray-500 dark:text-gray-400">
												{{ formatCurrency(snapshot.price) }}
											</p>
										</div>
										<div class="text-right">
											<p :class="[
												'text-sm font-semibold',
												snapshot.gainPercent >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
											]">
												{{ snapshot.gainPercent >= 0 ? '+' : '' }}{{
													snapshot.gainPercent.toFixed(2) }}%
											</p>
											<UProgress :value="Math.abs(snapshot.gainPercent)"
												:color="snapshot.gainPercent >= 0 ? 'green' : 'red'" size="xs" class="w-20" />
										</div>
									</div>
								</div>
							</div>

							<!-- Actions -->
							<div class="flex gap-2">
								<UButton v-if="selectedAnalysis.sellSignal.shouldSell" block color="orange" @click="sellFromAnalysis">
									Sell Position
								</UButton>
								<UButton block color="gray" variant="outline" @click="showAnalysisModal = false">
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

const positions = ref([])
const sellSignals = ref(null)
const updating = ref(false)
const checkingSignals = ref(false)
const showAnalysisModal = ref(false)
const selectedAnalysis = ref(null)

// Fetch positions
const fetchPositions = async () => {
	const { data } = await api.positions.getAll()
	if (data) positions.value = data
}

// Update positions with current prices
const updatePositions = async () => {
	updating.value = true
	const { error } = await api.positions.update()

	if (!error) {
		await fetchPositions()
		toast.add({
			title: 'Positions Updated',
			description: 'All positions updated with current prices',
			color: 'green',
		})
	} else {
		toast.add({
			title: 'Update Failed',
			description: error || 'Failed to update positions',
			color: 'red',
		})
	}

	updating.value = false
}

// Check sell signals
const checkSellSignals = async () => {
	checkingSignals.value = true
	const { data, error } = await api.positions.getSellSignals()

	if (data) {
		sellSignals.value = data
		await fetchPositions()

		if (data.totalSignals > 0) {
			toast.add({
				title: 'Sell Signals Found',
				description: `${data.totalSignals} position(s) meet sell criteria`,
				color: 'warning',
			})
		} else {
			toast.add({
				title: 'No Sell Signals',
				description: 'All positions are performing well',
				color: 'primary',
			})
		}
	} else {
		toast.add({
			title: 'Check Failed',
			description: error || 'Failed to check sell signals',
			color: 'error',
		})
	}

	checkingSignals.value = false
}

// View detailed analysis
const viewAnalysis = async (symbol) => {
	const { data, error } = await api.positions.getAnalysis(symbol)

	if (data && !data.error) {
		selectedAnalysis.value = data
		showAnalysisModal.value = true
	} else {
		toast.add({
			title: 'Analysis Failed',
			description: data?.error || error || 'Failed to fetch analysis',
			color: 'error',
		})
	}
}

// Sell position
const sellPosition = async (position) => {
	if (!confirm(`Sell ${position.quantity} shares of ${position.symbol}?`)) {
		return
	}

	const { error } = await api.trades.sell({
		symbol: position.symbol,
		quantity: position.quantity,
	})

	if (!error) {
		toast.add({
			title: 'Position Sold',
			description: `Sold ${position.quantity} shares of ${position.symbol}`,
			color: 'primary',
		})
		await fetchPositions()
		sellSignals.value = null
	} else {
		toast.add({
			title: 'Sell Failed',
			description: error || 'Failed to sell position',
			color: 'error',
		})
	}
}

// Sell from analysis modal
const sellFromAnalysis = async () => {
	if (!selectedAnalysis.value) return

	const position = positions.value.find(p => p.symbol === selectedAnalysis.value.symbol)
	if (!position) return

	showAnalysisModal.value = false
	await sellPosition(position)
}

// Get sell signal for a position
const getSignalForPosition = (symbol) => {
	if (!sellSignals.value || !sellSignals.value.signals) return null
	return sellSignals.value.signals.find(s => s.symbol === symbol)
}

// Computed values
const totalPnL = computed(() => {
	return positions.value.reduce((sum, p) => sum + p.unrealizedPnL, 0)
})

const avgGain = computed(() => {
	if (positions.value.length === 0) return 0
	const sum = positions.value.reduce((sum, p) => sum + p.unrealizedPnLPercent, 0)
	return sum / positions.value.length
})

// Utilities
const formatCurrency = (value) => {
	return new Intl.NumberFormat('en-US', {
		style: 'currency',
		currency: 'USD',
	}).format(value)
}

const formatDate = (date) => {
	return new Date(date).toLocaleDateString('en-US', {
		month: 'short',
		day: 'numeric',
	})
}

// Fetch on mount
onMounted(() => {
	fetchPositions()
	checkSellSignals()
})
</script>