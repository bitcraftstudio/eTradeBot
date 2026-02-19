export default defineAppConfig({
  ui: {
    primary: 'green',
    gray: 'slate',
    
    // Strategy for managing UI components
    strategy: 'merge',
    
    // Global component defaults
    button: {
      default: {
        loadingIcon: 'i-heroicons-arrow-path',
      },
    },
    
    card: {
      base: '',
      background: 'bg-white dark:bg-gray-800',
      divide: 'divide-y divide-gray-200 dark:divide-gray-700',
      ring: 'ring-1 ring-gray-200 dark:ring-gray-700',
      rounded: 'rounded-lg',
      shadow: 'shadow',
    },
    
    badge: {
      default: {
        variant: 'subtle',
      },
    },
    
    notifications: {
      position: 'top-0 bottom-auto',
    },
  },
})
