import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { type FC, memo, type RefObject, useRef } from 'react';
import { useQueueGroupView } from '../../../Queue';
import {
  orderCategoriesColorsDict,
  wavesCategoriesColorsDict,
} from '../../model/consts/orderCategoriesConsts.ts';
import { categoryGap } from '../../model/consts/sizes.ts';
import { useChangeActiveCategoryWithScroll } from '../../model/lib/useChangeActiveCategoryWithScroll/useChangeActiveCategoryWithScroll.ts';
import { useScrollCategoryBlockHeight } from '../../model/lib/useScrollCategoryBlockHeight/useScrollCategoryBlockHeight.ts';
import { type CategoryWithOrders } from '../../model/types/order.ts';
import { OrderCard } from '../OrderCard/OrderCard.tsx';
import { OrderCategory } from '../OrderCategory/OrderCategory';
import cls from './OrderCardsCategoryList.module.scss';

export interface OrderCardProps {
  orders: CategoryWithOrders[];
  /**
   * При скролле списка меняется активная категория (в заивисмости от положения скролла)
   */
  scrollWithChangingActiveCategory?: boolean;
  containerRef?: RefObject<HTMLElement | null>;
}

export const OrderCardsCategoryList: FC<OrderCardProps> = memo(
  ({ orders, scrollWithChangingActiveCategory, containerRef }) => {
    const activeView = useQueueGroupView();
    const categoriesColorsDict =
      activeView === 'order' ? orderCategoriesColorsDict : wavesCategoriesColorsDict;
    const lastCategoryHeight = useScrollCategoryBlockHeight(orders.at(-1));

    const refs = useRef<Record<string, HTMLElement | null>>({});
    useChangeActiveCategoryWithScroll({ refs, containerRef, orders });

    return (
      <VStack gap="20" className={cls.wrapper}>
        {orders.map((category, i) => (
          <VStack
            id={String(category.id)}
            key={category.id + activeView}
            ref={(el: HTMLElement | null) => {
              refs.current[category.id] = el;
            }}
            className={cls.categoryWrapper}
            gap={categoryGap}
            max
          >
            <OrderCategory
              id={category.id}
              name={category.name}
              color={categoriesColorsDict[category.name as keyof typeof categoriesColorsDict]}
              view="rectangle"
            />
            {category.orders.map((el) => (
              <OrderCard key={el.id} view="big" order={el} />
            ))}
            {scrollWithChangingActiveCategory && i + 1 === orders.length && (
              <HStack
                justify="center"
                className={cls.categoryMinHeight}
                style={{ height: lastCategoryHeight }}
              >
                <Text variant="transparent" size="16" align="center">
                  треков больше нет...
                </Text>
              </HStack>
            )}
          </VStack>
        ))}
      </VStack>
    );
  },
);
