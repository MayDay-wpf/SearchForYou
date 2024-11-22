import './assets/main.css'
import { createApp } from 'vue'
import App from './App.vue'
// 导入 Element Plus
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'

const app = createApp(App)
// 使用 Element Plus
app.use(ElementPlus)
app.mount('#app')