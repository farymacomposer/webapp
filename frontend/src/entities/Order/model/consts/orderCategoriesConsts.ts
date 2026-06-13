export enum EOrderCategories {
  NOW = 'NOW',
  NEXT_UP = 'NEXT UP',
  FINISHED = 'FINISHED',
  FUTURE = 'FUTURE',
  FROZEN = 'FROZEN',
}

export enum EWavesCategories {
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
  [EOrderCategories.NOW]: OrderCategoriesColors.MAGENTA,
  [EOrderCategories.NEXT_UP]: OrderCategoriesColors.ORANGE,
  [EOrderCategories.FINISHED]: OrderCategoriesColors.SKU_BLUE,
  [EOrderCategories.FUTURE]: OrderCategoriesColors.VIOLET,
  [EOrderCategories.FROZEN]: OrderCategoriesColors.INACTIVE,
};

export const wavesCategoriesColorsDict = {
  [EWavesCategories.NEW]: OrderCategoriesColors.ORANGE,
  [EWavesCategories.WAVE1]: OrderCategoriesColors.CYAN_BLUE,
  [EWavesCategories.WAVE2]: OrderCategoriesColors.SKU_BLUE,
  [EWavesCategories.WAVE3]: OrderCategoriesColors.DEEP_BLUE,
  [EWavesCategories.WAVE4]: OrderCategoriesColors.NICKEL,
  [EWavesCategories.WAVE5]: OrderCategoriesColors.VIOLET,
  [EWavesCategories.WAVE6]: OrderCategoriesColors.PURPLE,
  [EWavesCategories.WAVE7]: OrderCategoriesColors.DEEP_MAGENTA,
  [EWavesCategories.WAVE8]: OrderCategoriesColors.MAGENTA,
  [EWavesCategories.WAVE9]: OrderCategoriesColors.MAGENTA_PINK,
};
