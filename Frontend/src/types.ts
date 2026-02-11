export interface DrinkShop {
  id: number
  name: string
  menuItemCount: number
}

export interface MenuItem {
  id: number
  name: string
  priceMedium: number
  priceLarge: number
}

export interface MenuCategory {
  name: string
  items: MenuItem[]
}

export interface ShopMenu {
  shop: { id: number; name: string }
  categories: MenuCategory[]
}

export interface GroupOrderListItem {
  id: number
  shopName: string
  creatorName: string
  title: string | null
  deadline: string
  status: string
  itemCount: number
  totalCups: number
  totalPrice: number
  createdAt: string
}

export interface OrderItemDetail {
  id: number
  menuItemId: number
  menuItemName: string
  personName: string
  size: string
  sweetLevel: string
  iceLevel: string
  toppings: string[]
  quantity: number
  note: string | null
  subTotal: number
}

export interface PersonOrderSummary {
  name: string
  items: OrderItemDetail[]
  personTotal: number
}

export interface GroupOrderDetail {
  id: number
  shop: { id: number; name: string }
  creatorName: string
  title: string | null
  deadline: string
  status: string
  createdAt: string
  summary: {
    totalItems: number
    totalCups: number
    totalPrice: number
    byPerson: PersonOrderSummary[]
  }
}

export interface DrinkOptions {
  sweetLevels: string[]
  iceLevels: string[]
  sizes: string[]
  toppings: ToppingOption[]
}

export interface ToppingOption {
  name: string
  price: number
}
