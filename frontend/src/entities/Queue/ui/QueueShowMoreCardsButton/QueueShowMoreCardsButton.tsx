import Arrow from '@shared/assets/icons/arrow_in_circle.svg';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { Icon } from '@shared/ui/Icon';
import { HStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { type FC, memo, useCallback } from 'react';
import { useGetHiddenCardsCount } from '../../model/hooks/useGetHiddenCardsCount/useGetHiddenCardsCount.ts';
import { queueActions } from '../../model/slice/queueSlice.ts';
import type { CategoryWithOrdersSchema } from '../../model/types/queue.ts';
import cls from './QueueShowMoreCardsButton.module.scss';

interface IProps {
  category: CategoryWithOrdersSchema;
}

export const QueueShowMoreCardsButton: FC<IProps> = memo(({ category }) => {
  const dispatch = useAppDispatch();

  const { hiddenCardsCount, hiddenCardsText } = useGetHiddenCardsCount(category);

  const changeNumberOfCategoryOpenCards = useCallback(
    (category: CategoryWithOrdersSchema) => {
      if (category.showMoreBtn) {
        dispatch(queueActions.changeNumberOfCategoryOpenCards({ categoryId: category.id }));
      } else {
        dispatch(
          queueActions.changeNumberOfCategoryOpenCards({ categoryId: category.id, type: 'hide' }),
        );
      }
    },
    [dispatch],
  );

  return (
    <HStack
      className={cls.showMore}
      gap="8"
      justify="end"
      max
      onClick={() => changeNumberOfCategoryOpenCards(category)}
    >
      <Icon
        className={category.showMoreBtn ? '' : cls.showMoreIconRotate}
        width="18"
        height="18"
        Svg={Arrow}
        type="not-clickable"
      />
      <Text size="14">
        {category.showMoreBtn ? `ещё ${hiddenCardsCount} ${hiddenCardsText}` : 'свернуть'}
      </Text>
    </HStack>
  );
});
