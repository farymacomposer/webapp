import { HStack } from '@shared/ui/Stack';
import { type FC, memo } from 'react';
import { useActiveCategoryOrWaveId, useQueueGroupView } from '../../../Queue';
import { orderCategories, wavesCategories } from '../../model/consts/orderCategoriesConsts.ts';
import { OrderCategory } from '../OrderCategory/OrderCategory';
import cls from './OrderCategoriesList.module.scss';

interface IOrderCategoriesListProps {
  onClick: (id: number) => () => void;
}

export const OrderCategoriesList: FC<IOrderCategoriesListProps> = memo(({ onClick }) => {
  const activeView = useQueueGroupView();
  const activeCategoryOrWaveId = useActiveCategoryOrWaveId();
  const categories = activeView === 'order' ? orderCategories : wavesCategories;

  const activeAll = !activeCategoryOrWaveId;

  return (
    <HStack className={cls.wrapper} gap="8" max>
      {categories.map((el, i) => {
        return (
          <OrderCategory
            className={cls.btn}
            key={i}
            view="button"
            id={el.id}
            name={el.value}
            color={el.color}
            onClick={onClick}
            active={activeAll || el.id === activeCategoryOrWaveId}
          />
        );
      })}
    </HStack>
  );
});
