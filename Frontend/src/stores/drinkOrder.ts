import { ref } from 'vue'
import { defineStore } from 'pinia'
import type { DrinkShop, DrinkOptions, GroupOrderListItem } from '@/types'

export const useDrinkOrderStore = defineStore('drinkOrder', () => {
  const shops = ref<DrinkShop[]>([])
  const drinkOptions = ref<DrinkOptions | null>(null)
  const activeOrders = ref<GroupOrderListItem[]>([])
  const recentOrders = ref<GroupOrderListItem[]>([])
  const userName = ref(localStorage.getItem('userName') || '')

  function setUserName(name: string) {
    userName.value = name
    localStorage.setItem('userName', name)
  }

  async function loadShops() {
    const res = await fetch('/api/shops')
    shops.value = await res.json()
  }

  async function loadDrinkOptions() {
    if (drinkOptions.value) return
    const res = await fetch('/api/drink-options')
    drinkOptions.value = await res.json()
  }

  async function loadActiveOrders() {
    const res = await fetch('/api/group-orders?status=開放中')
    activeOrders.value = await res.json()
  }

  async function loadRecentOrders() {
    const res = await fetch('/api/group-orders')
    const all: GroupOrderListItem[] = await res.json()
    activeOrders.value = all.filter(o => o.status === '開放中')
    recentOrders.value = all.filter(o => o.status !== '開放中').slice(0, 10)
  }

  return {
    shops, drinkOptions, activeOrders, recentOrders, userName,
    setUserName, loadShops, loadDrinkOptions, loadActiveOrders, loadRecentOrders
  }
})
