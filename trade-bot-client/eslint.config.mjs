// @ts-check
import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt({
    rules: {
        '@stylistic/comma-dangle': ['error', 'never'],
        '@typescript-eslint/no-unused-vars': 'off',
        '@typescript-eslint/no-explicit-any': 'off',
        '@stylistic/quote-props': 'off',
        '@stylistic/eol-last': 'off',
        'vue/no-multiple-template-root': 'off',
        'vue/comma-dangle': 'off',
        'indent': ['error', '4']
    }
}).override('nuxt/vue/rules', {
    rules: {
        'vue/html-indent': ['error', 'tab', {
            attribute: 1,
            baseIndent: 0,
            closeBracket: 0,
            alignAttributesVertically: true
        }],
        'vue/html-self-closing': 'off',
        'vue/max-attributes-per-line': 'off',
        'vue/singleline-html-element-content-newline': 'off',
        'vue/no-multiple-template-root': 'off',
        'vue/attribute-hyphenation': 'off',
        'indent': ['error', '4']
    }
}).override('nuxt/sort-config', {
    rules: {
        'nuxt/nuxt-config-keys-order': 'off'
    }
})
