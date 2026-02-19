<template>
  <UDashboardGroup>
    <UDashboardSidebar collapsible resizable>
        <template #header="{ collapsed }">
            <div class="flex items-center gap-3">
                <UIcon name="i-heroicons-chart-bar-20-solid" class="w-8 h-8 text-primary-500" />
                <div v-if="!collapsed">
                <h1 class="text-lg font-bold text-gray-900 dark:text-white">AI Trading Bot</h1>
                <p class="text-xs text-gray-500 dark:text-gray-400">Powered by xAI Grok</p>
                </div>
            </div>
        </template>

    </UDashboardSidebar>
    <UDashboardPanel id="home">
        <template #header>
            <UDashboardNavbar title="Home" :ui="{ right: 'gap-3' }">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>

        <template #right>
          <UTooltip text="Notifications" :shortcuts="['N']">
            <UButton
              color="neutral"
              variant="ghost"
              square
              @click="isNotificationsSlideoverOpen = true"
            >
              <UChip color="error" inset>
                <UIcon name="i-lucide-bell" class="size-5 shrink-0" />
              </UChip>
            </UButton>
          </UTooltip>

          <UDropdownMenu :items="items">
            <UButton icon="i-lucide-plus" size="md" class="rounded-full" />
          </UDropdownMenu>
        </template>
      </UDashboardNavbar>
        </template>
        <template #body>
            <slot />
        </template>
    </UDashboardPanel>
  </UDashboardGroup>
</template>