import type { ButtonColor } from '@shared/ui/Button';
import {
  EOrderCategories,
  EWavesCategories,
  orderCategoriesColorsDict,
  wavesCategoriesColorsDict,
} from '@/entities/Order';

export const orderCategories = Object.values(EOrderCategories).map((el, i) => ({
  id: i + 1,
  value: el,
  shortValue: el,
  color: orderCategoriesColorsDict[el] as ButtonColor,
}));

export const wavesCategories = Object.values(EWavesCategories).map((el, i) => ({
  id: i + 1,
  value: el,
  shortValue: el.slice(0, 1) + el.slice(-1),
  color: wavesCategoriesColorsDict[el] as ButtonColor,
}));
