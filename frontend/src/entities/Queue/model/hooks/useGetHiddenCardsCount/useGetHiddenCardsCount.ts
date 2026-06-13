import { initialNumOfOpenCards } from '../../const/const.ts';
import type { CategoryWithOrdersSchema } from '../../types/queue.ts';

export const useGetHiddenCardsCount = (category: CategoryWithOrdersSchema) => {
  const hiddenCardsCount = category.orderIds.length - initialNumOfOpenCards;
  let hiddenCardsText = '';

  if (hiddenCardsCount % 10 === 1 && hiddenCardsCount % 100 !== 11) {
    hiddenCardsText = 'трек';
  } else if (
    hiddenCardsCount % 10 >= 2 &&
    hiddenCardsCount % 10 <= 4 &&
    (hiddenCardsCount % 100 < 12 || hiddenCardsCount % 100 > 14)
  ) {
    hiddenCardsText = 'трека';
  } else {
    hiddenCardsText = 'треков';
  }

  return { hiddenCardsCount, hiddenCardsText };
};
