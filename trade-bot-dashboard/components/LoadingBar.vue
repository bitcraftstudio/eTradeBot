<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-transform duration-200 ease-out"
      enter-from-class="-translate-y-full"
      enter-to-class="translate-y-0"
      leave-active-class="transition-opacity duration-300 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="isLoading"
        class="fixed top-0 left-0 right-0 z-50 h-1 bg-primary-100 dark:bg-primary-900 overflow-hidden"
      >
        <div 
          class="h-full bg-gradient-to-r from-primary-400 via-primary-500 to-primary-600 animate-progress-bar"
        ></div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
const { isLoading } = useLoading()
</script>

<style scoped>
@keyframes progress-bar {
  0% {
    transform: translateX(-100%);
    width: 100%;
  }
  50% {
    transform: translateX(0%);
    width: 100%;
  }
  100% {
    transform: translateX(100%);
    width: 100%;
  }
}

.animate-progress-bar {
  animation: progress-bar 1.5s ease-in-out infinite;
}
</style>
