import { categoryGap, headerHeight } from '../../const/sizes.ts';
import { useActiveCategories } from '../../selectors/getActiveCategories/getActiveCategories.ts';
import { orderBigCardHeight, rectangleCategoryNameHeight } from '@/entities/Order';

const paddings = 20;

export const useScrollCategoryBlockHeight = (): string => {
  const categories = useActiveCategories();
  const category = categories.at(-1);

  if (!category) return '0';

  const height =
    category.openIds.length * (orderBigCardHeight + Number(categoryGap)) +
    rectangleCategoryNameHeight +
    Number(categoryGap) * 2 +
    Number(category.needMoreBtn) * (18 + Number(categoryGap)) +
    paddings;

  return `calc(100vh - ${height}px - ${headerHeight}px)`;
};
