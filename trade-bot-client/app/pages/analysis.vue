<template>
	<UDashboardPanel id="analysis">
		<template #header>
			<Topbar :title="'AI Analysis'" :description="'AI-powered stock analysis'" />
		</template>
		<template #body>
			<div class="space-y-6">
				<!-- Stock Analysis Form -->
				<UCard>
					<template #header>
						<h3 class="text-xl font-bold">AI Stock Analysis</h3>
					</template>

					<div class="space-y-4">
						<UInput v-model="symbol" icon="i-heroicons-magnifying-glass" size="lg"
							placeholder="Enter stock symbol (e.g., AAPL, TSLA, MSFT)" @keyup.enter="analyzeStock" />

						<UButton block size="lg" :loading="analyzing" @click="analyzeStock">
							Analyze Stock
						</UButton>
					</div>
				</UCard>

				<!-- Analysis Results -->
				<div v-if="analysis" class="space-y-6">
					<!-- Price Header -->
					<UCard>
						<div class="flex items-center justify-between">
							<div>
								<h2 class="text-2xl font-bold text-gray-900 dark:text-white">{{ analysis.symbol }}</h2>
								<p class="text-3xl font-bold text-gray-900 dark:text-white mt-2">
									{{ formatCurrency(analysis.currentPrice) }}
								</p>
							</div>
							<UBadge :color="getDecisionColor(recommendation?.decision)" size="lg" class="text-lg px-4 py-2">
								{{ getDecisionLabel(recommendation?.decision) }}
							</UBadge>
						</div>
					</UCard>

					<!-- AI Recommendation -->
					<UCard>
						<template #header>
							<div class="flex items-center justify-between">
								<h3 class="text-lg font-semibold">AI Recommendation</h3>
								<div class="flex items-center gap-2">
									<span class="text-sm text-gray-500">Confidence:</span>
									<UBadge color="blue" variant="subtle">
										{{ formatPercent(recommendation?.confidence) }}
									</UBadge>
								</div>
							</div>
						</template>

						<div class="space-y-4">
							<!-- Reasoning -->
							<div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
								<p class="text-sm font-medium text-blue-900 dark:text-blue-300 mb-2">AI Reasoning</p>
								<p class="text-blue-800 dark:text-blue-200">{{ recommendation?.reasoning || 'No reasoning available' }}
								</p>
							</div>

							<!-- Scores Grid -->
							<div class="grid grid-cols-2 md:grid-cols-4 gap-4">
								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-500 dark:text-gray-400">Confidence</p>
									<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
										{{ formatPercent(recommendation?.confidence) }}
									</p>
									<UProgress :value="(recommendation?.confidence || 0) * 100" class="mt-2" />
								</div>

								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-500 dark:text-gray-400">Risk Level</p>
									<UBadge :color="getRiskColor(recommendation?.riskAssessment)" size="lg" class="mt-2">
										{{ getRiskLabel(recommendation?.riskAssessment) }}
									</UBadge>
								</div>

								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-500 dark:text-gray-400">Technical Score</p>
									<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
										{{ recommendation?.technicalScore || 0 }}/100
									</p>
								</div>

								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-500 dark:text-gray-400">Momentum Score</p>
									<p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">
										{{ recommendation?.momentumScore || 0 }}/100
									</p>
								</div>
							</div>

							<!-- Trade Suggestions -->
							<div v-if="recommendation?.suggestedEntryPrice" class="grid grid-cols-3 gap-4">
								<div class="p-4 rounded-lg bg-green-50 dark:bg-green-900/20">
									<p class="text-sm text-green-600 dark:text-green-400">Entry Price</p>
									<p class="text-xl font-bold text-green-700 dark:text-green-300 mt-1">
										{{ formatCurrency(recommendation.suggestedEntryPrice) }}
									</p>
								</div>

								<div class="p-4 rounded-lg bg-red-50 dark:bg-red-900/20">
									<p class="text-sm text-red-600 dark:text-red-400">Stop Loss</p>
									<p class="text-xl font-bold text-red-700 dark:text-red-300 mt-1">
										{{ formatCurrency(recommendation.suggestedStopLoss) }}
									</p>
								</div>

								<div class="p-4 rounded-lg bg-blue-50 dark:bg-blue-900/20">
									<p class="text-sm text-blue-600 dark:text-blue-400">Take Profit</p>
									<p class="text-xl font-bold text-blue-700 dark:text-blue-300 mt-1">
										{{ formatCurrency(recommendation.suggestedTakeProfit) }}
									</p>
								</div>
							</div>

							<!-- Expected Returns -->
							<div class="grid grid-cols-2 gap-4">
								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-500 dark:text-gray-400">Expected Return</p>
									<p class="text-xl font-bold mt-1"
										:class="recommendation?.expectedReturn >= 0 ? 'text-green-600' : 'text-red-600'">
										{{ formatPercent(recommendation?.expectedReturn) }}
									</p>
								</div>

								<div class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
									<p class="text-sm text-gray-500 dark:text-gray-400">Max Risk</p>
									<p class="text-xl font-bold text-red-600 mt-1">
										{{ formatPercent(recommendation?.maxRisk) }}
									</p>
								</div>
							</div>
						</div>
					</UCard>

					<!-- Technical Analysis -->
					<UCard>
						<template #header>
							<div class="flex items-center justify-between">
								<h3 class="text-lg font-semibold">Technical Analysis</h3>
								<UBadge :color="getTrendColor(technicalAnalysis?.trend)" variant="subtle">
									{{ getTrendLabel(technicalAnalysis?.trend) }} ({{ technicalAnalysis?.strength || 0
									}}%)
								</UBadge>
							</div>
						</template>

						<!-- Signals -->
						<div v-if="technicalAnalysis?.signals?.length" class="mb-4">
							<p class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Signals</p>
							<div class="flex flex-wrap gap-2">
								<UBadge v-for="(signal, idx) in technicalAnalysis.signals" :key="idx" color="neutral" variant="subtle">
									{{ signal }}
								</UBadge>
							</div>
						</div>
					</UCard>

					<!-- News Sentiment -->
					<UCard>
						<template #header>
							<div class="flex items-center justify-between">
								<h3 class="text-lg font-semibold">News Sentiment</h3>
								<UBadge :color="getSentimentColor(sentiment?.overallSentiment)" variant="subtle">
									{{ sentiment?.sentimentLabel || 'NEUTRAL' }}
								</UBadge>
							</div>
						</template>

						<div v-if="sentiment?.articles?.length" class="space-y-3">
							<div v-for="(article, idx) in sentiment.articles.slice(0, 5)" :key="idx"
								class="p-4 rounded-lg bg-gray-50 dark:bg-gray-800">
								<div class="flex items-start justify-between gap-4">
									<div class="flex-1">
										<p class="font-medium text-gray-900 dark:text-white">{{ article.title }}</p>
										<p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
											{{ article.source }} • {{ formatDate(article.publishedDate) }}
										</p>
									</div>
									<UBadge :color="getSentimentColor(article.sentiment)" variant="subtle">
										{{ getSentimentLabel(article.sentiment) }}
									</UBadge>
								</div>
							</div>
						</div>

						<div v-else class="text-center py-4 text-gray-500">
							No recent news available
						</div>
					</UCard>

					<!-- Quick Trade -->
					<UCard>
						<div class="flex items-center justify-between">
							<div>
								<h3 class="text-lg font-semibold">Quick Trade</h3>
								<p class="text-sm text-gray-500">Execute based on AI recommendation</p>
							</div>
							<div class="flex gap-3">
								<UButton color="green" size="lg" @click="goToTrade('BUY')">
									Buy {{ analysis.symbol }}
								</UButton>
								<UButton color="red" size="lg" @click="goToTrade('SELL')">
									Sell {{ analysis.symbol }}
								</UButton>
							</div>
						</div>
					</UCard>
				</div>

				<!-- No Results -->
				<UAlert v-else-if="!analyzing" icon="i-heroicons-information-circle" title="Enter a stock symbol"
					description="Enter a stock symbol above to get AI-powered analysis" />
			</div>
		</template>
	</UDashboardPanel>
