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
    <el-skeleton v-if="loadingIntent && !hasStartedReceiving" animated>
      <template #template>
        <el-skeleton-item variant="h3" style="width: 30%" />
        <el-skeleton-item variant="p" style="width: 100%" />
        <el-skeleton-item variant="p" style="width: 80%" />
      </template>
    </el-skeleton>

    <div v-show="!loadingIntent || hasStartedReceiving" class="content-card intent-analysis">
      <h3>搜索意图分析</h3>
      <p>搜索内容: {{ searchText }}</p>
      <p>搜索引擎: {{ searchEngine }}</p>
      <p>用户可能的意图: </p>
      <div class="markdown-content" v-html="renderedContent"></div>
    </div>

    <!-- AI回答部分 -->
    <el-skeleton v-if="loadingAiResponse && !hasStartedReceivingai" animated>
      <template #template>
        <el-skeleton-item variant="h3" style="width: 20%" />
        <el-skeleton-item variant="p" style="width: 100%" />
        <el-skeleton-item variant="p" style="width: 90%" />
        <el-skeleton-item variant="p" style="width: 80%" />
      </template>
    </el-skeleton>

    <div v-if="!loadingAiResponse || hasStartedReceivingai" class="content-card ai-answer">
      <h3>AI回答</h3>
      <div class="markdown-content" v-html="renderedAiResponse"></div>
    </div>

    <!-- 图片部分 -->
    <el-skeleton v-if="imageUrl" :loading="loadingImage" animated>
      <template #template>
        <div class="image-grid">
          <el-skeleton-item v-for="(_, index) in references"
                            :key="index"
                            variant="image"
                            style="width: 240px; height: 240px" />
        </div>
      </template>
      <template #default>
        <div class="content-card image-section">
          <h3>相关图片</h3>
          <div class="image-grid">
            <el-image
                v-for="(item, index) in references"
                :key="index"
                :src="item.url"
                :preview-src-list="references.map(ref => ref.url)"
                fit="cover"
                class="search-image"
                @error="handleImageError"
            >
              <template #error>
                <div class="image-error">
                  <el-icon><picture-filled /></el-icon>
                  <span>加载失败</span>
                </div>
              </template>
            </el-image>
          </div>
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
import request from '@/utils/request'
import { ref, onMounted,computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ElButton,
  ElSkeleton,
  ElSkeletonItem,
  ElIcon,
  ElLink,
  ElMessage,
} from 'element-plus'
import { ArrowLeft } from '@element-plus/icons-vue'
import { useDark } from '@vueuse/core'
import MarkdownIt from 'markdown-it'
import 'highlight.js/styles/github.css'
import hljs from 'highlight.js'
import texmath from 'markdown-it-texmath'
import katex from 'katex'
const md = new MarkdownIt({
  html: true,
  linkify: true,
  typographer: true,
  highlight: function (str, lang) {
    if (lang && hljs.getLanguage(lang)) {
      try {
        return '<pre class="hljs"><code>' +
            hljs.highlight(str, { language: lang, ignoreIllegals: true }).value +
            '</code></pre>';
      } catch (__) {}
    }
    return '<pre class="hljs"><code>' + md.utils.escapeHtml(str) + '</code></pre>';
  }
})

