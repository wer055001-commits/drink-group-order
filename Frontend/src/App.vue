<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app-container">
    <header class="app-header">
      <RouterLink to="/" class="app-title">
        <span class="title-icon">🧋</span>
        <h1>團購飲料</h1>
      </RouterLink>
      <nav>
        <RouterLink to="/" class="nav-link">首頁</RouterLink>
        <RouterLink to="/create" class="nav-link nav-create">開新團</RouterLink>
      </nav>
    </header>
    <main class="app-main">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.app-container {
  min-height: 100vh;
}

.app-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 20px;
  background: var(--color-card-bg);
  border-bottom: 1px solid var(--color-card-border);
  position: sticky;
  top: 0;
  z-index: 100;
}

.app-title {
  display: flex;
  align-items: center;
  gap: 8px;
  text-decoration: none;
  color: var(--color-heading);
}

.app-title h1 {
  font-size: 20px;
  font-weight: 700;
}

.title-icon {
  font-size: 28px;
}

nav {
  display: flex;
  gap: 8px;
}

.nav-link {
  padding: 6px 16px;
  border-radius: var(--border-radius-sm);
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
  text-decoration: none;
  transition: all 0.2s;
}

.nav-link:hover {
  background: var(--color-background-soft);
}

.nav-link.router-link-exact-active {
  color: var(--color-primary);
  background: var(--color-primary-bg);
}

.nav-create {
  background: var(--color-accent);
  color: white !important;
}

.nav-create:hover {
  background: var(--color-accent-hover);
}

.nav-create.router-link-exact-active {
  background: var(--color-accent);
  color: white !important;
}

.app-main {
  padding: 20px;
}
</style>
