<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Global Loading Indicators -->
    <LoadingBar />
    <LoadingOverlay />
    
    <!-- Sidebar Navigation -->
    <USlideover v-model="isMobileSidebarOpen" side="left">
      <template #default>
        <MobileSidebar @close="isMobileSidebarOpen = false" />
      </template>
    </USlideover>

    <!-- Desktop Sidebar -->
    <aside class="hidden lg:fixed lg:inset-y-0 lg:left-0 lg:w-64 lg:flex lg:flex-col bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-800">
      <!-- Logo -->
      <div class="p-6 border-b border-gray-200 dark:border-gray-800">
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
        />
      </nav>

      <!-- Footer -->
      <div class="p-4 border-t border-gray-200 dark:border-gray-800">
        <p class="text-xs text-gray-500 dark:text-gray-400 text-center">
          v1.0.0 • {{ new Date().getFullYear() }}
        </p>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="lg:ml-64 min-h-screen">
      <!-- Top Bar - Full Width -->
      <div class="sticky top-0 z-10 bg-white/80 dark:bg-gray-900/80 backdrop-blur border-b border-gray-200 dark:border-gray-800">
        <div class="px-4 sm:px-6 lg:px-8">
          <div class="flex items-center justify-between h-16">
            <div class="flex items-center gap-3">
              <!-- Mobile Menu Button -->
              <UButton
                class="lg:hidden"
                color="gray"
                variant="ghost"
                icon="i-heroicons-bars-3"
                @click="isMobileSidebarOpen = true"
              />
              
              <div>
                <h2 class="text-xl font-bold text-gray-900 dark:text-white">{{ pageTitle }}</h2>
                <p class="text-sm text-gray-500 dark:text-gray-400 hidden sm:block">{{ pageDescription }}</p>
              </div>
            </div>
            
            <div class="flex items-center gap-2">
              <!-- Refresh Button -->
              <UButton
                icon="i-heroicons-arrow-path"
                color="gray"
                variant="ghost"
                :loading="isRefreshing"
                @click="refreshData"
              />
              
              <!-- Color Mode Toggle -->
              <ClientOnly>
                <UButton
                  :icon="isDark ? 'i-heroicons-moon-20-solid' : 'i-heroicons-sun-20-solid'"
                  color="gray"
                  variant="ghost"
                  @click="toggleColorMode"
                />
                <template #fallback>
                  <UButton
                    icon="i-heroicons-moon-20-solid"
                    color="gray"
                    variant="ghost"
                    disabled
                  />
                </template>
              </ClientOnly>
            </div>
          </div>
        </div>
      </div>

      <!-- Page Content -->
      <UContainer class="py-8">
        <slot />
      </UContainer>
    </main>
  </div>
</template>

<script setup>
const route = useRoute()
const colorMode = useColorMode()
const isMobileSidebarOpen = ref(false)

// Computed property for checking dark mode
const isDark = computed(() => colorMode.value === 'dark')

// Toggle function that persists the preference
const toggleColorMode = () => {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

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

const pageTitle = computed(() => {
  const item = navigation.find(n => n.path === route.path)
  return item?.label || 'Dashboard'
})

const pageDescription = computed(() => {
  const descriptions = {
    '/': 'Overview of your trading performance',
    '/portfolio': 'Portfolio summary and P&L',
    '/positions': 'Active positions and monitoring',
    '/trades': 'Complete trade history',
    '/analysis': 'AI-powered stock analysis',
    '/learning': 'Performance insights and patterns',
    '/stock-hunter': 'Discover winning stocks',
    '/scheduler': 'Automation and scheduled tasks',
    '/trade': 'Execute buy/sell orders',
  }
  return descriptions[route.path] || ''
})

const isRefreshing = ref(false)
const { startLoading, stopLoading } = useLoading()

const refreshData = async () => {
  isRefreshing.value = true
  startLoading('Refreshing data...')
  
  // Emit refresh event that pages can listen to
  window.dispatchEvent(new CustomEvent('dashboard-refresh'))
  
  setTimeout(() => {
    isRefreshing.value = false
    stopLoading()
  }, 1000)
}
</script>
