// Plugin to handle page loading state
export default defineNuxtPlugin((nuxtApp) => {
  const { startLoading, stopLoading, forceStopLoading } = useLoading()

  // Hook into page navigation
  nuxtApp.hook('page:start', () => {
    startLoading('Loading page...')
  })

  nuxtApp.hook('page:finish', () => {
    // Small delay to ensure smooth transition
    setTimeout(() => {
      stopLoading()
    }, 100)
  })

  // Handle errors
  nuxtApp.hook('vue:error', () => {
    forceStopLoading()
  })

  nuxtApp.hook('app:error', () => {
    forceStopLoading()
  })
})
