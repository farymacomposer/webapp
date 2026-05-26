import {
  categoryGap,
  headerHeight,
  orderBigCardHeight,
  rectangleCategoryNameHeight,
} from '../../consts/sizes.ts';
import { type CategoryWithOrders } from '../../types/order.ts';

const paddings = 40;

export const useScrollCategoryBlockHeight = (category?: CategoryWithOrders): string => {
  if (!category) return '0';

  const height =
    category.orders.length * (orderBigCardHeight + Number(categoryGap)) +
    rectangleCategoryNameHeight +
    Number(categoryGap) +
    paddings;

  return `calc(100vh - ${height}px - ${headerHeight}px)`;
};
