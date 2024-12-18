<template>
  <div class="search-container">
    <div class="logo">
      <img :src="logoSrc" alt="Logo" />
      <h1 class="title">Search For You</h1>
    </div>
    <div class="subtitle">🤔想搜点什么？</div>
    <div class="subtitle">Bing与Google的图片反向搜索效果不理想，推荐Serper🤗</div>
    <div class="mode-switch">
      <div class="switch-container" @click="toggleReading">
        <div class="switch-option" :class="{ active: !reading }">
          ⚡️ 简易
        </div>
        <div class="switch-option" :class="{ active: reading }">
          🐌 深入
        </div>
        <div class="switch-background" :class="{ right: reading }"></div>
      </div>
    </div>
    <div class="search-box">
      <el-input
          v-model="searchText"
          placeholder="请输入，Enter键发送，Shift+Enter键换行"
          :rows="4"
          type="textarea"
          resize="none"
          @keydown.enter="handleEnter"
          @paste="handlePaste"
          @drop.prevent="handleDrop"
          @dragover.prevent
      />
      <!-- 工具栏 包含搜索引擎选择和图片上传 -->
      <div class="toolbar">
        <el-dropdown @command="handleEngineChange" trigger="click">
          <div class="dropdown-trigger">
            <img :src="currentEngineLogo" class="engine-logo" alt="current engine" />
            <span class="engine-name">{{ currentEngineName }}</span>
            <el-icon class="el-icon--right">
              <arrow-down />
            </el-icon>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item
                  v-for="engine in searchEngines"
                  :key="engine.value"
                  :command="engine.value"
              >
                <div class="search-engine-option">
                  <img :src="engine.logo" class="engine-logo" alt="engine logo" />
                  {{ engine.label }}
                </div>
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <div class="image-upload">
          <el-upload
              class="upload-container"
              :show-file-list="false"
              :auto-upload="false"
              accept="image/*"
              @change="handleImageChange"
          >
            <template #trigger>
              <el-button :icon="Camera" circle v-if="!imageUrl" />
            </template>
          </el-upload>
          <div v-if="imageUrl" class="image-preview">
            <img :src="imageUrl" class="preview-image" />
            <el-button
                type="danger"
                :icon="Delete"
                circle
                class="remove-button"
                @click="removeImage"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
  <LogoScroll />
</template>

<script setup>
import request from '@/utils/request'
import LogoScroll from "@/components/LogoScroll.vue";
import { ref, computed } from 'vue'
import { Camera, Delete, ArrowDown } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import logoLight from '@/assets/logo.svg'
import logoDark from '@/assets/logo_white.svg'
import googleLogo from '@/assets/google.svg'
import bingLogo from '@/assets/bing.svg'
import serperLogo from '@/assets/serper.svg'
import { useDark } from "@vueuse/core"
import { useRouter } from 'vue-router'

const router = useRouter()
const isDark = useDark()
const logoSrc = computed(() => (isDark.value ? logoDark : logoLight))
const searchText = ref('')
const imageUrl = ref('')
const searchEngine = ref('bing')
const reading = ref(false)

const toggleReading = () => {
  reading.value = !reading.value
}
const searchEngines = [
  {
    label: 'Bing',
    value: 'bing',
    logo: bingLogo
  },
  {
    label: 'Google',
    value: 'google',
    logo: googleLogo
  },
  {
    label: 'Serper',
    value: 'serper',
    logo: serperLogo
  }
]

const currentEngineLogo = computed(() => {
  const engine = searchEngines.find(e => e.value === searchEngine.value)
  return engine ? engine.logo : searchEngines[0].logo
})

const currentEngineName = computed(() => {
  const engine = searchEngines.find(e => e.value === searchEngine.value)
  return engine ? engine.label : searchEngines[0].label
})

const handleEngineChange = (command) => {
  searchEngine.value = command
}

const handleEnter = async (e) => {
  if (e.isComposing || e.keyCode === 229) {
    return
  }
  if (e.shiftKey) {
    return
  }
  e.preventDefault()
  if(!searchText.value){
    ElMessage.warning('请输入搜索内容')
    return
  }
  // 如果有正在上传的图片，等待上传完成
  if (selectedFile.value) {
    ElMessage.warning('图片上传中，请稍后')
    return
  }

  const buildSearchQuery = (params) => {
    const searchParams = new URLSearchParams()

    Object.entries(params).forEach(([key, value]) => {
      if (value) {
        searchParams.append(key, value)
      }
    })

    return Object.fromEntries(searchParams)
  }

// 构建搜索参数
  const searchParams = buildSearchQuery({
    q: searchText.value,
    engine: searchEngine.value,
    image: imageUrl.value,
    reading: reading.value
  })

  // 跳转到搜索结果页面
  router.push({
    name: 'SearchResult',
    query: searchParams
  })
}

// 处理粘贴事件
const handlePaste = (e) => {
  const items = e.clipboardData?.items
  if (!items) return

  for (const item of items) {
    if (item.type.startsWith('image/')) {
      const file = item.getAsFile()
      handleImageFile(file)
      break
    }
  }
}

