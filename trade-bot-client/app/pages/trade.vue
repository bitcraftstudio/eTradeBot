<template>
	<UDashboardPanel id="trade">
		<template #header>
			<Topbar :title="'Execute Trade'" :description="'Execute buy/sell orders'" />
		</template>
		<template #body>
			<div class="space-y-6">
				<!-- Trade Form -->
				<UCard>
					<template #header>
						<h3 class="text-xl font-bold">Execute Trade</h3>
					</template>

					<div class="space-y-6">
						<!-- Symbol Input -->
						<UFormGroup label="Stock Symbol" required>
							<UInput v-model="form.symbol" size="lg" placeholder="e.g., AAPL, TSLA, MSFT"
								icon="i-heroicons-magnifying-glass" @input="form.symbol = form.symbol.toUpperCase()" />
						</UFormGroup>

						<!-- Get Quote Button -->
						<UButton block variant="outline" :loading="loadingQuote" @click="getQuote">
							Get Current Quote
						</UButton>

						<!-- Current Quote -->
						<UAlert v-if="quote" icon="i-heroicons-information-circle"
							:title="`${quote.symbol} - ${formatCurrency(quote.price)}`" :description="quoteDescription"
							:color="quote.change >= 0 ? 'green' : 'red'" />

						<!-- Trade Type -->
						<UFormGroup label="Trade Type" required>
							<div class="grid grid-cols-2 gap-4">
								<UButton :variant="form.type === 'BUY' ? 'solid' : 'outline'" color="primary" size="lg" block
									@click="form.type = 'BUY'">
									<UIcon name="i-heroicons-arrow-trending-up" class="mr-2" />
									Buy
								</UButton>
								<UButton :variant="form.type === 'SELL' ? 'solid' : 'outline'" color="error" size="lg" block
									@click="form.type = 'SELL'">
									<UIcon name="i-heroicons-arrow-trending-down" class="mr-2" />
									Sell
								</UButton>
							</div>
						</UFormGroup>

						<!-- Quantity -->
						<UFormGroup label="Quantity (shares)" hint="Leave blank for auto-calculation">
							<UInput v-model.number="form.quantity" type="number" min="1" size="lg"
								placeholder="Auto-calculated based on risk" />
						</UFormGroup>

						<!-- Position Estimate -->
						<div v-if="quote && form.type === 'BUY'" class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
							<p class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Position Estimate</p>
							<div class="space-y-1 text-sm">
								<div class="flex justify-between">
									<span class="text-gray-600 dark:text-gray-400">Shares:</span>
									<span class="font-semibold text-gray-900 dark:text-white">
										{{ form.quantity || 'Auto' }}
									</span>
								</div>
								<div v-if="form.quantity" class="flex justify-between">
									<span class="text-gray-600 dark:text-gray-400">Total Cost:</span>
									<span class="font-semibold text-gray-900 dark:text-white">
										{{ formatCurrency(form.quantity * quote.price) }}
									</span>
								</div>
							</div>
						</div>

						<!-- AI Analysis Toggle -->
						<UFormGroup>
							<UCheckbox v-model="useAI" label="Get AI recommendation before executing" />
						</UFormGroup>

						<!-- Execute Button -->
						<UButton block size="lg" :color="form.type === 'BUY' ? 'green' : 'red'" :loading="executing"
							:disabled="!form.symbol" @click="executeTrade">
							<UIcon :name="form.type === 'BUY' ? 'i-heroicons-arrow-trending-up' : 'i-heroicons-arrow-trending-down'"
								class="mr-2" />
							{{ form.type === 'BUY' ? 'Execute Buy Order' : 'Execute Sell Order' }}
						</UButton>
					</div>
				</UCard>

				<!-- AI Recommendation (if enabled and loaded) -->
				<UCard v-if="aiAnalysis && useAI">
					<template #header>
						<div class="flex items-center justify-between">
							<h3 class="text-lg font-semibold">AI Recommendation</h3>
							<UBadge :color="getDecisionColor(aiRecommendation?.decision)" size="lg">
								{{ getDecisionLabel(aiRecommendation?.decision) }}
							</UBadge>
						</div>
					</template>

					<div class="space-y-4">
						<div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
							<p class="text-sm font-medium text-blue-900 dark:text-blue-300 mb-2">AI Reasoning</p>
							<p class="text-blue-800 dark:text-blue-200">{{ aiRecommendation?.reasoning || 'No reasoning available' }}
							</p>
						</div>

						<div class="grid grid-cols-2 gap-4">
							<div class="p-3 rounded-lg bg-gray-50 dark:bg-gray-800">
								<p class="text-xs text-gray-500 dark:text-gray-400">Confidence</p>
								<p class="text-xl font-bold text-gray-900 dark:text-white mt-1">
									{{ formatPercent(aiRecommendation?.confidence) }}
								</p>
							</div>

							<div class="p-3 rounded-lg bg-gray-50 dark:bg-gray-800">
								<p class="text-xs text-gray-500 dark:text-gray-400">Risk Level</p>
								<UBadge :color="getRiskColor(aiRecommendation?.riskAssessment)" class="mt-2">
									{{ getRiskLabel(aiRecommendation?.riskAssessment) }}
								</UBadge>
							</div>
						</div>

						<!-- Mismatch Warning -->
						<UAlert v-if="hasMismatch" icon="i-heroicons-exclamation-triangle" color="yellow"
							title="AI Recommendation Mismatch"
							:description="`AI recommends ${getDecisionLabel(aiRecommendation?.decision)}, but you're executing a ${form.type} order`" />

						<!-- Trade Suggestions -->
						<div v-if="aiRecommendation?.suggestedEntryPrice" class="grid grid-cols-3 gap-4">
							<div class="p-3 rounded-lg bg-green-50 dark:bg-green-900/20">
								<p class="text-xs text-green-600 dark:text-green-400">Entry Price</p>
								<p class="text-lg font-bold text-green-700 dark:text-green-300 mt-1">
									{{ formatCurrency(aiRecommendation.suggestedEntryPrice) }}
								</p>
							</div>

							<div class="p-3 rounded-lg bg-red-50 dark:bg-red-900/20">
								<p class="text-xs text-red-600 dark:text-red-400">Stop Loss</p>
								<p class="text-lg font-bold text-red-700 dark:text-red-300 mt-1">
									{{ formatCurrency(aiRecommendation.suggestedStopLoss) }}
								</p>
							</div>

							<div class="p-3 rounded-lg bg-blue-50 dark:bg-blue-900/20">
								<p class="text-xs text-blue-600 dark:text-blue-400">Take Profit</p>
								<p class="text-lg font-bold text-blue-700 dark:text-blue-300 mt-1">
									{{ formatCurrency(aiRecommendation.suggestedTakeProfit) }}
								</p>
							</div>
						</div>
					</div>
				</UCard>
			</div>
		</template>
	</UDashboardPanel>
