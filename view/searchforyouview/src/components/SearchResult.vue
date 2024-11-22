<template>
  <div class="search-result" :class="{ 'dark-mode': isDark }">
    <!-- 头部返回按钮 -->
    <header class="header">
      <el-button @click="goBack">
        <el-icon><ArrowLeft /></el-icon>
        返回搜索
      </el-button>
    </header>

    <!-- 搜索意图分析部分 -->
    <el-skeleton :loading="loadingIntent" animated>
      <template #template>
        <el-skeleton-item variant="h3" style="width: 30%" />
        <el-skeleton-item variant="p" style="width: 100%" />
        <el-skeleton-item variant="p" style="width: 80%" />
      </template>
      <template #default>
        <div class="content-card intent-analysis">
          <h3>搜索意图分析</h3>
          <p>搜索内容: {{ searchText }}</p>
          <p>搜索引擎: {{ searchEngine }}</p>
          <p>意图分析: {{ intentAnalysis }}</p>
        </div>
      </template>
    </el-skeleton>

    <!-- AI回答部分 -->
    <el-skeleton :loading="loadingAiResponse" animated>
      <template #template>
        <el-skeleton-item variant="h3" style="width: 20%" />
        <el-skeleton-item variant="p" style="width: 100%" />
        <el-skeleton-item variant="p" style="width: 90%" />
        <el-skeleton-item variant="p" style="width: 80%" />
      </template>
      <template #default>
        <div class="content-card ai-answer">
          <h3>AI回答</h3>
          <p>{{ aiResponse }}</p>
        </div>
      </template>
    </el-skeleton>

    <!-- 图片部分 -->
    <el-skeleton v-if="imageUrl" :loading="loadingImage" animated>
      <template #template>
        <el-skeleton-item variant="image" style="width: 240px; height: 240px" />
      </template>
      <template #default>
        <div class="content-card image-section">
          <h3>相关图片</h3>
          <img :src="imageUrl" alt="搜索图片" class="search-image" />
        </div>
      </template>
    </el-skeleton>

    <!-- 引用来源链接部分 -->
    <el-skeleton :loading="loadingReferences" animated>
      <template #template>
        <el-skeleton-item variant="h3" style="width: 25%" />
        <div style="padding: 10px 0">
          <el-skeleton-item variant="text" style="width: 70%; margin-bottom: 10px" />
          <el-skeleton-item variant="text" style="width: 60%; margin-bottom: 10px" />
          <el-skeleton-item variant="text" style="width: 65%" />
        </div>
      </template>
      <template #default>
        <div class="content-card references">
          <h3>参考来源</h3>
          <ul>
            <li v-for="(ref, index) in references" :key="index">
              <el-link :href="ref.url" target="_blank" type="primary">
                {{ ref.title }}
              </el-link>
            </li>
          </ul>
        </div>
      </template>
    </el-skeleton>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ElButton,
  ElSkeleton,
  ElSkeletonItem,
  ElIcon,
  ElLink
} from 'element-plus'
import { ArrowLeft } from '@element-plus/icons-vue'
import { useDark } from '@vueuse/core'

const isDark = useDark()
const route = useRoute()
const router = useRouter()

// 分别控制各部分的loading状态
const loadingIntent = ref(true)
const loadingAiResponse = ref(true)
const loadingImage = ref(true)
const loadingReferences = ref(true)

const searchText = ref('')
const searchEngine = ref('')
const imageUrl = ref('')
const aiResponse = ref('')
const intentAnalysis = ref('')
const references = ref([])

// 返回搜索页面
const goBack = () => {
  router.push('/search')
}

// 模拟获取不同部分的数据
const fetchIntentAnalysis = () => {
  setTimeout(() => {
    intentAnalysis.value = '用户想要了解...'
    loadingIntent.value = false
  }, 1000)
}

const fetchAiResponse = () => {
  setTimeout(() => {
    aiResponse.value = '这是AI的详细回答...'
    loadingAiResponse.value = false
  }, 2000)
}

const fetchImage = () => {
  setTimeout(() => {
    loadingImage.value = false
  }, 1500)
}

const fetchReferences = () => {
  setTimeout(() => {
    references.value = [
      { title: '参考来源1 - 详细标题', url: 'https://example1.com' },
      { title: '参考来源2 - 详细标题', url: 'https://example2.com' },
      { title: '参考来源3 - 详细标题', url: 'https://example3.com' },
    ]
    loadingReferences.value = false
  }, 2500)
}

onMounted(() => {
  // 从路由参数中获取搜索信息
  const { query } = route
  searchText.value = query.q || ''
  searchEngine.value = query.engine || ''
  imageUrl.value = query.image || ''

  // 分别加载各部分数据
  fetchIntentAnalysis()
  fetchAiResponse()
  fetchImage()
  fetchReferences()
})
</script>

<style scoped>
.search-result {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
  transition: all 0.3s ease;
}

.content-card {
  margin-bottom: 30px;
  padding: 20px;
  border-radius: 8px;
  transition: all 0.3s ease;
}

/* 浅色模式样式 */
.content-card {
  background-color: #f5f7fa;
}

/* 深色模式样式 */
.dark-mode {
  color: var(--el-text-color-primary);
}

.dark-mode .content-card {
  background-color: var(--el-bg-color-overlay);
  border: 1px solid var(--el-border-color-lighter);
}

.dark-mode h3 {
  color: var(--el-text-color-primary);
}

.dark-mode p {
  color: var(--el-text-color-regular);
}

.header {
  margin-bottom: 20px;
}

.search-image {
  max-width: 240px;
  max-height: 240px;
  border-radius: 8px;
  object-fit: cover;
}

.references ul {
  list-style: none;
  padding: 0;
}

.references li {
  margin-bottom: 12px;
}

h3 {
  margin-top: 0;
  margin-bottom: 16px;
  transition: color 0.3s ease;
}

/* 骨架屏样式 */
:deep(.el-skeleton) {
  margin-bottom: 20px;
}

:deep(.el-skeleton__item) {
  margin-bottom: 10px;
}

/* 深色模式下的骨架屏样式 */
.dark-mode :deep(.el-skeleton__item) {
  background: var(--el-fill-color-darker);
}

/* 添加过渡效果 */
.intent-analysis,
.ai-answer,
.image-section,
.references,
p,
h3 {
  transition: all 0.3s ease;
}

/* 深色模式下的链接样式 */
.dark-mode :deep(.el-link) {
  color: var(--el-color-primary);
}

.dark-mode :deep(.el-link:hover) {
  color: var(--el-color-primary-light-3);
}

/* 深色模式下的按钮样式 */
.dark-mode :deep(.el-button) {
  background-color: var(--el-bg-color-overlay);
  border-color: var(--el-border-color);
  color: var(--el-text-color-primary);
}

.dark-mode :deep(.el-button:hover) {
  background-color: var(--el-fill-color-darker);
  border-color: var(--el-border-color-darker);
}
</style>
