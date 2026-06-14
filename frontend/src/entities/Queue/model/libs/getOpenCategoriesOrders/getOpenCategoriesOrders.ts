import { initialNumOfOpenCards } from '../../const/const.ts';
import { type CategoryWithOrders, type CategoryWithOrdersSchema } from '../../types/queue.ts';

interface IProps {
  categories: CategoryWithOrders[];
  prev: CategoryWithOrdersSchema[];
  openCategoryId?: number;
  type?: 'show' | 'hide';
}

export const getOpenCategoriesOrders = ({
  categories,
  prev,
  openCategoryId,
  type = 'show',
}: IProps): CategoryWithOrdersSchema[] => {
  return categories.map((el) => {
    const prevEl = prev?.find((cat) => cat.id === el.id);
    const numberOfOpenCards = !prevEl ? initialNumOfOpenCards : prevEl.openIds.length;

    if (!prevEl || openCategoryId !== el.id) {
      return {
        ...el,
        openIds: el.orderIds.slice(0, numberOfOpenCards),
        needMoreBtn: el.orderIds.length > initialNumOfOpenCards,
        showMoreBtn: el.orderIds.length > numberOfOpenCards,
      };
    }

    const newNumberOfOpenCards = type === 'show' ? prevEl?.orderIds.length : initialNumOfOpenCards;

    return {
      ...el,
      openIds: el.orderIds.slice(0, newNumberOfOpenCards),
      needMoreBtn: el.orderIds.length > initialNumOfOpenCards,
      showMoreBtn: el.orderIds.length > newNumberOfOpenCards,
    };
  });
};
