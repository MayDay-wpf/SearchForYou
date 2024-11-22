<template>
  <header class="header">
    <div class="left">
      <!-- 左侧内容 -->
    </div>
    <div class="right">
      <el-link href="https://github.com/MayDay-wpf/SearchForYou" target="_blank">
        <img src="@/assets/github.svg" alt="" class="github" width="20" />
      </el-link>
      <el-switch
          v-model="isDark"
          inline-prompt
          :active-icon="Moon"
          :inactive-icon="Sunny"
      />
    </div>
  </header>
</template>

<script setup>
import { useDark } from '@vueuse/core'
import { Moon, Sunny } from '@element-plus/icons-vue'

const isDark = useDark({
  selector: 'html',
  attribute: 'class',
  valueDark: 'dark',
  valueLight: ''
})
</script>

<style scoped>
.header {
  height: 60px;
  padding: 0 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  //border-bottom: 1px solid var(--el-border-color);
}

.right {
  display: flex;
  align-items: center;
  gap: 16px;
}
</style>
