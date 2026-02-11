<script setup lang="ts">
import type { DrinkShop } from '@/types'

defineProps<{
  shop: DrinkShop
  selected: boolean
}>()

defineEmits<{
  select: []
}>()
</script>

<template>
  <div :class="['shop-card card', { selected }]" @click="$emit('select')">
    <div class="shop-name">{{ shop.name }}</div>
    <div class="shop-count">{{ shop.menuItemCount }} 種飲品</div>
  </div>
</template>

<style scoped>
.shop-card {
  cursor: pointer;
  text-align: center;
  transition: all 0.2s;
  border: 2px solid transparent;
}
.shop-card:hover {
  border-color: var(--color-primary-light);
}
.shop-card.selected {
  border-color: var(--color-primary);
  background: var(--color-primary-bg);
}
.shop-name {
  font-size: 18px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 4px;
}
.shop-count {
  font-size: 13px;
  color: var(--color-text-light);
}
</style>
