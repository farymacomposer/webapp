import { type ButtonColor } from '@shared/ui/Button';

enum OrderCategories {
  NOW = 'NOW',
  NEXT_UP = 'NEXT UP',
  FINISHED = 'FINISHED',
  FUTURE = 'FUTURE',
  FROZEN = 'FROZEN',
}

enum WavesCategories {
  NEW = 'NEW',
  WAVE1 = 'WAVE1',
  WAVE2 = 'WAVE2',
  WAVE3 = 'WAVE3',
  WAVE4 = 'WAVE4',
  WAVE5 = 'WAVE5',
  WAVE6 = 'WAVE6',
  WAVE7 = 'WAVE7',
  WAVE8 = 'WAVE8',
  WAVE9 = 'WAVE9',
}

export enum OrderCategoriesColors {
  MAGENTA = 'magenta',
  MAGENTA_PINK = 'magenta-pink',
  DEEP_MAGENTA = 'deep-magenta',
  ORANGE = 'orange',
  CYAN_BLUE = 'cyan-blue',
  SKU_BLUE = 'sky-blue',
  DEEP_BLUE = 'deep-blue',
  NICKEL = 'nickel',
  VIOLET = 'violet',
  PURPLE = 'purple',
  INACTIVE = 'inactive-color',
}

export const orderCategoriesColorsDict = {
  [OrderCategories.NOW]: OrderCategoriesColors.MAGENTA,
  [OrderCategories.NEXT_UP]: OrderCategoriesColors.ORANGE,
  [OrderCategories.FINISHED]: OrderCategoriesColors.SKU_BLUE,
  [OrderCategories.FUTURE]: OrderCategoriesColors.VIOLET,
  [OrderCategories.FROZEN]: OrderCategoriesColors.INACTIVE,
};

export const wavesCategoriesColorsDict = {
  [WavesCategories.NEW]: OrderCategoriesColors.ORANGE,
  [WavesCategories.WAVE1]: OrderCategoriesColors.CYAN_BLUE,
  [WavesCategories.WAVE2]: OrderCategoriesColors.SKU_BLUE,
  [WavesCategories.WAVE3]: OrderCategoriesColors.DEEP_BLUE,
  [WavesCategories.WAVE4]: OrderCategoriesColors.NICKEL,
  [WavesCategories.WAVE5]: OrderCategoriesColors.VIOLET,
  [WavesCategories.WAVE6]: OrderCategoriesColors.PURPLE,
  [WavesCategories.WAVE7]: OrderCategoriesColors.DEEP_MAGENTA,
  [WavesCategories.WAVE8]: OrderCategoriesColors.MAGENTA,
  [WavesCategories.WAVE9]: OrderCategoriesColors.MAGENTA_PINK,
};

export const orderCategories = Object.values(OrderCategories).map((el, i) => ({
  id: i + 1,
  value: el,
  color: orderCategoriesColorsDict[el] as ButtonColor,
}));

export const wavesCategories = Object.values(WavesCategories).map((el, i) => ({
  id: i + 1,
  value: el,
  color: wavesCategoriesColorsDict[el] as ButtonColor,
}));
