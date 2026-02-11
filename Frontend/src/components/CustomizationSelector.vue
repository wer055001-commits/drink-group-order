<script setup lang="ts">
import type { DrinkOptions } from '@/types'

const props = defineProps<{
  drinkOptions: DrinkOptions
  size: string
  sweetLevel: string
  iceLevel: string
  toppings: string[]
}>()

const emit = defineEmits<{
  'update:size': [value: string]
  'update:sweetLevel': [value: string]
  'update:iceLevel': [value: string]
  'update:toppings': [value: string[]]
}>()

function toggleTopping(name: string) {
  const current = [...props.toppings]
  const idx = current.indexOf(name)
  if (idx >= 0) {
    current.splice(idx, 1)
  } else {
    current.push(name)
  }
  emit('update:toppings', current)
}
</script>

<template>
  <div class="customization">
    <div class="option-group">
      <label class="option-label">大小</label>
      <div class="option-buttons">
        <button
          v-for="s in drinkOptions.sizes"
          :key="s"
          :class="['option-btn', { active: size === s }]"
          @click="emit('update:size', s)"
        >
          {{ s }}
        </button>
      </div>
    </div>

    <div class="option-group">
      <label class="option-label">甜度</label>
      <div class="option-buttons">
        <button
          v-for="sw in drinkOptions.sweetLevels"
          :key="sw"
          :class="['option-btn', { active: sweetLevel === sw }]"
          @click="emit('update:sweetLevel', sw)"
        >
          {{ sw }}
        </button>
      </div>
    </div>

    <div class="option-group">
      <label class="option-label">冰塊</label>
      <div class="option-buttons">
        <button
          v-for="ice in drinkOptions.iceLevels"
          :key="ice"
          :class="['option-btn', { active: iceLevel === ice }]"
          @click="emit('update:iceLevel', ice)"
        >
          {{ ice }}
        </button>
      </div>
    </div>

    <div class="option-group">
      <label class="option-label">加料</label>
      <div class="option-buttons topping-buttons">
        <button
          v-for="t in drinkOptions.toppings"
          :key="t.name"
          :class="['option-btn topping-btn', { active: toppings.includes(t.name) }]"
          @click="toggleTopping(t.name)"
        >
          {{ t.name }}
          <span class="topping-price">+${{ t.price }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.customization {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.option-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.option-label {
  font-weight: 500;
  font-size: 14px;
  color: var(--color-heading);
}
.option-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.option-btn {
  padding: 8px 16px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--border-radius-sm);
  background: white;
  color: var(--color-text);
  font-size: 14px;
  font-family: inherit;
  cursor: pointer;
  transition: all 0.2s;
}
.option-btn:hover {
  border-color: var(--color-primary-light);
}
.option-btn.active {
  border-color: var(--color-primary);
  background: var(--color-primary-bg);
  color: var(--color-primary);
  font-weight: 500;
}
.topping-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  min-width: 70px;
}
.topping-price {
  font-size: 11px;
  color: var(--color-text-light);
}
.topping-btn.active .topping-price {
  color: var(--color-primary);
}
</style>
