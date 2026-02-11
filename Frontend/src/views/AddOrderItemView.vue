<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useDrinkOrderStore } from '@/stores/drinkOrder'
import type { ShopMenu, MenuItem } from '@/types'
import MenuItemCard from '@/components/MenuItemCard.vue'
import CustomizationSelector from '@/components/CustomizationSelector.vue'

const route = useRoute()
const router = useRouter()
const store = useDrinkOrderStore()

const groupId = computed(() => Number(route.params.id))

const shopMenu = ref<ShopMenu | null>(null)
const shopId = ref<number | null>(null)
const loading = ref(true)
const error = ref('')
const submitting = ref(false)
const submitSuccess = ref(false)

// 點餐表單
const personName = ref(store.userName)
const selectedItem = ref<MenuItem | null>(null)
const size = ref('中杯')
const sweetLevel = ref('正常甜')
const iceLevel = ref('正常冰')
const toppings = ref<string[]>([])
const quantity = ref(1)
const note = ref('')

// 計算即時價格
const livePrice = computed(() => {
  if (!selectedItem.value || !store.drinkOptions) return 0
  const base = size.value === '大杯'
    ? selectedItem.value.priceLarge
    : selectedItem.value.priceMedium
  const toppingsPrice = toppings.value.reduce((sum, t) => {
    const match = store.drinkOptions!.toppings.find(opt => opt.name === t)
    return sum + (match?.price ?? 0)
  }, 0)
  return (base + toppingsPrice) * quantity.value
})

const canSubmit = computed(() =>
  personName.value.trim() && selectedItem.value !== null
)

onMounted(async () => {
  try {
    // 載入團購資訊以取得 shopId
    const orderRes = await fetch(`/api/group-orders/${groupId.value}`)
    if (!orderRes.ok) {
      error.value = '找不到這筆團購'
      return
    }
    const orderData = await orderRes.json()
    shopId.value = orderData.shop.id

    if (orderData.status !== '開放中') {
      error.value = '這筆團購已截止，無法點餐'
      return
    }

    // 載入菜單
    const menuRes = await fetch(`/api/shops/${shopId.value}`)
    if (!menuRes.ok) {
      error.value = '載入菜單失敗'
      return
    }
    shopMenu.value = await menuRes.json()

    // 載入飲料選項
    await store.loadDrinkOptions()
  } catch {
    error.value = '連線失敗'
  } finally {
    loading.value = false
  }
})

function selectMenuItem(item: MenuItem) {
  selectedItem.value = item
}

function resetForm() {
  selectedItem.value = null
  size.value = '中杯'
  sweetLevel.value = '正常甜'
  iceLevel.value = '正常冰'
  toppings.value = []
  quantity.value = 1
  note.value = ''
  submitSuccess.value = false
}

