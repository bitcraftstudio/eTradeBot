// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  // Nuxt 4 compatibility
  future: {
    compatibilityVersion: 4,
  },

  compatibilityDate: '2024-11-01',
  
  devtools: { enabled: true },
  
  modules: ['@nuxt/ui'],

  // Global CSS
  css: [
    '~/assets/css/main.css',
    '~/assets/css/transitions.css',
  ],

  runtimeConfig: {
    public: {
      apiUrl: process.env.NUXT_PUBLIC_API_URL || 'http://localhost:3001',
    },
  },

  app: {
    head: {
      title: 'AI Trading Bot Dashboard',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'AI-powered trading bot with Nuxt 4 & UI v3' },
      ],
    },
    // Page transition for smooth loading
    pageTransition: { name: 'page', mode: 'out-in' },
  },

  colorMode: {
    preference: 'dark',
    fallback: 'dark',
    storageKey: 'nuxt-color-mode',
    classSuffix: '',
  },

  // Components auto-import
  components: {
    global: true,
    dirs: ['~/components'],
  },

  // PostCSS configuration for Tailwind v4
  postcss: {
    plugins: {
      '@tailwindcss/postcss': {},
    },
  },
})