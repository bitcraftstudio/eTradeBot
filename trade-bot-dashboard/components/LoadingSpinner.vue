<template>
  <div 
    v-if="loading"
    class="flex items-center justify-center py-8"
    :class="containerClass"
  >
    <div class="flex flex-col items-center gap-3">
      <!-- Spinner -->
      <div class="relative">
        <div 
          class="border-4 border-gray-200 dark:border-gray-700 rounded-full"
          :class="sizeClasses"
        ></div>
        <div 
          class="absolute top-0 left-0 border-4 border-transparent border-t-primary-500 rounded-full animate-spin"
          :class="sizeClasses"
        ></div>
      </div>
      
      <!-- Message -->
      <p 
        v-if="message" 
        class="text-sm text-gray-500 dark:text-gray-400"
      >
        {{ message }}
      </p>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  loading: {
    type: Boolean,
    default: false,
  },
  message: {
    type: String,
    default: '',
  },
  size: {
    type: String,
    default: 'md', // sm, md, lg
  },
  containerClass: {
    type: String,
    default: '',
  },
})

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'sm': return 'w-6 h-6'
    case 'lg': return 'w-12 h-12'
    default: return 'w-8 h-8'
  }
})
</script>
