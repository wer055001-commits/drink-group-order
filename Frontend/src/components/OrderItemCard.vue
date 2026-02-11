<script setup lang="ts">
import type { OrderItemDetail } from '@/types'

defineProps<{
  item: OrderItemDetail
  editable: boolean
}>()

defineEmits<{
  delete: []
}>()
</script>

<template>
  <div class="item-card">
    <div class="item-header">
      <span class="person-badge">{{ item.personName }}</span>
      <span class="item-price">${{ item.subTotal }}</span>
    </div>
    <div class="item-detail">
      <span class="drink-name">{{ item.menuItemName }}</span>
      <span class="drink-size">{{ item.size }}</span>
      <span v-if="item.quantity > 1" class="drink-qty">x{{ item.quantity }}</span>
    </div>
    <div class="item-custom">
      <span class="custom-tag">{{ item.sweetLevel }}</span>
      <span class="custom-tag">{{ item.iceLevel }}</span>
      <span v-for="t in item.toppings" :key="t" class="custom-tag topping-tag">{{ t }}</span>
    </div>
    <div v-if="item.note" class="item-note">備註：{{ item.note }}</div>
    <div v-if="editable" class="item-actions">
      <button class="btn btn-sm btn-danger" @click="$emit('delete')">刪除</button>
    </div>
  </div>
</template>

<style scoped>
.item-card {
  padding: 14px;
  border: 1px solid var(--color-card-border);
  border-radius: var(--border-radius-sm);
  background: white;
}
.item-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}
.person-badge {
  background: var(--color-primary-bg);
  color: var(--color-primary);
  padding: 2px 10px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 500;
}
.item-price {
  font-weight: 700;
  color: var(--color-primary);
  font-size: 16px;
}
.item-detail {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}
.drink-name {
  font-weight: 500;
  color: var(--color-heading);
}
.drink-size {
  font-size: 13px;
  color: var(--color-text-light);
}
.drink-qty {
  font-size: 13px;
  color: var(--color-warning);
  font-weight: 500;
}
.item-custom {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 4px;
}
.custom-tag {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 4px;
  background: var(--color-background-soft);
  color: var(--color-text-light);
}
.topping-tag {
  background: #FFF3E0;
  color: #E65100;
}
.item-note {
  font-size: 13px;
  color: var(--color-text-light);
  font-style: italic;
  margin-top: 4px;
}
.item-actions {
  margin-top: 8px;
  display: flex;
  justify-content: flex-end;
}
</style>
