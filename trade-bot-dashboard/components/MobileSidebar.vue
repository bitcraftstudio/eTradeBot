<template>
  <div class="flex flex-col h-full">
    <!-- Logo -->
    <div class="p-4 border-b border-gray-200 dark:border-gray-800">
      <div class="flex items-center gap-3">
        <UIcon name="i-heroicons-chart-bar-20-solid" class="w-8 h-8 text-primary-500" />
        <div>
          <h1 class="text-lg font-bold text-gray-900 dark:text-white">AI Trading Bot</h1>
          <p class="text-xs text-gray-500 dark:text-gray-400">Powered by xAI Grok</p>
        </div>
      </div>
    </div>

    <!-- Navigation Links -->
    <nav class="flex-1 p-4 space-y-1 overflow-y-auto">
      <UButton
        v-for="item in navigation"
        :key="item.path"
        :to="item.path"
        :icon="item.icon"
        :label="item.label"
        :color="$route.path === item.path ? 'primary' : 'gray'"
        :variant="$route.path === item.path ? 'soft' : 'ghost'"
        block
        class="justify-start"
        @click="closeOnNavigate"
      />
    </nav>

    <!-- Footer -->
    <div class="p-4 border-t border-gray-200 dark:border-gray-800">
      <p class="text-xs text-gray-500 dark:text-gray-400 text-center">
        v1.0.0 • {{ new Date().getFullYear() }}
      </p>
    </div>
  </div>
</template>

<script setup>
const route = useRoute()

const navigation = [
  {
    label: 'Dashboard',
    path: '/',
    icon: 'i-heroicons-home-20-solid',
  },
  {
    label: 'Portfolio',
    path: '/portfolio',
    icon: 'i-heroicons-chart-pie-20-solid',
  },
  {
    label: 'Positions',
    path: '/positions',
    icon: 'i-heroicons-briefcase-20-solid',
  },
  {
    label: 'Trade History',
    path: '/trades',
    icon: 'i-heroicons-clock-20-solid',
  },
  {
    label: 'AI Analysis',
    path: '/analysis',
    icon: 'i-heroicons-sparkles-20-solid',
  },
  {
    label: 'Learning',
    path: '/learning',
    icon: 'i-heroicons-academic-cap-20-solid',
  },
  {
    label: 'Stock Hunter',
    path: '/stock-hunter',
    icon: 'i-heroicons-magnifying-glass-circle-20-solid',
  },
  {
    label: 'Scheduler',
    path: '/scheduler',
    icon: 'i-heroicons-clock-20-solid',
  },
  {
    label: 'Execute Trade',
    path: '/trade',
    icon: 'i-heroicons-bolt-20-solid',
  },
]

const emit = defineEmits(['close'])

const closeOnNavigate = () => {
  emit('close')
}
</script>
