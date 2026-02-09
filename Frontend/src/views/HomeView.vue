<script setup lang="ts">
import { ref, onMounted } from 'vue'

const status = ref<'loading' | 'connected' | 'failed'>('loading')
const serverTime = ref('')
const dotnetVersion = ref('')
const errorMessage = ref('')

async function checkConnection() {
  status.value = 'loading'
  try {
    const res = await fetch('/api/health')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    const data = await res.json()
    status.value = 'connected'
    serverTime.value = data.serverTime
    dotnetVersion.value = data.dotnetVersion
  } catch (e: any) {
    status.value = 'failed'
    errorMessage.value = e.message
  }
}

onMounted(checkConnection)
</script>

<template>
  <main class="health-page">
    <h1>VibeCoding</h1>
    <p class="subtitle">前後端串接狀態</p>

    <div class="status-card" :class="status">
      <div class="indicator"></div>

      <template v-if="status === 'loading'">
        <p class="status-text">連線中...</p>
      </template>

      <template v-else-if="status === 'connected'">
        <p class="status-text">串接成功</p>
        <table>
          <tr>
            <td class="label">後端狀態</td>
            <td>正常運作</td>
          </tr>
          <tr>
            <td class="label">伺服器時間</td>
            <td>{{ serverTime }}</td>
          </tr>
          <tr>
            <td class="label">執行環境</td>
            <td>{{ dotnetVersion }}</td>
          </tr>
        </table>
      </template>

      <template v-else>
        <p class="status-text">連線失敗</p>
        <p class="error">{{ errorMessage }}</p>
        <p class="hint">請確認後端是否已啟動（port 5248）</p>
      </template>
    </div>

    <button @click="checkConnection" class="retry-btn">重新檢查</button>
  </main>
</template>

<style scoped>
.health-page {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem;
}

h1 {
  font-size: 2rem;
  margin-bottom: 0.25rem;
}

.subtitle {
  color: #888;
  margin-bottom: 2rem;
}

.status-card {
  border: 2px solid #ddd;
  border-radius: 12px;
  padding: 2rem;
  min-width: 360px;
  text-align: center;
  transition: border-color 0.3s;
}

.status-card.connected {
  border-color: #4caf50;
}

.status-card.failed {
  border-color: #f44336;
}

.indicator {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  margin: 0 auto 1rem;
  background: #ccc;
}

.connected .indicator {
  background: #4caf50;
  box-shadow: 0 0 8px #4caf5088;
}

.failed .indicator {
  background: #f44336;
  box-shadow: 0 0 8px #f4433688;
}

.loading .indicator {
  background: #ff9800;
  animation: pulse 1s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.3; }
}

.status-text {
  font-size: 1.25rem;
  font-weight: bold;
  margin-bottom: 1rem;
}

table {
  margin: 0 auto;
  text-align: left;
  border-collapse: collapse;
}

td {
  padding: 0.4rem 0.75rem;
}

.label {
  color: #888;
  font-weight: 500;
}

.error {
  color: #f44336;
  font-family: monospace;
}

.hint {
  color: #888;
  font-size: 0.875rem;
  margin-top: 0.5rem;
}

.retry-btn {
  margin-top: 1.5rem;
  padding: 0.5rem 1.5rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  background: white;
  cursor: pointer;
  font-size: 0.875rem;
}

.retry-btn:hover {
  background: #f5f5f5;
}
</style>
