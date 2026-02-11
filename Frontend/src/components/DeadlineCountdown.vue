<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'

const props = defineProps<{ deadline: string }>()

const now = ref(Date.now())
let timer: number | undefined

onMounted(() => {
  timer = window.setInterval(() => {
    now.value = Date.now()
  }, 1000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})

const remaining = computed(() => {
  const diff = new Date(props.deadline).getTime() - now.value
  if (diff <= 0) return null

  const hours = Math.floor(diff / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))

  if (hours > 24) {
    const days = Math.floor(hours / 24)
    return `還有 ${days} 天 ${hours % 24} 小時`
  }
  if (hours > 0) {
    return `還有 ${hours} 小時 ${minutes} 分鐘`
  }
  return `還有 ${minutes} 分鐘`
})

const isExpired = computed(() => remaining.value === null)
</script>

<template>
  <span :class="['countdown', { expired: isExpired }]">
    {{ isExpired ? '已截止' : remaining }}
  </span>
</template>

<style scoped>
.countdown {
  font-size: 13px;
  color: var(--color-accent);
  font-weight: 500;
}
.countdown.expired {
  color: var(--color-danger);
}
</style>
