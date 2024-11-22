<script setup>
import SideMenu from "@/components/SideMenu.vue";
import Header from "@/components/Header.vue";
import LogoScroll from "@/components/LogoScroll.vue";

</script>

<template>
  <Header />
  <main>
    <SideMenu />
    <div class="wrapper">
      <router-view></router-view>
      <LogoScroll />
    </div>
  </main>
</template>
<style>
.wrapper {
  margin-left: 64px;
  padding: 20px;
  transition: margin-left 0.3s;
}

/* 当菜单展开时 */
.el-menu:not(.el-menu--collapse) + .wrapper {
  margin-left: 200px;
}
:root[data-theme='dark'] {
  --el-bg-color: #1d1e1f;
  --el-text-color-primary: #ffffff;
}

:root[data-theme='light'] {
  --el-bg-color: #ffffff;
  --el-text-color-primary: #303133;

}
</style>