md.use(texmath, {
  engine: katex,
  delimiters: ['dollars', 'brackets'], // 同时支持 $ 和 [ 作为分隔符
  katexOptions: {
    macros: {
      // 基本运算符
      '\\defeq': ':=',
      '\\bm': '\\boldsymbol',

      // 集合论
      '\\set': '\\left\\{ #1 \\right\\}', // \set{x}
      '\\N': '\\mathbb{N}',
      '\\Z': '\\mathbb{Z}',
      '\\Q': '\\mathbb{Q}',
      '\\R': '\\mathbb{R}',
      '\\C': '\\mathbb{C}',

      // 线性代数
      '\\mat': '\\begin{matrix} #1 \\end{matrix}', // \mat{...}
      '\\vec': '\\mathbf{#1}', // \vec{v}
      '\\det': '\\operatorname{det}',
      '\\tr': '\\operatorname{tr}',

      // 微积分
      '\\d': '\\mathrm{d}',
      '\\diff': '\\frac{\\d}{\\d #1}', // \diff{x}
      '\\pd': '\\frac{\\partial}{\\partial #1}', // \pd{x}

      // 概率论
      '\\P': '\\operatorname{P}',
      '\\E': '\\operatorname{E}',
      '\\Var': '\\operatorname{Var}',
      '\\Cov': '\\operatorname{Cov}',

      // 函数和极限
      '\\lim': '\\operatorname{lim}',
      '\\sup': '\\operatorname{sup}',
      '\\inf': '\\operatorname{inf}',
      '\\max': '\\operatorname{max}',
      '\\min': '\\operatorname{min}',

      // 三角函数
      '\\sin': '\\operatorname{sin}',
      '\\cos': '\\operatorname{cos}',
      '\\tan': '\\operatorname{tan}',
      '\\csc': '\\operatorname{csc}',
      '\\sec': '\\operatorname{sec}',
      '\\cot': '\\operatorname{cot}',

      // 双曲函数
      '\\sinh': '\\operatorname{sinh}',
      '\\cosh': '\\operatorname{cosh}',
      '\\tanh': '\\operatorname{tanh}',

      // 对数函数
      '\\log': '\\operatorname{log}',
      '\\ln': '\\operatorname{ln}',
      '\\lg': '\\operatorname{lg}',

      // 特殊函数
      '\\exp': '\\operatorname{exp}',
      '\\sgn': '\\operatorname{sgn}',

      // 复分析
      '\\Re': '\\operatorname{Re}',
      '\\Im': '\\operatorname{Im}',
      '\\arg': '\\operatorname{arg}',

      // 向量分析
      '\\grad': '\\operatorname{grad}',
      '\\div': '\\operatorname{div}',
      '\\rot': '\\operatorname{rot}',
      '\\curl': '\\operatorname{curl}',

      // 常用箭头
      '\\ra': '\\rightarrow',
      '\\Ra': '\\Rightarrow',
      '\\la': '\\leftarrow',
      '\\La': '\\Leftarrow',
      '\\lra': '\\leftrightarrow',
      '\\Lra': '\\Leftrightarrow',

      // 其他常用符号
      '\\eps': '\\varepsilon',
      '\\phi': '\\varphi',
      '\\ell': '\\ell',

      // 矩阵简写
      '\\pmatrix': '\\begin{pmatrix} #1 \\end{pmatrix}',
      '\\bmatrix': '\\begin{bmatrix} #1 \\end{bmatrix}',
      '\\vmatrix': '\\begin{vmatrix} #1 \\end{vmatrix}',

      // 定界符
      '\\abs': '\\left|#1\\right|',
      '\\norm': '\\left\\|#1\\right\\|',
      '\\ceil': '\\left\\lceil#1\\right\\rceil',
      '\\floor': '\\left\\lfloor#1\\right\\rfloor',

      // 求和、积分等
      '\\sum': '\\sum\\limits',
      '\\prod': '\\prod\\limits',
      '\\lim': '\\lim\\limits',

      // 自定义环境
      '\\cases': '\\begin{cases} #1 \\end{cases}',
      '\\align': '\\begin{align} #1 \\end{align}',
    },
    throwOnError: false, // 防止渲染错误导致整个公式失败
    errorColor: '#cc0000', // 错误时显示红色
    strict: false // 不要太严格的语法检查
  }
})
const isDark = useDark()
const router = useRouter()

// 分别控制各部分的loading状态
const loadingReferences = ref(true)

