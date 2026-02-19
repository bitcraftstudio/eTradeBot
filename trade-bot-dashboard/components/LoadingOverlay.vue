<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="showOverlay"
        class="fixed inset-0 z-9999 flex items-center justify-center bg-gray-900/50 dark:bg-gray-950/70 backdrop-blur-sm"
        @click.self="() => {}"
      >
        <div class="flex flex-col items-center gap-4 p-8 rounded-2xl bg-white dark:bg-gray-800 shadow-2xl border border-gray-200 dark:border-gray-700">
          <!-- Spinner -->
          <div class="relative">
            <div class="w-14 h-14 border-4 border-primary-100 dark:border-primary-900 rounded-full"></div>
            <div class="absolute top-0 left-0 w-14 h-14 border-4 border-transparent border-t-primary-500 border-r-primary-500 rounded-full animate-spin"></div>
          </div>
          
          <!-- Message -->
          <p class="text-base font-medium text-gray-700 dark:text-gray-200">
            {{ displayMessage }}
          </p>
          
          <!-- Animated dots -->
          <div class="flex gap-1.5">
            <span class="w-2 h-2 bg-primary-400 rounded-full animate-bounce" style="animation-delay: 0ms"></span>
            <span class="w-2 h-2 bg-primary-500 rounded-full animate-bounce" style="animation-delay: 150ms"></span>
            <span class="w-2 h-2 bg-primary-600 rounded-full animate-bounce" style="animation-delay: 300ms"></span>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
const { isLoading, message } = useLoading()

// Debounce to prevent flash for fast loads
const showOverlay = ref(false)
let showTimer: ReturnType<typeof setTimeout> | null = null
let hideTimer: ReturnType<typeof setTimeout> | null = null

const displayMessage = computed(() => message.value || 'Loading...')

/*
watch(isLoading, (loading) => {
  if (loading) {
    // Clear any pending hide timer
    if (hideTimer) {
      clearTimeout(hideTimer)
      hideTimer = null
    }
    // Show after small delay to prevent flash for fast operations
    showTimer = setTimeout(() => {
      showOverlay.value = true
    }, 150)
  } else {
    // Clear show timer if load finished quickly
    if (showTimer) {
      clearTimeout(showTimer)
      showTimer = null
    }
    // Hide with small delay for smooth transition
    hideTimer = setTimeout(() => {
      showOverlay.value = false
    }, 100)
  }
}, { immediate: true })
*/
onUnmounted(() => {
  if (showTimer) clearTimeout(showTimer)
  if (hideTimer) clearTimeout(hideTimer)
})
</script>
