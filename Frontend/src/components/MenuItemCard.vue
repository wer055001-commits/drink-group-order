<script setup lang="ts">
import type { MenuItem } from '@/types'

defineProps<{
  item: MenuItem
  selected: boolean
}>()

defineEmits<{
  select: []
}>()
</script>

<template>
  <div :class="['menu-item-card', { selected }]" @click="$emit('select')">
    <span class="item-name">{{ item.name }}</span>
    <span class="item-price">${{ item.priceMedium }} ~ ${{ item.priceLarge }}</span>
  </div>
</template>

<style scoped>
.menu-item-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border: 1.5px solid var(--color-card-border);
  border-radius: var(--border-radius-sm);
  cursor: pointer;
  transition: all 0.2s;
  background: white;
}
.menu-item-card:hover {
  border-color: var(--color-primary-light);
  background: var(--color-primary-bg);
}
.menu-item-card.selected {
  border-color: var(--color-primary);
  background: var(--color-primary-bg);
}
.item-name {
  font-weight: 500;
  color: var(--color-heading);
}
.item-price {
  font-size: 13px;
  color: var(--color-text-light);
  white-space: nowrap;
  margin-left: 12px;
}
</style>
