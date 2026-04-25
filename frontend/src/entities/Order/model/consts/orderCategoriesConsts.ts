import { type ButtonColor } from '@shared/ui/Button';

enum OrderCategories {
  NOW = 'NOW',
  NEXT_UP = 'NEXT UP',
  FINISHED = 'FINISHED',
  FUTURE = 'FUTURE',
  FROZEN = 'FROZEN',
}

export enum OrderCategoriesColors {
  MAGENTA = 'magenta',
  ORANGE = 'orange',
  SKU_BLUE = 'sky-blue',
  VIOLET = 'violet',
  INACTIVE = 'inactive-color',
}

export const orderCategoriesColorsDict = {
  [OrderCategories.NOW]: OrderCategoriesColors.MAGENTA,
  [OrderCategories.NEXT_UP]: OrderCategoriesColors.ORANGE,
  [OrderCategories.FINISHED]: OrderCategoriesColors.SKU_BLUE,
  [OrderCategories.FUTURE]: OrderCategoriesColors.VIOLET,
  [OrderCategories.FROZEN]: OrderCategoriesColors.INACTIVE,
};

export const orderCategories = Object.values(OrderCategories).map((el, i) => ({
  id: i + 1,
  value: el,
  color: orderCategoriesColorsDict[el] as ButtonColor,
}));
