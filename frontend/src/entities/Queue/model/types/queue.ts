import { type Order } from '@/entities/Order';

export type QueueGroupView = 'order' | 'waves';

export type ActiveCategoryId = number | null;

export type ActiveWaveId = number | null;

export type OpenCardId = number | null;

export interface OrdersListSchema {
  [orderId: number]: Order;
}

export interface CategoryWithOrders {
  id: number;
  name: string;
  orderIds: number[];
}

export interface CategoryWithOrdersSchema {
  id: number;
  name: string;
  orderIds: number[];
  openIds: number[];
  needMoreBtn: boolean;
  showMoreBtn: boolean;
}

export interface QueueSchema {
  orders: OrdersListSchema;
  categories: CategoryWithOrdersSchema[];
  waves: CategoryWithOrdersSchema[];
  open: boolean;
  groupView: QueueGroupView;
  activeCategoryId: ActiveCategoryId;
  activeWaveId: ActiveWaveId;
  openCardId: OpenCardId;
}
