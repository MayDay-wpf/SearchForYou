import './assets/main.css'
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
// 导入 Element Plus
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'

const app = createApp(App)
// 使用 Element Plus
app.use(ElementPlus)
app.use(router)
app.mount('#app')