// 处理拖拽事件
const handleDrop = (e) => {
  const files = e.dataTransfer?.files
  if (!files?.length) return

  for (const file of files) {
    if (file.type.startsWith('image/')) {
      handleImageFile(file)
      break
    }
  }
}
// 统一处理图片文件
const handleImageFile = async (file) => {
  if (!file) return null
  selectedFile.value=true;
  const maxSize = 5 * 1024 * 1024
  if (file.size > maxSize) {
    ElMessage.error('图片大小不能超过 5MB')
    return null
  }

  return new Promise((resolve) => {
    const reader = new FileReader()
    reader.onload = async (e) => {
      imageUrl.value = e.target.result

      try {
        const formData = new FormData()
        formData.append('file', file)

        const response = await request.post('/api/Home/UploadImage', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        })

        if (response.data) {
          ElMessage.success('图片上传成功')
          imageUrl.value = response.data.url
          selectedFile.value= false
          resolve(response.data.url)
        }
      } catch (error) {
        console.error('上传失败:', error)
        ElMessage.error('图片上传失败')
        resolve(null)
      }
    }
    reader.readAsDataURL(file)
  })
}

const selectedFile = ref(false)
const handleImageChange = (file) => {
  if (file.raw) {
    handleImageFile(file.raw)
  }
}
const removeImage = () => {
  imageUrl.value = ''
}
</script>

<style scoped>
.search-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 120px;
  width: 100%;
  max-width: 800px;
  margin: 0 auto;
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.logo img {
  width: 40px;
  height: 40px;
}

.title {
  font-size: 24px;
  font-weight: bold;
  color: var(--el-text-color-primary);
}

.subtitle {
  color: var(--el-text-color-secondary);
  margin-bottom: 32px;
}

.search-box {
  width: 100%;
}

/* 工具栏样式 */
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 24px;
}

.dropdown-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 8px 12px;
  border-radius: 6px;
  transition: background-color 0.3s;
}

.dropdown-trigger:hover {
  background-color: var(--el-fill-color-light);
}

.engine-logo {
  width: 20px;
  height: 20px;
  object-fit: contain;
}

.engine-name {
  color: var(--el-text-color-primary);
  font-size: 14px;
}

.search-engine-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

.image-upload {
  display: flex;
  align-items: center;
  gap: 16px;
}

.image-preview {
  position: relative;
  display: inline-block;
}

.preview-image {
  width: 100px;
  height: 100px;
  object-fit: cover;
  border-radius: 8px;
}

.remove-button {
  position: absolute;
  top: -10px;
  right: -10px;
  padding: 6px;
}

.upload-container {
  display: inline-block;
}

:deep(.el-dropdown-menu) {
  padding: 4px;
}

:deep(.el-dropdown-menu__item) {
  padding: 8px 16px;
}

:deep(.el-dropdown-menu__item:not(.is-disabled):hover) {
  background-color: var(--el-fill-color-light);
  color: var(--el-text-color-primary);
}

:deep(.el-textarea__inner) {
  border-radius: 8px;
  font-size: 16px;
  padding: 12px;
  scrollbar-width: none;
  -ms-overflow-style: none;
  position: relative;

  &::after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: var(--el-fill-color-lighter);
    opacity: 0;
    transition: opacity 0.3s;
    pointer-events: none;
  }

  &:drop {
    &::after {
      opacity: 0.5;
    }
  }
}

:deep(.el-textarea__inner)::-webkit-scrollbar {
  display: none;
}

@media (max-width: 768px) {
  .search-container {
    padding: 60px 20px;
  }

  .preview-image {
    width: 80px;
    height: 80px;
  }

  .toolbar {
    flex-direction: row;
    align-items: center;
  }
}

.mode-switch {
  margin: 20px auto;
  display: flex;
  justify-content: center;
}

.switch-container {
  background-color: var(--el-fill-color-light);
  border-radius: 25px;
  padding: 4px;
  display: flex;
  position: relative;
  cursor: pointer;
  width: 200px;
  height: 40px;
  border: 1px solid var(--el-border-color);
}

.switch-option {
  flex: 1;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1;
  color: var(--el-text-color-regular);
  transition: color 0.3s;
  user-select: none;
  font-size: 16px;
  font-weight: bold;
}

.switch-option.active {
  color: var(--el-color-white);
  font-weight: 500;
}

.switch-background {
  position: absolute;
  width: calc(50% - 4px); /* 修改：调整宽度确保不会超出 */
  height: calc(100% - 8px);
  background-color: var(--el-color-primary);
  border-radius: 20px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  left: 4px;
  top: 4px;
}

.switch-background.right {
  transform: translateX(100%); /* 修改：恢复为100% */
}
/* 深色模式特定样式 */
:root.dark .switch-container {
  background-color: var(--el-fill-color-dark);
}

:root.dark .switch-option {
  color: var(--el-text-color-secondary);
}

:root.dark .switch-option.active {
  color: var(--el-color-white);
}

/* hover效果 */
.switch-container:hover {
  background-color: var(--el-fill-color);
}

</style>