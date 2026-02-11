<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { GroupOrderDetail } from '@/types'
import { useDrinkOrderStore } from '@/stores/drinkOrder'
import DeadlineCountdown from '@/components/DeadlineCountdown.vue'
import OrderItemCard from '@/components/OrderItemCard.vue'

const route = useRoute()
const router = useRouter()
const store = useDrinkOrderStore()

const order = ref<GroupOrderDetail | null>(null)
const loading = ref(true)
const error = ref('')
const copied = ref(false)

const orderId = computed(() => Number(route.params.id))

const allItems = computed(() => {
  if (!order.value) return []
  return order.value.summary.byPerson.flatMap(p => p.items)
})

const isOpen = computed(() => order.value?.status === '開放中')

async function loadOrder() {
  try {
    const res = await fetch(`/api/group-orders/${orderId.value}`)
    if (!res.ok) {
      error.value = '找不到這筆團購'
      return
    }
    order.value = await res.json()
  } catch {
    error.value = '連線失敗'
  } finally {
    loading.value = false
  }
}

async function updateStatus(status: string) {
  try {
    await fetch(`/api/group-orders/${orderId.value}/status`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status })
    })
    await loadOrder()
  } catch {
    // ignore
  }
}

async function deleteItem(itemId: number) {
  try {
    await fetch(`/api/group-orders/${orderId.value}/items/${itemId}`, {
      method: 'DELETE'
    })
    await loadOrder()
  } catch {
    // ignore
  }
}

function shareLink() {
  navigator.clipboard.writeText(window.location.href)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}

function statusClass(status: string) {
  if (status === '開放中') return 'status-open'
  if (status === '已截止') return 'status-closed'
  return 'status-done'
}

onMounted(() => {
  loadOrder()
})
</script>

<template>
  <div class="detail-page">
    <div v-if="loading" class="loading">載入中...</div>
    <div v-else-if="error" class="error-msg">{{ error }}</div>
    <template v-else-if="order">
      <div class="order-header card">
        <div class="header-top">
          <h2 class="shop-name">{{ order.shop.name }}</h2>
          <span :class="['status-badge', statusClass(order.status)]">{{ order.status }}</span>
        </div>
        <div v-if="order.title" class="order-title">{{ order.title }}</div>
        <div class="header-meta">
          <span>{{ order.creatorName }} 開的團</span>
          <DeadlineCountdown :deadline="order.deadline" />
        </div>
        <div class="header-stats">
          <span>{{ order.summary.totalItems }} 人</span>
          <span>{{ order.summary.totalCups }} 杯</span>
          <span class="total-price">${{ order.summary.totalPrice }}</span>
        </div>
      </div>

      <div class="actions">
        <button v-if="isOpen" class="btn btn-accent" @click="router.push(`/order/${orderId}/add`)">
          我要點餐
        </button>
        <button class="btn btn-outline" @click="router.push(`/order/${orderId}/summary`)">
          查看統計
        </button>
        <button class="btn btn-outline" @click="shareLink">
          {{ copied ? '已複製！' : '分享連結' }}
        </button>
      </div>

      <div v-if="isOpen && order.creatorName === store.userName" class="creator-actions">
        <button class="btn btn-sm btn-outline" @click="updateStatus('已截止')">截止團購</button>
        <button class="btn btn-sm btn-outline" @click="updateStatus('已結單')">直接結單</button>
      </div>
      <div v-else-if="order.status === '已截止' && order.creatorName === store.userName" class="creator-actions">
        <button class="btn btn-sm btn-outline" @click="updateStatus('已結單')">結單</button>
        <button class="btn btn-sm btn-outline" @click="updateStatus('開放中')">重新開放</button>
      </div>

      <section class="section">
        <h3 class="section-title">目前訂單 ({{ allItems.length }} 杯)</h3>
        <div v-if="allItems.length > 0" class="items-list">
          <OrderItemCard
            v-for="item in allItems"
            :key="item.id"
            :item="item"
            :editable="item.personName === store.userName"
            @delete="deleteItem(item.id)"
          />
        </div>
        <div v-else class="empty-items">
          還沒有人點餐，快來當第一個！
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.detail-page {
  max-width: 600px;
  margin: 0 auto;
}
.loading, .error-msg {
  text-align: center;
  padding: 40px;
  color: var(--color-text-light);
}
.error-msg {
  color: var(--color-danger);
}
.order-header {
  margin-bottom: 16px;
}
.header-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 4px;
}
.shop-name {
  font-size: 22px;
  font-weight: 700;
  color: var(--color-heading);
}
.order-title {
  font-size: 14px;
  color: var(--color-text-light);
  margin-bottom: 8px;
}
.header-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 13px;
  color: var(--color-text-light);
  margin-bottom: 12px;
}
.header-stats {
  display: flex;
  gap: 16px;
  font-size: 14px;
  padding-top: 10px;
  border-top: 1px solid var(--color-card-border);
}
.total-price {
  margin-left: auto;
  font-weight: 700;
  color: var(--color-primary);
  font-size: 16px;
}
.actions {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}
.creator-actions {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}
.section {
  margin-bottom: 24px;
}
.section-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 12px;
}
.items-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.empty-items {
  text-align: center;
  padding: 40px;
  color: var(--color-text-light);
  background: var(--color-card-bg);
  border-radius: var(--border-radius);
  border: 1px dashed var(--color-border);
}
</style>
