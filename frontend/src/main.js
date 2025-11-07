import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './routers/index.js'
import { createPinia } from 'pinia'

import PrimeVue from 'primevue/config'
import 'primeicons/primeicons.css'
import 'primeflex/primeflex.css'
import Aura from '@primeuix/themes/lara';

const pinia = createPinia()

const app = createApp(App)
    app.use(router)
    app.use(pinia)
    app.use(PrimeVue, {
        theme: {
            preset: Aura
        }
    })
    app.mount('#app')