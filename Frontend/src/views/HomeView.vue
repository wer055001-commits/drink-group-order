<script setup lang="ts">
import { ref, onMounted } from 'vue'

// ── 連線狀態 ──
const connectionStatus = ref<'loading' | 'connected' | 'failed'>('loading')
const serverTime = ref('')
const dotnetVersion = ref('')
const errorMessage = ref('')

// ── 待辦清單 ──
const todos = ref<{ id: number; title: string; isDone: boolean; createdAt: string }[]>([])
const newTitle = ref('')

async function checkConnection() {
  connectionStatus.value = 'loading'
  try {
    const res = await fetch('/api/health')
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    const data = await res.json()
    connectionStatus.value = 'connected'
    serverTime.value = data.serverTime
    dotnetVersion.value = data.dotnetVersion
    await loadTodos()
  } catch (e: any) {
    connectionStatus.value = 'failed'
    errorMessage.value = e.message
  }
}

async function loadTodos() {
  const res = await fetch('/api/todos')
  todos.value = await res.json()
}

async function addTodo() {
  const title = newTitle.value.trim()
  if (!title) return
  await fetch('/api/todos', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ title, isDone: false })
  })
  newTitle.value = ''
  await loadTodos()
}

async function toggleTodo(todo: (typeof todos.value)[0]) {
  await fetch(`/api/todos/${todo.id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ...todo, isDone: !todo.isDone })
  })
  await loadTodos()
}

async function deleteTodo(id: number) {
  await fetch(`/api/todos/${id}`, { method: 'DELETE' })
  await loadTodos()
}

onMounted(checkConnection)
</script>

<template>
  <main class="page">
    <h1>VibeCoding</h1>

    <!-- 連線狀態卡片 -->
    <div class="status-card" :class="connectionStatus">
      <div class="indicator"></div>
      <template v-if="connectionStatus === 'loading'">
        <p class="status-text">連線中...</p>
      </template>
      <template v-else-if="connectionStatus === 'connected'">
        <p class="status-text">前後端 + 資料庫 串接成功</p>
        <div class="status-details">
          <span>伺服器時間：{{ serverTime }}</span>
          <span>{{ dotnetVersion }}</span>
        </div>
      </template>
      <template v-else>
        <p class="status-text">連線失敗</p>
        <p class="error">{{ errorMessage }}</p>
      </template>
    </div>

    <!-- 待辦清單 -->
    <section v-if="connectionStatus === 'connected'" class="todo-section">
      <h2>待辦清單（資料庫讀寫測試）</h2>

      <form class="add-form" @submit.prevent="addTodo">
        <input
          v-model="newTitle"
          placeholder="輸入新的待辦事項..."
          class="add-input"
        />
        <button type="submit" class="add-btn">新增</button>
      </form>

      <p v-if="todos.length === 0" class="empty">還沒有任何待辦事項，試著新增一個吧！</p>

      <ul class="todo-list">
        <li v-for="todo in todos" :key="todo.id" class="todo-item">
          <label class="todo-label" :class="{ done: todo.isDone }">
            <input
              type="checkbox"
              :checked="todo.isDone"
              @change="toggleTodo(todo)"
            />
            <span>{{ todo.title }}</span>
          </label>
          <button class="delete-btn" @click="deleteTodo(todo.id)">刪除</button>
        </li>
      </ul>
    </section>
  </main>
</template>

<style scoped>
.page {
  max-width: 560px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

h1 {
  text-align: center;
  font-size: 2rem;
  margin-bottom: 1.5rem;
}

/* ── 狀態卡片 ── */

.status-card {
  border: 2px solid #ddd;
  border-radius: 10px;
  padding: 1.25rem;
  text-align: center;
  margin-bottom: 2rem;
  transition: border-color 0.3s;
}

.status-card.connected { border-color: #4caf50; }
.status-card.failed    { border-color: #f44336; }

.indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  margin: 0 auto 0.75rem;
  background: #ccc;
}

.connected .indicator { background: #4caf50; box-shadow: 0 0 6px #4caf5088; }
.failed    .indicator { background: #f44336; box-shadow: 0 0 6px #f4433688; }
.loading   .indicator { background: #ff9800; animation: pulse 1s infinite; }

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.3; }
}

.status-text {
  font-size: 1.1rem;
  font-weight: bold;
  margin-bottom: 0.5rem;
}

.status-details {
  display: flex;
  justify-content: center;
  gap: 1.5rem;
  color: #888;
  font-size: 0.8rem;
}

.error {
  color: #f44336;
  font-family: monospace;
  font-size: 0.875rem;
}

/* ── 待辦清單 ── */

.todo-section {
  margin-top: 1rem;
}

h2 {
  font-size: 1.1rem;
  margin-bottom: 1rem;
  color: #555;
}

.add-form {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.add-input {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.9rem;
}

.add-input:focus {
  outline: none;
  border-color: #4caf50;
}

.add-btn {
  padding: 0.5rem 1rem;
  background: #4caf50;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
}

.add-btn:hover { background: #43a047; }

.empty {
  text-align: center;
  color: #aaa;
  padding: 2rem 0;
}

.todo-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.todo-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.6rem 0;
  border-bottom: 1px solid #f0f0f0;
}

.todo-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-size: 0.95rem;
}

.todo-label.done span {
  text-decoration: line-through;
  color: #aaa;
}

.delete-btn {
  padding: 0.25rem 0.6rem;
  background: none;
  border: 1px solid #ddd;
  border-radius: 4px;
  color: #999;
  cursor: pointer;
  font-size: 0.8rem;
}

.delete-btn:hover {
  border-color: #f44336;
  color: #f44336;
}
</style>
