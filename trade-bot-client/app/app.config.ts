export default defineAppConfig({
  ui: {
    primary: 'green',
    gray: 'slate',

    strategy: 'merge',

    dashboardPanel: {
      slots: {
        root: 'bg-gray-50 dark:bg-gray-950',
        body: 'bg-gray-50 dark:bg-gray-950 w-full max-w-7xl mx-auto'
      }
    },
    dashboardNavbar: {
      slots: {
        root: 'bg-white dark:bg-slate-900 border-b',
      }
    },

    notifications: {
      position: 'top-0 bottom-auto',
    },
  }
})