import { VStack } from '@shared/ui/Stack';
import { memo } from 'react';
import { orderCategoriesColorsDict } from '../../model/consts/orderCategoriesConsts.ts';
import { type CategoryWithOrders } from '../../model/types/order.ts';
import { OrderCard } from '../OrderCard/OrderCard.tsx';
import { OrderCategory } from '../OrderCategory/OrderCategory';
import cls from './OrderCardsCategoryList.module.scss';

export interface OrderCardProps {
  orders: CategoryWithOrders[];
}

export const OrderCardsCategoryList = memo(({ orders }: OrderCardProps) => {
  return (
    <VStack gap="32" className={cls.wrapper}>
      {orders.map((category) => (
        <VStack key={category.id} gap="16" max>
          <OrderCategory
            id={category.name}
            name={category.name}
            color={
              orderCategoriesColorsDict[category.name as keyof typeof orderCategoriesColorsDict]
            }
            view="rectangle"
          />
          {category.orders.map((el) => (
            <OrderCard key={el.id} view="big" order={el} />
          ))}
        </VStack>
      ))}
    </VStack>
  );
});