async function submit(andContinue: boolean) {
  if (!canSubmit.value || submitting.value) return

  submitting.value = true
  store.setUserName(personName.value.trim())

  try {
    const res = await fetch(`/api/group-orders/${groupId.value}/items`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        menuItemId: selectedItem.value!.id,
        personName: personName.value.trim(),
        size: size.value,
        sweetLevel: sweetLevel.value,
        iceLevel: iceLevel.value,
        toppings: toppings.value.length > 0 ? toppings.value : null,
        quantity: quantity.value,
        note: note.value.trim() || null
      })
    })

    if (!res.ok) {
      const text = await res.text()
      error.value = text || '點餐失敗'
      return
    }

    if (andContinue) {
      resetForm()
      submitSuccess.value = true
      setTimeout(() => { submitSuccess.value = false }, 2000)
    } else {
      router.push(`/order/${groupId.value}`)
    }
  } catch {
    error.value = '連線失敗'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="add-page">
    <div v-if="loading" class="loading">載入菜單中...</div>
    <div v-else-if="error" class="error-msg">{{ error }}</div>
    <template v-else-if="shopMenu && store.drinkOptions">
      <h2 class="page-title">{{ shopMenu.shop.name }} - 點餐</h2>

      <div class="form-group">
        <label class="form-label">你的名字</label>
        <input v-model="personName" class="form-input" placeholder="輸入你的名字" />
      </div>

      <div v-if="submitSuccess" class="success-msg">已成功加入訂單！繼續點下一杯</div>

      <!-- 菜單 -->
      <div class="menu-section">
        <div v-for="category in shopMenu.categories" :key="category.name" class="category">
          <h3 class="category-title">{{ category.name }}</h3>
          <div class="category-items">
            <MenuItemCard
              v-for="item in category.items"
              :key="item.id"
              :item="item"
              :selected="selectedItem?.id === item.id"
              @select="selectMenuItem(item)"
            />
          </div>
        </div>
      </div>

      <!-- 客製化 -->
      <div v-if="selectedItem" class="customize-section card">
        <h3 class="section-title">
          客製化：{{ selectedItem.name }}
        </h3>

        <CustomizationSelector
          :drink-options="store.drinkOptions"
          :size="size"
          :sweet-level="sweetLevel"
          :ice-level="iceLevel"
          :toppings="toppings"
          @update:size="size = $event"
          @update:sweet-level="sweetLevel = $event"
          @update:ice-level="iceLevel = $event"
          @update:toppings="toppings = $event"
        />

        <div class="quantity-group">
          <label class="option-label">數量</label>
          <div class="quantity-control">
            <button class="qty-btn" @click="quantity = Math.max(1, quantity - 1)">-</button>
            <span class="qty-value">{{ quantity }}</span>
            <button class="qty-btn" @click="quantity++">+</button>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">備註（選填）</label>
          <input v-model="note" class="form-input" placeholder="例：去冰改溫的" />
        </div>

        <div class="price-display">
          小計：<span class="price-amount">${{ livePrice }}</span>
        </div>

        <div class="submit-actions">
          <button
            class="btn btn-accent"
            :disabled="!canSubmit || submitting"
            @click="submit(false)"
          >
            {{ submitting ? '送出中...' : '加入訂單' }}
          </button>
          <button
            class="btn btn-outline"
            :disabled="!canSubmit || submitting"
            @click="submit(true)"
          >
            送出並繼續點
          </button>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.add-page {
  max-width: 600px;
  margin: 0 auto;
  padding-bottom: 40px;
}
.loading, .error-msg {
  text-align: center;
  padding: 40px;
  color: var(--color-text-light);
}
.error-msg {
  color: var(--color-danger);
}
.page-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 20px;
}
.success-msg {
  background: #E8F5E9;
  color: #2E7D32;
  padding: 10px 16px;
  border-radius: var(--border-radius-sm);
  margin-bottom: 16px;
  text-align: center;
  font-weight: 500;
}
.menu-section {
  margin-bottom: 20px;
}
.category {
  margin-bottom: 16px;
}
.category-title {
  font-size: 14px;
  font-weight: 700;
  color: var(--color-primary);
  margin-bottom: 8px;
  padding-left: 4px;
}
.category-items {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.customize-section {
  margin-top: 20px;
}
.section-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 16px;
}
.quantity-group {
  margin-top: 16px;
}
.option-label {
  font-weight: 500;
  font-size: 14px;
  color: var(--color-heading);
  display: block;
  margin-bottom: 8px;
}
.quantity-control {
  display: flex;
  align-items: center;
  gap: 16px;
}
.qty-btn {
  width: 36px;
  height: 36px;
  border: 1.5px solid var(--color-border);
  border-radius: var(--border-radius-sm);
  background: white;
  font-size: 18px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text);
  font-family: inherit;
}
.qty-btn:hover {
  border-color: var(--color-primary);
}
.qty-value {
  font-size: 18px;
  font-weight: 500;
  min-width: 24px;
  text-align: center;
}
.price-display {
  margin-top: 20px;
  padding: 14px 16px;
  background: var(--color-primary-bg);
  border-radius: var(--border-radius-sm);
  font-size: 16px;
  text-align: center;
}
.price-amount {
  font-size: 22px;
  font-weight: 700;
  color: var(--color-primary);
}
.submit-actions {
  display: flex;
  gap: 10px;
  margin-top: 16px;
}
.submit-actions .btn {
  flex: 1;
  padding: 14px;
  font-size: 15px;
  font-weight: 500;
}
.submit-actions .btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
