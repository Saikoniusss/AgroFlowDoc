import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './routers/index.js'
import { createPinia } from 'pinia'
import ToastService from 'primevue/toastservice'
import PrimeVue from 'primevue/config'
import 'primeicons/primeicons.css'
import 'primeflex/primeflex.css'
import Aura from '@primeuix/themes/lara';
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Calendar from 'primevue/calendar'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import Toast from 'primevue/toast'

const pinia = createPinia()

const app = createApp(App)
    app.use(router)
    app.use(pinia)
    app.use(PrimeVue, {
        theme: {
            preset: Aura
        }
    })
    app.use(ToastService) // <-- регистрация ToastService
    app.component('InputText', InputText)
    app.component('InputNumber', InputNumber)
    app.component('Calendar', Calendar)
    app.component('Dropdown', Dropdown)
    app.component('Button', Button)
    app.component('Toast', Toast)
    app.mount('#app')