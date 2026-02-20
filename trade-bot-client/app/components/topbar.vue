<template>
	<UDashboardNavbar :ui="{ title: 'font-bold text-xl' }">
		<template #leading>
			<div class="flex items-center gap-3">
				<UDashboardSidebarCollapse />
				<div>
					<h1 class="text-lg font-bold text-gray-900 dark:text-white">{{ title }}</h1>
					<p class="text-xs text-gray-500 dark:text-gray-400">{{ description }}</p>
				</div>
			</div>
		</template>

		<template #right>
			<UButton icon="i-heroicons-arrow-path" variant="ghost" color="neutral" :loading="isRefreshing"
				@click="refreshData" />
			<UColorModeButton />
		</template>
	</UDashboardNavbar>
</template>

<script setup lang="ts">
interface Props {
	title?: string,
	description?: string
}

const props = withDefaults(defineProps<Props>(), {
	title: 'AI Trading Bot',
	description: 'Powered by xAI Grok'
});
</script>