<template>
  <el-config-provider :size="'default'">
    <el-container>
      <el-header class="header">
        <div class="brand">SitePatrol</div>
        <div class="info">A project of CEE-247C @ Stanford University</div>
      </el-header>
      <el-main style="padding: 0">
        <Home />
      </el-main>
      <el-footer class="footer"
        >SitePatrol, Stanford University CEE 247C winter 2025, course project.
      </el-footer>
    </el-container>
  </el-config-provider>
</template>

<script setup lang="ts">
import { ElConfigProvider } from "element-plus";
import Home from "./components/Home.vue";
</script>

<style>
html,
body {
  font-family:
    Inter, "Helvetica Neue", Helvetica, "PingFang SC", "Hiragino Sans GB",
    "Microsoft YaHei", "微软雅黑", Arial, sans-serif;
  margin: 0;
  padding: 0;
}
</style>

<style scoped>
.header {
  background-color: #333;
  color: white;
  display: flex;
  flex-direction: row;
  align-items: center;
}

.header .brand {
  font-weight: bold;
  font-size: 1.25em;
  margin-right: 20px;
}

.header .info {
  opacity: 0.6;
}

.footer {
  text-align: center;
  opacity: 0.6;
  font-size: 0.8em;
}
</style>