</template>

<script setup lang="ts">
const api = useApi()
const toast = useToast()
const router = useRouter()
const route = useRoute()

const form = ref({
	symbol: '',
	type: 'BUY',
	quantity: null,
})

const useAI = ref(true)
const loadingQuote = ref(false)
const executing = ref(false)
const quote = ref(null)
const aiAnalysis = ref(null)

// Computed for nested recommendation data
const aiRecommendation = computed(() => aiAnalysis.value?.recommendation)

// Quote description computed
const quoteDescription = computed(() => {
	if (!quote.value) return ''
	const change = quote.value.change ?? 0
	const changePercent = quote.value.changePercent ?? 0
	return `Change: ${change >= 0 ? '+' : ''}${change.toFixed(2)} (${changePercent.toFixed(2)}%)`
})

// Decision enum mapping
const decisionMap = {
	0: 'BUY',
	1: 'SELL',
	2: 'HOLD',
}

// Risk enum mapping
const riskMap = {
	0: 'LOW',
	1: 'MODERATE',
	2: 'HIGH',
	3: 'VERY_HIGH',
}

// Check for mismatch between AI recommendation and selected trade type
const hasMismatch = computed(() => {
	if (!aiRecommendation.value) return false
	const aiDecision = getDecisionLabel(aiRecommendation.value.decision)
	return aiDecision !== form.value.type && aiDecision !== 'HOLD'
})

