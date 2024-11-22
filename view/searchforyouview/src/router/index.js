import { createRouter, createWebHistory } from 'vue-router'
import SearchBox from '@/components/SearchBox.vue'
import SearchResult from '@/components/SearchResult.vue'
import Share from '@/components/Share.vue'


const routes = [
    {
        path: '/',
        name: 'Home',
        component: SearchBox
    },
    {
        path: '/search',
        name: 'Search',
        component: SearchBox
    },
    {
        path: '/result',
        name: 'SearchResult',
        component: SearchResult
    },
    {
        path: '/share',
        name: 'Share',
        component: Share
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

export default router
