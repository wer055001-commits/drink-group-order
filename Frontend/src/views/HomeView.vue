<script setup lang="ts">
import { onMounted } from 'vue'
import { useDrinkOrderStore } from '@/stores/drinkOrder'
import GroupOrderCard from '@/components/GroupOrderCard.vue'

const store = useDrinkOrderStore()

onMounted(() => {
  store.loadRecentOrders()
})
</script>

<template>
  <div class="home">
    <div class="hero">
      <h2>和同事朋友一起訂飲料！</h2>
      <p>開一個團購，分享連結，大家一起點餐</p>
      <RouterLink to="/create" class="btn btn-accent hero-btn">開新團</RouterLink>
    </div>

    <section v-if="store.activeOrders.length > 0" class="section">
      <h3 class="section-title">進行中的團購</h3>
      <div class="orders-list">
        <GroupOrderCard
          v-for="order in store.activeOrders"
          :key="order.id"
          :order="order"
        />
      </div>
    </section>

    <div v-if="store.activeOrders.length === 0" class="empty-state">
      <div class="empty-icon">🧋</div>
      <p>目前沒有進行中的團購</p>
      <p class="empty-hint">來開一團吧！</p>
    </div>

    <section v-if="store.recentOrders.length > 0" class="section">
      <h3 class="section-title">最近的團購</h3>
      <div class="orders-list">
        <GroupOrderCard
          v-for="order in store.recentOrders"
          :key="order.id"
          :order="order"
        />
      </div>
    </section>
  </div>
</template>

<style scoped>
.home {
  max-width: 600px;
  margin: 0 auto;
}
.hero {
  text-align: center;
  padding: 40px 20px 30px;
}
.hero h2 {
  font-size: 24px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 8px;
}
.hero p {
  color: var(--color-text-light);
  margin-bottom: 20px;
}
.hero-btn {
  padding: 12px 32px;
  font-size: 16px;
  font-weight: 500;
  text-decoration: none;
}
.section {
  margin-bottom: 30px;
}
.section-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-heading);
  margin-bottom: 12px;
  padding-left: 4px;
}
.orders-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--color-text-light);
}
.empty-icon {
  font-size: 48px;
  margin-bottom: 12px;
}
.empty-hint {
  font-size: 13px;
  margin-top: 4px;
}
</style>
