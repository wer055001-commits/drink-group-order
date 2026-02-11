import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/create',
      name: 'create-group-order',
      component: () => import('../views/CreateGroupOrderView.vue'),
    },
    {
      path: '/order/:id',
      name: 'group-order-detail',
      component: () => import('../views/GroupOrderDetailView.vue'),
    },
    {
      path: '/order/:id/add',
      name: 'add-order-item',
      component: () => import('../views/AddOrderItemView.vue'),
    },
    {
      path: '/order/:id/summary',
      name: 'order-summary',
      component: () => import('../views/OrderSummaryView.vue'),
    },
  ],
})

export default router
