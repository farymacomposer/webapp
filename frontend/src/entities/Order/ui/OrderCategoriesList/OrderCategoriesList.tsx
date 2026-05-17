import { HStack } from '@shared/ui/Stack';
import { type FC, memo } from 'react';
import { useQueueGroupView } from '../../../Queue';
import { orderCategories, wavesCategories } from '../../model/consts/orderCategoriesConsts.ts';
import { OrderCategory } from '../OrderCategory/OrderCategory';
import cls from './OrderCategoriesList.module.scss';

interface IOrderCategoriesListProps {
  onClick: (id: string) => () => void;
}

export const OrderCategoriesList: FC<IOrderCategoriesListProps> = memo(({ onClick }) => {
  const activeView = useQueueGroupView();
  const categories = activeView === 'order' ? orderCategories : wavesCategories;

  return (
    <HStack className={cls.wrapper} gap="10" max>
      {categories.map((el, i) => (
        <OrderCategory
          className={cls.btn}
          key={i}
          view="button"
          name={el.value}
          color={el.color}
          onClick={onClick}
        />
      ))}
    </HStack>
  );
});
