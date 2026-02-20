<template>
  <UCard>
    <div class="flex items-start justify-between">
      <div>
        <p class="text-sm font-medium text-gray-500 dark:text-gray-400">{{ title }}</p>
        <p class="text-2xl font-bold mt-2" :class="valueClass">
          {{ formattedValue }}
        </p>
        <p v-if="subtitle" class="text-sm text-gray-500 dark:text-gray-400 mt-1">
          {{ subtitle }}
        </p>
      </div>
      <UIcon :name="icon" class="w-8 h-8" :class="iconClass" />
    </div>
  </UCard>
</template>

<script setup lang="ts">
interface Props {
  title: string
  value: number | string
  subtitle?: string
  icon: string
  iconColor?: string
  format?: 'currency' | 'number' | 'percentage' | 'text'
  valueColor?: 'positive' | 'negative' | 'neutral'
}

const props = withDefaults(defineProps<Props>(), {
  format: 'currency',
  valueColor: 'neutral',
  iconColor: 'text-blue-500'
})

const formattedValue = computed(() => {
  if (props.format === 'text') return props.value

  const numValue = typeof props.value === 'string' ? parseFloat(props.value) : props.value

  switch (props.format) {
    case 'currency':
      return formatCurrency(numValue)
    case 'percentage':
      return `${numValue.toFixed(2)}%`
    case 'number':
    default:
      return numValue.toString()
  }
})

const valueClass = computed(() => {
  switch (props.valueColor) {
    case 'positive':
      return 'text-green-600 dark:text-green-400'
    case 'negative':
      return 'text-red-600 dark:text-red-400'
    case 'neutral':
    default:
      return 'text-gray-900 dark:text-white'
  }
})

const iconClass = computed(() => {
  if (props.iconColor) return props.iconColor
  return 'text-blue-500'
})

// Import formatCurrency from composables
const { formatCurrency } = useFormatters()
</script>