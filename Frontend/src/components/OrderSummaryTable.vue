<script setup lang="ts">
import type { PersonOrderSummary } from '@/types'

defineProps<{
  byPerson: PersonOrderSummary[]
  totalCups: number
  totalPrice: number
}>()
</script>

<template>
  <div class="summary">
    <div class="summary-header">
      <span>共 {{ totalCups }} 杯</span>
      <span class="summary-total">總計 ${{ totalPrice }}</span>
    </div>

    <div v-for="person in byPerson" :key="person.name" class="person-section">
      <div class="person-header">
        <span class="person-name">{{ person.name }}</span>
        <span class="person-total">${{ person.personTotal }}</span>
      </div>
      <div v-for="item in person.items" :key="item.id" class="person-item">
        <div class="person-item-main">
          <span>{{ item.menuItemName }}</span>
          <span class="item-spec">{{ item.size }} / {{ item.sweetLevel }} / {{ item.iceLevel }}</span>
          <span v-if="item.toppings.length">+ {{ item.toppings.join('、') }}</span>
        </div>
        <span class="person-item-price">
          <span v-if="item.quantity > 1">x{{ item.quantity }} </span>
          ${{ item.subTotal }}
        </span>
      </div>
    </div>

    <div v-if="byPerson.length === 0" class="empty-state">
      還沒有人點餐
    </div>
  </div>
</template>

<style scoped>
.summary-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  background: var(--color-primary-bg);
  border-radius: var(--border-radius-sm);
  margin-bottom: 16px;
  font-weight: 500;
}
.summary-total {
  font-size: 18px;
  font-weight: 700;
  color: var(--color-primary);
}
.person-section {
  margin-bottom: 16px;
  border: 1px solid var(--color-card-border);
  border-radius: var(--border-radius-sm);
  overflow: hidden;
}
.person-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: var(--color-background-soft);
  font-weight: 500;
}
.person-name {
  color: var(--color-heading);
}
.person-total {
  color: var(--color-primary);
  font-weight: 700;
}
.person-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  border-top: 1px solid var(--color-card-border);
  font-size: 14px;
}
.person-item-main {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}
.item-spec {
  font-size: 12px;
  color: var(--color-text-light);
}
.person-item-price {
  white-space: nowrap;
  font-weight: 500;
  margin-left: 12px;
}
.empty-state {
  text-align: center;
  padding: 40px;
  color: var(--color-text-light);
}
</style>
