import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useOrders] = buildSelector((state) => state?.queue.orders);
