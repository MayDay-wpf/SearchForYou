import './assets/main.css'
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
// 导入 Element Plus
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import 'katex/dist/katex.min.css'
import 'markdown-it-texmath/css/texmath.css'

const app = createApp(App)
// 使用 Element Plus
app.use(ElementPlus)
app.use(router)
app.mount('#app')
