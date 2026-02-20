// Global loading state composable - uses Vue's reactive system
export const useLoading = () => {
  // Use useState for SSR-safe global state
  const isLoading = useState('global-loading', () => false)
  const message = useState('global-loading-message', () => '')
  const loadingCount = useState('global-loading-count', () => 0)

  const startLoading = (msg = 'Loading...') => {
    loadingCount.value++
    isLoading.value = true
    message.value = msg
  }

  const stopLoading = () => {
    loadingCount.value = Math.max(0, loadingCount.value - 1)
    if (loadingCount.value === 0) {
      isLoading.value = false
      message.value = ''
    }
  }

  const forceStopLoading = () => {
    loadingCount.value = 0
    isLoading.value = false
    message.value = ''
  }

  // Wrap an async function with loading state
  const withLoading = async <T>(fn: () => Promise<T>, msg = 'Loading...'): Promise<T> => {
    startLoading(msg)
    try {
      return await fn()
    } finally {
      stopLoading()
    }
  }

  return {
    isLoading: computed(() => isLoading.value),
    message: computed(() => message.value),
    startLoading,
    stopLoading,
    forceStopLoading,
    withLoading,
  }
}