// Initialize from query params
onMounted(async () => {
	const { symbol, type } = route.query

	if (symbol) {
		form.value.symbol = symbol.toUpperCase()
	}

	if (type && ['BUY', 'SELL'].includes(type.toUpperCase())) {
		form.value.type = type.toUpperCase()
	}

	// Auto-fetch quote if symbol provided
	if (symbol) {
		await getQuote()
	}
})

const getQuote = async () => {
	if (!form.value.symbol) {
		toast.add({
			title: 'Error',
			description: 'Please enter a stock symbol',
			color: 'red',
		})
		return
	}

	loadingQuote.value = true
	quote.value = null

	const { data, error } = await api.market.getQuote(form.value.symbol)

	if (data) {
		quote.value = data
		if (useAI.value) {
			await getAIAnalysis()
		}
	} else {
		toast.add({
			title: 'Error',
			description: error || 'Failed to get quote',
			color: 'red',
		})
	}

	loadingQuote.value = false
}

const getAIAnalysis = async () => {
	const { data } = await api.ai.analyzeStock(form.value.symbol)
	if (data) {
		aiAnalysis.value = data
		console.log('AI Analysis:', data)
	}
}

const executeTrade = async () => {
	if (!form.value.symbol) {
		toast.add({
			title: 'Error',
			description: 'Please enter a stock symbol',
			color: 'red',
		})
		return
	}

	executing.value = true

	const payload = {
		symbol: form.value.symbol,
		type: form.value.type,
	}

	if (form.value.quantity) {
		payload.quantity = form.value.quantity
	}

	const { data, error } = await api.trades.execute(payload)

	if (data && data.success !== false) {
		toast.add({
			title: 'Trade Executed',
			description: `${form.value.type} order for ${data.quantity || 'auto'} shares of ${form.value.symbol} executed`,
			color: 'green',
		})

		// Reset form
		form.value = {
			symbol: '',
			type: 'BUY',
			quantity: null,
		}
		quote.value = null
		aiAnalysis.value = null

		// Redirect to trades page
		setTimeout(() => {
			router.push('/trades')
		}, 1500)
	} else {
		toast.add({
			title: 'Error',
			description: data?.message || error || 'Failed to execute trade',
			color: 'red',
		})
	}

	executing.value = false
}

// Formatters
const formatCurrency = (value) => {
	if (value == null) return '$0.00'
	return new Intl.NumberFormat('en-US', {
		style: 'currency',
		currency: 'USD',
	}).format(value)
}

const formatPercent = (value) => {
	if (value == null) return '0%'
	return `${(value * 100).toFixed(0)}%`
}

// Decision helpers
const getDecisionLabel = (decision) => {
	if (typeof decision === 'number') return decisionMap[decision] || 'HOLD'
	return decision || 'HOLD'
}

const getDecisionColor = (decision) => {
	const label = getDecisionLabel(decision)
	const colors = {
		'BUY': 'green',
		'SELL': 'red',
		'HOLD': 'yellow',
	}
	return colors[label] || 'gray'
}

// Risk helpers
const getRiskLabel = (risk) => {
	if (typeof risk === 'number') return riskMap[risk] || 'MODERATE'
	return risk || 'MODERATE'
}

const getRiskColor = (risk) => {
	const label = getRiskLabel(risk)
	const colors = {
		'LOW': 'green',
		'MODERATE': 'yellow',
		'HIGH': 'orange',
		'VERY_HIGH': 'red',
	}
	return colors[label] || 'gray'
}
</script>