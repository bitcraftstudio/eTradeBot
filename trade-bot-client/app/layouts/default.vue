<template>
	<UDashboardGroup unit="rem">
		<UDashboardSidebar :resizable="false" collapsible :default-size="25" :max-size="25" :min-size="25" :ui="{
			footer: 'border-t border-default',
			header: 'border-b border-default mb-4'
		}">
			<template #header="{ collapsed }">
				<div class="flex items-center gap-3">
					<UIcon name="i-heroicons-chart-bar-20-solid" class="w-8 h-8 text-primary-500" />
					<div v-if="!collapsed">
						<h1 class="text-lg font-bold text-gray-900 dark:text-white">AI Trading Bot</h1>
						<p class="text-xs text-gray-500 dark:text-gray-400">Powered by xAI Grok</p>
					</div>
				</div>
			</template>
			<template #default="{ collapsed }">
				<UNavigationMenu :collapsed="collapsed" :items="items" orientation="vertical" />
			</template>
			<template #footer="{ collapsed }">
				<div class="p-4 text-gray-600">v1.0.0</div>
			</template>
		</UDashboardSidebar>
		<slot />
	</UDashboardGroup>
</template>

<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui';

const items: NavigationMenuItem[] = [
	[
		{ label: 'Dashboard', icon: 'i-heroicons-home-20-solid', to: '/' },
		{ label: 'Portfolio', icon: 'i-heroicons-chart-pie-20-solid', to: '/portfolio' },
		{ label: 'Positions', icon: 'i-heroicons-briefcase-20-solid', to: '/positions' },
		{ label: 'Trade History', icon: 'i-heroicons-clock-20-solid', to: '/trades' },
		{ label: 'AI Analysis', icon: 'i-heroicons-sparkles-20-solid', to: '/analysis' },
		{ label: 'Learning', icon: 'i-heroicons-academic-cap-20-solid', to: '/learning' },
		{ label: 'Stock Hunter', icon: 'i-heroicons-magnifying-glass-circle-20-solid', to: '/stock-hunter' },
		{ label: 'Scheduler', icon: 'i-heroicons-clock-20-solid', to: '/scheduler' },
		{ label: 'Execute Trade', icon: 'i-heroicons-bolt-20-solid', to: '/trade' }
	],
];

</script>