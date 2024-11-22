<template>
  <div class="side-menu" :class="{ expanded: isExpanded }">
    <div class="logo">
      <img src="@/assets/logo.svg" alt="Logo">
      <span v-show="isExpanded" class="logo-text">Search For You</span>
    </div>
    <nav class="menu-items">
      <el-tooltip
          v-for="item in menuItems"
          :key="item.id"
          :content="item.title"
          placement="right"
          :show-after="100"
          :disabled="isExpanded"
          effect="dark"
      >
        <div class="menu-item" @click="handleClick(item)">
          <el-icon>
            <component :is="item.icon" />
          </el-icon>
          <span v-show="isExpanded" class="menu-title">{{ item.title }}</span>
        </div>
      </el-tooltip>
    </nav>
    <div class="bottom-items">
      <el-tooltip
          v-for="item in bottomItems"
          :key="item.id"
          :content="item.title"
          placement="right"
          :show-after="100"
          :disabled="isExpanded"
          effect="dark"
      >
        <div class="menu-item" @click="handleMenuItemClick(item)">
          <el-icon>
            <component :is="item.id === 4 ? (isExpanded ? ArrowLeft : ArrowRight) : item.icon" />
          </el-icon>
          <span v-show="isExpanded" class="menu-title">
            {{ item.id === 4 ? (isExpanded ? '收起' : '展开') : item.title }}
          </span>
        </div>
      </el-tooltip>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Search, Menu, ArrowRight, ArrowLeft, InfoFilled } from '@element-plus/icons-vue'

const isExpanded = ref(false)

const menuItems = [
  { id: 1, icon: Search, title: '搜索' },
  { id: 2, icon: Menu, title: '应用' },
]

const bottomItems = [
  { id: 4, icon: ArrowRight, title: '展开' },
  { id: 6, icon: InfoFilled, title: '关于' },
]

const handleMenuItemClick = (item) => {
  if (item.id === 4) {
    isExpanded.value = !isExpanded.value
  } else {
    handleClick(item)
  }
}

const handleClick = (item) => {
  console.log('clicked:', item.title)
}
</script>

<style scoped>
.side-menu {
  position: fixed;
  left: 0;
  top: 0;
  width: 64px;
  height: 100%;
  background-color: #fff;
  border-right: 1px solid #eee;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-sizing: border-box;
  transition: width 0.3s ease;
}

.side-menu.expanded {
  width: 200px;
  align-items: flex-start;
}

.logo {
  padding: 16px 0;
  width: 100%;
  display: flex;
  justify-content: center;
}

.logo-text {
  margin-left: 8px;
  font-size: 14px;
  font-weight: bold;
}

.logo img {
  width: 32px;
  height: 32px;
}

.menu-items {
  flex: 1;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 8px 0;
}

.menu-item {
  height: 40px;
  display: flex;
  align-items: center;
  padding: 0 20px;
  cursor: pointer;
  color: #666;
  width: 100%;
  box-sizing: border-box;
  transition: all 0.3s ease;
}

.menu-item:hover {
  background-color: #f5f5f5;
  color: #1890ff;
}

.menu-title {
  margin-left: 12px;
  font-size: 14px;
}

.bottom-items {
  width: 100%;
  padding: 16px 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

i {
  font-size: 18px;
}
</style>
