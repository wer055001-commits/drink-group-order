<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useDrinkOrderStore } from '@/stores/drinkOrder'
import DrinkShopCard from '@/components/DrinkShopCard.vue'

const router = useRouter()
const store = useDrinkOrderStore()

const creatorName = ref(store.userName)
const selectedShopId = ref<number | null>(null)
const title = ref('')
const deadlineDate = ref('')
const deadlineTime = ref('16:00')
const error = ref('')
const submitting = ref(false)

onMounted(async () => {
  await store.loadShops()
  const today = new Date()
  deadlineDate.value = today.toISOString().split('T')[0]
})

const canSubmit = computed(() =>
  creatorName.value.trim() && selectedShopId.value !== null && deadlineDate.value && deadlineTime.value
)

async function submit() {
  if (!canSubmit.value || submitting.value) return

  error.value = ''
  submitting.value = true

  store.setUserName(creatorName.value.trim())

  const deadline = `${deadlineDate.value}T${deadlineTime.value}:00`

  try {
    const res = await fetch('/api/group-orders', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        shopId: selectedShopId.value,
        creatorName: creatorName.value.trim(),
        title: title.value.trim() || null,
        deadline
      })
    })

    if (!res.ok) {
      const text = await res.text()
      error.value = text || '建立失敗，請再試一次'
      return
    }

    const data = await res.json()
    router.push(`/order/${data.id}`)
  } catch {
    error.value = '連線失敗，請確認系統是否已啟動'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="create-page">
    <h2 class="page-title">開新團</h2>

    <div class="form-group">
      <label class="form-label">你的名字</label>
      <input v-model="creatorName" class="form-input" placeholder="輸入你的名字" />
    </div>

    <div class="form-group">
      <label class="form-label">選一家飲料店</label>
      <div class="shop-grid">
        <DrinkShopCard
          v-for="shop in store.shops"
          :key="shop.id"
          :shop="shop"
          :selected="selectedShopId === shop.id"
          @select="selectedShopId = shop.id"
        />
      </div>
    </div>

    <div class="form-group">
      <label class="form-label">團購標題（選填）</label>
      <input v-model="title" class="form-input" placeholder="例：下午茶時間" />
    </div>

    <div class="form-row">
      <div class="form-group">
        <label class="form-label">截止日期</label>
        <input v-model="deadlineDate" type="date" class="form-input" />
      </div>
      <div class="form-group">
        <label class="form-label">截止時間</label>
        <input v-model="deadlineTime" type="time" class="form-input" />
      </div>
    </div>

    <div v-if="error" class="error-msg">{{ error }}</div>

    <button class="btn btn-accent submit-btn" :disabled="!canSubmit || submitting" @click="submit">
      {{ submitting ? '建立中...' : '建立團購' }}
    </button>
  </div>
</template>

<style scoped>
.create-page {
  max-width: 600px;
  margin: 0 auto;
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 24px;
}
.shop-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}
.error-msg {
  color: var(--color-danger);
  font-size: 14px;
  margin-bottom: 12px;
}
.submit-btn {
  width: 100%;
  padding: 14px;
  font-size: 16px;
  font-weight: 500;
  margin-top: 8px;
}
.submit-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
