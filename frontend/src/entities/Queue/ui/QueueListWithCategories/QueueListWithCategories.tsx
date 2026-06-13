import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { type FC, memo, type RefObject, useRef } from 'react';
import { categoryGap } from '../../model/const/sizes.ts';
import { useChangeActiveCategoryWithScroll } from '../../model/hooks/useChangeActiveCategoryWithScroll/useChangeActiveCategoryWithScroll.ts';
import { useScrollCategoryBlockHeight } from '../../model/hooks/useScrollCategoryBlockHeight/useScrollCategoryBlockHeight.ts';
import { useActiveCategories } from '../../model/selectors/getActiveCategories/getActiveCategories.ts';
import { useOrders } from '../../model/selectors/getOrders/getOrders.ts';
import { useQueueGroupView } from '../../model/selectors/getQueueGroupView/getQueueGroupView.ts';
import { QueueShowMoreCardsButton } from '../QueueShowMoreCardsButton/QueueShowMoreCardsButton.tsx';
import cls from './QueueListWithCategories.module.scss';
import {
  OrderCard,
  orderCategoriesColorsDict,
  OrderCategory,
  wavesCategoriesColorsDict,
} from '@/entities/Order';

export interface OrderCardProps {
  /**
   * При скролле списка меняется активная категория (в заивисмости от положения скролла)
   */
  scrollWithChangingActiveCategory?: boolean;
  /**
   * Ref родительсколького div (контейнера)
   */
  containerRef?: RefObject<HTMLElement | null>;
}

export const QueueListWithCategories: FC<OrderCardProps> = memo(
  ({ scrollWithChangingActiveCategory, containerRef }) => {
    const orders = useOrders();
    const categories = useActiveCategories();
    const activeView = useQueueGroupView();
    const categoriesColorsDict =
      activeView === 'order' ? orderCategoriesColorsDict : wavesCategoriesColorsDict;
    const lastCategoryHeight = useScrollCategoryBlockHeight();

    const refs = useRef<Record<string, HTMLElement | null>>({});
    useChangeActiveCategoryWithScroll({ refs, containerRef });

    return (
      <VStack gap="20" className={cls.wrapper}>
        {categories.map((category, i) => (
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
            {category.openIds.map((el) => (
              <OrderCard key={el} view="big" order={orders[el]} />
            ))}
            {category.needMoreBtn && <QueueShowMoreCardsButton category={category} />}
            {scrollWithChangingActiveCategory && i + 1 === categories.length && (
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
