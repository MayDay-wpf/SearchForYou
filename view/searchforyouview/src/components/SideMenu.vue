<template>
  <el-menu
      :collapse="!isExpanded"
      :collapse-transition="false"
      class="side-menu"
      :default-active="activeIndex"
  >
    <div class="logo-container">
      <img :src="logoSrc" alt="Logo" class="logo-img">
      <span v-show="isExpanded" class="logo-text">Search For You</span>
    </div>

    <!-- 主菜单项 -->
    <el-menu-item
        v-for="item in menuItems"
        :key="item.id"
        :index="item.route"
        @click="handleClick(item)"
    >
      <el-icon><component :is="item.icon" /></el-icon>
      <template #title>{{ item.title }}</template>
    </el-menu-item>

    <!-- 底部菜单项 -->
    <div class="bottom-items">
      <el-menu-item
          v-for="item in bottomItems"
          :key="item.id"
          :index="String(item.id)"
          @click="handleMenuItemClick(item)"
      >
        <el-icon>
          <component :is="item.id === 4 ? (isExpanded ? ArrowLeft : ArrowRight) : item.icon" />
        </el-icon>
        <template #title>
          {{ item.id === 4 ? (isExpanded ? '收起' : '展开') : item.title }}
        </template>
      </el-menu-item>
    </div>
  </el-menu>
</template>

<script setup>
import {ref, computed} from 'vue'
import {useDark} from '@vueuse/core'
import {Search, Menu, ArrowRight, ArrowLeft, InfoFilled,Share} from '@element-plus/icons-vue'
import {useRouter, useRoute} from 'vue-router'

const isDark = useDark()
const isExpanded = ref(false)
const router = useRouter()
const route = useRoute()

const activeIndex = computed(() => route.path)

import logoLight from '@/assets/logo.svg'
import logoDark from '@/assets/logo_white.svg'

const logoSrc = computed(() => (isDark.value ? logoDark : logoLight))

const menuItems = [
  {id: 1, icon: Search, title: '搜索', route: '/search'},
  {id: 2, icon: Share, title: '分享', route: '/share'},
]

const bottomItems = [
  {id: 4, icon: ArrowRight, title: '展开'},
  {id: 6, icon: InfoFilled, title: '关于'},
]

const handleMenuItemClick = (item) => {
  if (item.id === 4) {
    isExpanded.value = !isExpanded.value
  } else {
    handleClick(item)
  }
}

const handleClick = (item) => {
  if (item.route) {
    router.push(item.route)
  }
  if(item.id===6){
    window.open('https://github.com/MayDay-wpf/SearchForYou')
  }
}
</script>

<style scoped>
.side-menu {
  position: fixed;
  left: 0;
  top: 0;
  height: 100vh;
  border-right: 1px solid var(--el-border-color);
  transition: width 0.3s;
}

.side-menu:not(.el-menu--collapse) {
  width: 200px;
}

.logo-container {
  height: 60px;
  padding: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.logo-img {
  width: 32px;
  height: 32px;
  filter: var(--el-menu-icon-filter);
}

.logo-text {
  margin-left: 8px;
  font-size: 14px;
  font-weight: bold;
  color: var(--el-text-color-primary);
}

.bottom-items {
  position: absolute;
  bottom: 0;
  width: 100%;
  border-top: 1px solid var(--el-border-color);
}

:deep(.el-menu-item) {
  display: flex;
  align-items: center;
}

/* 深色模式适配 */
html.dark {
  --el-menu-icon-filter: invert(1);
}

:deep(.el-menu) {
  background-color: var(--el-bg-color-overlay);
}

:deep(.el-menu-item) {
  color: var(--el-text-color-primary);
}

:deep(.el-menu-item.is-active) {
  background-color: var(--el-color-primary-light-9);
}
</style>
