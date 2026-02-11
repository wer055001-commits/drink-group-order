<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { GroupOrderDetail } from '@/types'
import OrderSummaryTable from '@/components/OrderSummaryTable.vue'

const route = useRoute()
const router = useRouter()

const order = ref<GroupOrderDetail | null>(null)
const loading = ref(true)
const error = ref('')

const orderId = computed(() => Number(route.params.id))

onMounted(async () => {
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
})

// 產生訂餐文字清單（方便打電話時唸出）
const orderText = computed(() => {
  if (!order.value) return ''
  const lines: string[] = []
  lines.push(`【${order.value.shop.name}】團購訂單`)
  lines.push('')
  for (const person of order.value.summary.byPerson) {
    for (const item of person.items) {
      const parts = [item.menuItemName, item.size, item.sweetLevel, item.iceLevel]
      if (item.toppings.length > 0) parts.push('加' + item.toppings.join('、'))
      if (item.quantity > 1) parts.push(`x${item.quantity}`)
      lines.push(parts.join(' / '))
    }
  }
  lines.push('')
  lines.push(`共 ${order.value.summary.totalCups} 杯，總計 $${order.value.summary.totalPrice}`)
  return lines.join('\n')
})

const copied = ref(false)
function copyOrderText() {
  navigator.clipboard.writeText(orderText.value)
  copied.value = true
  setTimeout(() => { copied.value = false }, 2000)
}
</script>

<template>
  <div class="summary-page">
    <div v-if="loading" class="loading">載入中...</div>
    <div v-else-if="error" class="error-msg">{{ error }}</div>
    <template v-else-if="order">
      <div class="page-header">
        <h2 class="page-title">{{ order.shop.name }} - 訂單總覽</h2>
        <button class="btn btn-sm btn-outline" @click="router.push(`/order/${orderId}`)">返回團購</button>
      </div>

      <OrderSummaryTable
        :by-person="order.summary.byPerson"
        :total-cups="order.summary.totalCups"
        :total-price="order.summary.totalPrice"
      />

      <div class="order-text-section card">
        <h3 class="section-title">訂餐清單（方便打電話用）</h3>
        <pre class="order-text">{{ orderText }}</pre>
        <button class="btn btn-outline" @click="copyOrderText">
          {{ copied ? '已複製！' : '複製文字' }}
        </button>
      </div>
    </template>
  </div>
</template>

<style scoped>
.summary-page {
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
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--color-heading);
}
.order-text-section {
  margin-top: 20px;
}
.section-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 12px;
}
.order-text {
  background: var(--color-background-soft);
  padding: 16px;
  border-radius: var(--border-radius-sm);
  font-size: 14px;
  line-height: 1.8;
  white-space: pre-wrap;
  margin-bottom: 12px;
  font-family: inherit;
}

@media print {
  .page-header button,
  .order-text-section button {
    display: none;
  }
}
</style>
