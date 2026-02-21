// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    modules: ['@nuxt/ui', '@nuxt/eslint'],
    compatibilityDate: '2025-07-15',
    devtools: { enabled: true },

    ssr: false,
    css: ['~/assets/css/main.css'],

    runtimeConfig: {
        public: {
            apiUrl: process.env.NUXT_PUBLIC_API_URL || 'http://localhost:7012'
        }
    },

    eslint: {
        config: {
            stylistic: {
                indent: 4,
                semi: false,
                quotes: 'single'
            }
        }
    },

    app: {
        head: {
            title: 'AI Trading Bot Dashboard'
        }
    },

    devServer: {
        port: 3002
    }
})
