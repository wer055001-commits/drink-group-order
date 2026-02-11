<script setup lang="ts">
import type { GroupOrderListItem } from '@/types'
import DeadlineCountdown from './DeadlineCountdown.vue'

defineProps<{ order: GroupOrderListItem }>()

function statusClass(status: string) {
  if (status === '開放中') return 'status-open'
  if (status === '已截止') return 'status-closed'
  return 'status-done'
}
</script>

<template>
  <RouterLink :to="`/order/${order.id}`" class="order-card card">
    <div class="order-card-header">
      <span class="shop-name">{{ order.shopName }}</span>
      <span :class="['status-badge', statusClass(order.status)]">{{ order.status }}</span>
    </div>
    <div v-if="order.title" class="order-title">{{ order.title }}</div>
    <div class="order-meta">
      <span>{{ order.creatorName }} 開的團</span>
      <DeadlineCountdown :deadline="order.deadline" />
    </div>
    <div class="order-stats">
      <span>{{ order.itemCount }} 人</span>
      <span>{{ order.totalCups }} 杯</span>
      <span class="total-price">${{ order.totalPrice }}</span>
    </div>
  </RouterLink>
</template>

<style scoped>
.order-card {
  display: block;
  text-decoration: none;
  color: inherit;
  transition: box-shadow 0.2s, transform 0.2s;
  cursor: pointer;
}
.order-card:hover {
  box-shadow: var(--shadow-card-hover);
  transform: translateY(-2px);
}
.order-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}
.shop-name {
  font-size: 18px;
  font-weight: 700;
  color: var(--color-heading);
}
.order-title {
  font-size: 14px;
  color: var(--color-text-light);
  margin-bottom: 8px;
}
.order-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 13px;
  color: var(--color-text-light);
  margin-bottom: 12px;
}
.order-stats {
  display: flex;
  gap: 16px;
  font-size: 14px;
  color: var(--color-text);
  padding-top: 10px;
  border-top: 1px solid var(--color-card-border);
}
.total-price {
  margin-left: auto;
  font-weight: 700;
  color: var(--color-primary);
}
</style>
