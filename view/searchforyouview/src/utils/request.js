// src/utils/request.js
import axios from 'axios'
import {
    ElMessage
} from 'element-plus'
// 创建 axios 实例
const request = axios.create({
    baseURL: 'http://localhost:2333', // 基础URL
    timeout: 15000 // 请求超时时间
})

// 请求拦截器
request.interceptors.request.use(
    config => {
        // 从 localStorage 获取 token
        const token = localStorage.getItem('token')
        if (token) {
            config.headers['Authorization'] = `Bearer ${token}`
        }
        return config
    },
    error => {
        console.error('请求错误:', error)
        return Promise.reject(error)
    }
)

// 响应拦截器
request.interceptors.response.use(
    response => {
        // 处理流式响应
        if (response.config.responseType === 'stream') {
            return new Response(response.data)
        }

        // 处理普通JSON响应
        const res = response.data
        if (!res.success) {
            ElMessage.error(res.msg || '操作失败')
            return Promise.reject(new Error(res.msg || '操作失败'))
        }
        return res
    },
    error => {
        // 处理SSE连接断开的特殊情况
        if (error.code === 'ERR_CANCELED' && error.message.includes('canceled')) {
            console.log('Stream connection closed')
            return
        }

        // 处理超时错误
        if (error.code === 'ECONNABORTED' && error.message.includes('timeout')) {
            ElMessage.error('请求超时，请重试')
            return Promise.reject(new Error('请求超时'))
        }

        // 处理网络错误
        if (!window.navigator.onLine) {
            ElMessage.error('网络连接已断开')
            return Promise.reject(new Error('网络连接已断开'))
        }

        // 其他错误处理
        console.error('响应错误:', error)
        ElMessage.error(error.message || '系统错误')
        return Promise.reject(error)
    }
)

export default request