const searchText = ref('')
const searchEngine = ref('')
const imageUrl = ref('')
const reading = ref(false)
const references = ref([])
const loadingIntent = ref(true)
const hasStartedReceiving = ref(false)
const receivedContent = ref('') // 存储完整的 markdown 文本
const renderedContent = computed(() => {
  return md.render(receivedContent.value)
})
// 返回搜索页面
const goBack = () => {
  router.push('/search')
}
//生成搜索关键词
const getSearchKeywords = async (question, intent) => {
  try {
    const response = await request.post('/api/Home/CreateSearchKeywords', {
      question,
      intant: intent // 注意这里使用完整的意图字符串
    });

    if (response.success) {
      return response.data; // 返回关键词字符串
    } else {
      throw new Error(response.msg || '获取搜索关键词失败');
    }
  } catch (error) {
    console.error('获取搜索关键词出错:', error);
    return null;
  }
}
//搜索意图分析
const fetchIntentAnalysis = () => {
  loadingIntent.value = true;
  hasStartedReceiving.value = false;
  receivedContent.value = '';

  const xhr = new XMLHttpRequest();
  xhr.open('POST', request.defaults.baseURL + '/api/Home/GetUserIntent');
  xhr.setRequestHeader('Content-Type', 'application/json');
  xhr.responseType = 'text';

  xhr.onprogress = (event) => {
    const newData = xhr.responseText.slice(xhr.seenBytes || 0);
    xhr.seenBytes = xhr.responseText.length;

    const lines = newData.split('\n');
    for (const line of lines) {
      if (line.trim()) {
        if (line.startsWith('data: ')) {
          try {
            const jsonData = JSON.parse(line.slice(6));
            if (jsonData.choices?.[0]?.delta?.content) {
              if (!hasStartedReceiving.value) {
                hasStartedReceiving.value = true;
              }
              receivedContent.value += jsonData.choices[0].delta.content;
            }
          } catch (e) {}
        }
      }
    }
  }

  xhr.onload = async () => {
    loadingIntent.value = false;

    // 意图分析完成后,调用获取搜索关键词API
    if (receivedContent.value) {
      const keywords = await getSearchKeywords(searchText.value, receivedContent.value);
      if (keywords) {
        await fetchReferences(keywords);
      }
    }
  }

  xhr.send(JSON.stringify({
    question: searchText.value,
    imageUrl: imageUrl.value
  }));
}

