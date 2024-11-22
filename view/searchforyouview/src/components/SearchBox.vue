<template>
  <div class="search-container">
    <div class="logo">
      <img :src="logoSrc" alt="Logo" />
      <h1 class="title">Search For You</h1>
    </div>
    <div class="subtitle">🤔想搜点什么？</div>
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
</template>

<script setup>
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
const searchEngine = ref('google')

const searchEngines = [
  {
    label: 'Google',
    value: 'google',
    logo: googleLogo
  },
  {
    label: 'Bing',
    value: 'bing',
    logo: bingLogo
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

const handleEnter = (e) => {
  if (e.shiftKey) {
    return
  }
  e.preventDefault()

  // 构建搜索参数
  const searchParams = {
    q: searchText.value,
    engine: searchEngine.value,
    image: imageUrl.value
  }

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
const handleImageFile = (file) => {
  if (!file) return

  // 验证文件大小（例如限制为 5MB）
  const maxSize = 5 * 1024 * 1024 // 5MB
  if (file.size > maxSize) {
    ElMessage.error('图片大小不能超过 5MB')
    return
  }

  const reader = new FileReader()
  reader.onload = (e) => {
    imageUrl.value = e.target.result
  }
  reader.readAsDataURL(file)
}

const handleImageChange = (file) => {
  handleImageFile(file.raw)
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
</style>