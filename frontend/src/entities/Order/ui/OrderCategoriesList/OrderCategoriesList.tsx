import { HStack } from '@shared/ui/Stack';
import { type FC, memo } from 'react';
import { orderCategories } from '../../model/consts/orderCategoriesConsts.ts';
import { OrderCategory } from '../OrderCategory/OrderCategory';
import cls from './OrderCategoriesList.module.scss';

interface IOrderCategoriesListProps {
  onClick: (id: string) => () => void;
}

export const OrderCategoriesList: FC<IOrderCategoriesListProps> = memo(({ onClick }) => {
  return (
    <HStack className={cls.wrapper} justify="between" gap="10" max>
      {orderCategories.map((el, i) => (
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
