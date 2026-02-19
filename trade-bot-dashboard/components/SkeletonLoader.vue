<template>
  <div class="animate-pulse">
    <!-- Text lines -->
    <template v-if="type === 'text'">
      <div 
        v-for="i in lines" 
        :key="i"
        class="h-4 bg-gray-200 dark:bg-gray-700 rounded mb-2"
        :class="i === lines ? 'w-3/4' : 'w-full'"
      ></div>
    </template>
    
    <!-- Card -->
    <template v-else-if="type === 'card'">
      <div class="p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/3 mb-4"></div>
        <div class="h-8 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-2"></div>
        <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
      </div>
    </template>
    
    <!-- Stats grid -->
    <template v-else-if="type === 'stats'">
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div v-for="i in 4" :key="i" class="p-4 rounded-lg bg-gray-100 dark:bg-gray-800">
          <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-2"></div>
          <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
        </div>
      </div>
    </template>
    
    <!-- Table rows -->
    <template v-else-if="type === 'table'">
      <div class="space-y-3">
        <div v-for="i in rows" :key="i" class="flex gap-4 p-3 border-b border-gray-200 dark:border-gray-700">
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/6"></div>
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/4"></div>
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/3"></div>
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/6"></div>
        </div>
      </div>
    </template>
    
    <!-- Chart placeholder -->
    <template v-else-if="type === 'chart'">
      <div class="h-64 bg-gray-100 dark:bg-gray-800 rounded-lg flex items-end justify-around p-4">
        <div v-for="i in 7" :key="i" class="bg-gray-200 dark:bg-gray-700 rounded-t w-8" :style="{ height: `${20 + Math.random() * 60}%` }"></div>
      </div>
    </template>
    
    <!-- Avatar with text -->
    <template v-else-if="type === 'avatar'">
      <div class="flex items-center gap-3">
        <div class="w-10 h-10 bg-gray-200 dark:bg-gray-700 rounded-full"></div>
        <div class="flex-1">
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/3 mb-2"></div>
          <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
defineProps({
  type: {
    type: String,
    default: 'text', // text, card, stats, table, chart, avatar
  },
  lines: {
    type: Number,
    default: 3,
  },
  rows: {
    type: Number,
    default: 5,
  },
})
</script>
