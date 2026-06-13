import { classNames } from '@shared/lib/classNames/classNames.ts';
import { HStack } from '@shared/ui/Stack';
import { type FC, memo } from 'react';
import { orderCategories, wavesCategories } from '../../model/const/queueCategories.ts';
import { useActiveCategoryOrWaveId } from '../../model/selectors/getActiveCategoryOrWaveId/getActiveCategoryOrWaveId.ts';
import { useQueueGroupView } from '../../model/selectors/getQueueGroupView/getQueueGroupView.ts';
import cls from './QueueCategoriesList.module.scss';
import { OrderCategory } from '@/entities/Order';

interface IOrderCategoriesListProps {
  onClick: (id: number) => () => void;
}

export const QueueCategoriesList: FC<IOrderCategoriesListProps> = memo(({ onClick }) => {
  const activeView = useQueueGroupView();
  const activeCategoryOrWaveId = useActiveCategoryOrWaveId();
  const isOrderView = activeView === 'order';
  const categories = isOrderView ? orderCategories : wavesCategories;

  const activeAll = !activeCategoryOrWaveId;

  const categoryMods = { [cls.small]: activeView === 'waves' };

  return (
    <HStack className={cls.wrapper} gap="8" max>
      {categories.map((el, i) => {
        return (
          <OrderCategory
            className={classNames(cls.btn, categoryMods, [])}
            key={i}
            view="button"
            id={el.id}
            name={el.shortValue}
            color={el.color}
            onClick={onClick}
            active={activeAll || el.id === activeCategoryOrWaveId}
          />
        );
      })}
    </HStack>
  );
});