</template>

<script setup lang="ts">
const api = useApi()
const toast = useToast()
const router = useRouter()

const symbol = ref('')
const analyzing = ref(false)
const analysis = ref(null)

// Computed properties to access nested data
const recommendation = computed(() => analysis.value?.recommendation)
const technicalAnalysis = computed(() => analysis.value?.technicalAnalysis)
const sentiment = computed(() => analysis.value?.sentiment)

// Decision enum mapping (backend sends numbers)
const decisionMap = {
	0: 'BUY',
	1: 'SELL',
	2: 'HOLD',
}

// Risk assessment enum mapping
const riskMap = {
	0: 'LOW',
	1: 'MODERATE',
	2: 'HIGH',
	3: 'VERY_HIGH',
}

// Trend enum mapping
const trendMap = {
	0: 'BULLISH',
	1: 'BEARISH',
	2: 'NEUTRAL',
}

const analyzeStock = async () => {
	if (!symbol.value) {
		toast.add({
			title: 'Error',
			description: 'Please enter a stock symbol',
			color: 'red',
		})
		return
	}

	analyzing.value = true
	analysis.value = null

	const { data, error } = await api.ai.analyzeStock(symbol.value.toUpperCase())

	if (data) {
		analysis.value = data
		console.log('Analysis result:', data)
	} else {
		toast.add({
			title: 'Error',
			description: error || 'Failed to analyze stock',
			color: 'red',
		})
	}

	analyzing.value = false
}

const goToTrade = (type) => {
	router.push({
		path: '/trade',
		query: {
			symbol: analysis.value?.symbol,
			type: type,
		},
	})
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
	return `${(value * 100).toFixed(1)}%`
}

const formatDate = (dateStr) => {
	if (!dateStr) return ''
	return new Date(dateStr).toLocaleDateString('en-US', {
		month: 'short',
		day: 'numeric',
	})
}

// Decision helpers (handles both string and number enum values)
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

// Risk helpers (handles both string and number enum values)
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

// Trend helpers (handles both string and number enum values)
const getTrendLabel = (trend) => {
	if (typeof trend === 'number') return trendMap[trend] || 'NEUTRAL'
	return trend || 'NEUTRAL'
}

const getTrendColor = (trend) => {
	const label = getTrendLabel(trend)
	const colors = {
		'BULLISH': 'green',
		'BEARISH': 'red',
		'NEUTRAL': 'yellow',
	}
	return colors[label] || 'gray'
}

// Sentiment helpers
const getSentimentColor = (sentiment) => {
	if (sentiment == null) return 'gray'
	if (sentiment > 0.3) return 'green'
	if (sentiment < -0.3) return 'red'
	return 'gray'
}

const getSentimentLabel = (sentiment) => {
	if (sentiment == null) return 'Neutral'
	if (sentiment > 0.3) return 'Positive'
	if (sentiment < -0.3) return 'Negative'
	return 'Neutral'
}
</script>