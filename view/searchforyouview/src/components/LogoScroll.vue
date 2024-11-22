<template>
  <div class="thanks-container" :class="{ 'dark-mode': isDark }">
    <h3 class="thanks-title">特别鸣谢</h3>
    <div class="thanks-wrapper">
      <div class="logo-list">
        <div v-for="(logo, index) in thanksList" :key="index" class="logo-item" @click="handleLogoClick(logo)">
          <el-image
              :src="logo.url"
              :alt="logo.name"
              fit="contain"
              class="logo-image"
          />
          <div class="logo-name">{{ logo.name }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import googleLogo from '@/assets/google.svg'
import bingLogo from '@/assets/bing.svg'
import serperLogo from '@/assets/serper.svg'
import aibotpro from '@/assets/aibotprologo.png'
import jinaai from '@/assets/jinaai.svg'
import siliconflow from '@/assets/siliconflow.png'
import { useDark } from '@vueuse/core'
const isDark = useDark()
const thanksList = [
  {
    name: 'Google',
    url: googleLogo,
    link: 'https://www.google.com'
  },
  {
    name: 'Bing',
    url: bingLogo,
    link: 'https://www.bing.com'
  },
  {
    name: 'Serper',
    url: serperLogo,
    link: 'https://serper.dev'
  },
  {
      name: 'AIBotPRO',
      url: aibotpro,
      link: 'https://embbot.com'
  },
  {
      name: 'JinaAI',
      url: jinaai,
      link: 'https://jina.ai'
  },
  {
      name: 'SiliconFlow',
      url: siliconflow,
      link: 'https://siliconflow.cn'
  }
]
const handleLogoClick = (logo) => {
  window.location.href=logo.link
}
</script>

<style scoped>
.thanks-container {
  max-width: 800px;
  margin: 100px auto;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  padding: 20px;
}

.thanks-title {
  text-align: center;
  color: #333;
  font-size: 20px;
  margin: 0 0 20px 0;
  font-weight: 500;
  position: relative;
  padding-bottom: 10px;
}

/* 添加标题下划线装饰 */
.thanks-title::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 40px;
  height: 2px;
  background-color: #409EFF; /* 使用 Element Plus 主题蓝色 */
}

.thanks-wrapper {
  width: 100%;
}

.logo-list {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 30px;
}

.logo-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  transition: transform 0.3s;
  cursor: pointer;
}

.logo-image {
  height: 30px;
  width: auto;
}

.logo-name {
  font-size: 12px;
  margin-top: 8px;
  color: #666;
}

.logo-item:hover {
  transform: scale(1.1);
}
/* 深色模式样式 */
.dark-mode {
  color: var(--el-text-color-primary);
}

.dark-mode {
  background-color: var(--el-bg-color-overlay);
  border: 1px solid var(--el-border-color-lighter);
}

.dark-mode h3 {
  color: var(--el-text-color-primary);
}

.dark-mode .logo-name {
  color: var(--el-text-color-regular);
}

</style>