// 响应式变量
const loadingAiResponse = ref(true)
const hasStartedReceivingai = ref(false)
const aiResponse = ref('') // 存储完整的 markdown 文本
const renderedAiResponse = computed(() => {
  return md.render(aiResponse.value)
})
// 获取AI回复
const fetchAiResponse = () => {
  // 重置所有状态
  loadingAiResponse.value = true
  hasStartedReceivingai.value = false
  aiResponse.value = ''

  const xhr = new XMLHttpRequest()
  xhr.open('POST', request.defaults.baseURL + '/api/Home/GetAIResultByWeb')
  xhr.setRequestHeader('Content-Type', 'application/json')
  xhr.responseType = 'text'

  xhr.onprogress = (event) => {
    const newData = xhr.responseText.slice(xhr.seenBytes || 0)
    xhr.seenBytes = xhr.responseText.length

    const lines = newData.split('\n')
    for (const line of lines) {
      if (line.trim()) {
        if (line.startsWith('data: ')) {
          try {
            const jsonData = JSON.parse(line.slice(6))
            if (jsonData.choices?.[0]?.delta?.content) {
              hasStartedReceivingai.value = true // 收到内容就设置为true
              aiResponse.value += jsonData.choices[0].delta.content
            }
          } catch (e) {
            console.error('Parse error:', e)
          }
        }
      }
    }
  }

  xhr.onload = () => {
    loadingAiResponse.value = false
  }

  xhr.onerror = (error) => {
    console.error('Request failed:', error)
    loadingAiResponse.value = false
  }

  const params = {
    searchEngineResultList: references.value,
    question: searchText.value,
    intant: receivedContent.value,
    imageUrl: imageUrl.value || '',
    reading: reading.value|| false
  }

  xhr.send(JSON.stringify(params))
}
// 图片加载
const loadingImage = ref(true)
const fetchImage = () => {
  setTimeout(() => {
    loadingImage.value = false
  }, 1000)
}
const handleImageError = () => {
  //ElMessage.warning('图片加载失败')
}
// 搜索结果
const fetchReferences = async (keywords) => {
  try {
    loadingReferences.value = true;

    const params = {
      keyword: keywords,
      imageUrl: imageUrl.value || '',
      engine: searchEngine.value
    };

    const response = await request.post('/api/Home/GetSearchEngineResult', params);

    // 检查response.data是否存在
    if (!response.data) {
      throw new Error('Response data is undefined');
    }

    // 验证返回数据格式
    if (!Array.isArray(response.data)) {
      ElMessage.error('非数组响应:', response.data);
      // 如果response.data不是数组,但可能是嵌套在其他字段中
      const resultsData = response.data.data || response.data.results || response.data;

      if (Array.isArray(resultsData)) {
        references.value = resultsData.map(item => ({
          title: item.title || '',
          url: item.url || '',
          snippet: item.snippet || ''
        }));
        return;
      }
      throw new Error('Response data is not in expected format');
    }

    // 正常数组数据处理
    references.value = response.data.map(item => ({
      title: item.title || '',
      url: item.url || '',
      snippet: item.snippet || ''
    }));
    if (references.value.length > 0) {
      fetchAiResponse();
    }
    if (imageUrl.value) {
      await fetchImage();
    }
  } catch (error) {
    ElMessage.error('搜索引擎错误:'+error.message);

    references.value = []; // 清空结果
  } finally {
    loadingReferences.value = false;
  }
};
onMounted(() => {
  const urlParams = new URLSearchParams(window.location.search)

  searchText.value = urlParams.get('q') || ''
  searchEngine.value = urlParams.get('engine') || ''
  imageUrl.value = urlParams.get('image') || ''
  reading.value = urlParams.get('reading') === 'true'|| false
  if(searchText.value || imageUrl.value) {
    fetchIntentAnalysis()
  }
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

.streaming-content {
  white-space: pre-wrap;
  min-height: 100px;
  max-height: 500px;
  overflow-y: auto;
  padding: 10px;
  line-height: 1.6;
}

.char-animation {
  display: inline-block;
  animation: fadeIn 0.1s ease-in;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(5px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 处理换行符的显示 */
.char-animation:has(+ .char-animation[data-char="\n"]),
.char-animation[data-char="\n"] {
  display: block;
}
.streaming-content {
  margin-top: 10px;
}

.char-animation {
  opacity: 0;
  animation: fadeIn 0.3s forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
.markdown-content {
  margin-top: 15px;
}

.markdown-content :deep(pre) {
  background-color: #f6f8fa;
  border-radius: 6px;
  padding: 16px;
  overflow: auto;
}

.markdown-content :deep(code) {
  font-family: ui-monospace,SFMono-Regular,SF Mono,Menlo,Consolas,Liberation Mono,monospace;
  font-size: 85%;
}

.markdown-content :deep(p) {
  margin: 10px 0;
  line-height: 1.6;
}

.markdown-content :deep(ul),
.markdown-content :deep(ol) {
  padding-left: 20px;
}

.markdown-content :deep(h1),
.markdown-content :deep(h2),
.markdown-content :deep(h3),
.markdown-content :deep(h4),
.markdown-content :deep(h5),
.markdown-content :deep(h6) {
  margin: 16px 0 8px;
}
.ai-answer {
  margin: 20px 0;
}
.image-section {
  margin: 20px 0;
}

.search-image {
  width: 240px;
  height: 240px;
  border-radius: 8px;
}

.image-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #909399;
  font-size: 14px;
}
</style>
