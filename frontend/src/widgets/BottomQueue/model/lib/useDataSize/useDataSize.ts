import { type CategoryWithOrders, useCategories } from '@entities/Queue';
import { Breakpoints } from '@shared/const/breakpoints.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { useMemo } from 'react';
import { maxCardWidth } from '../../../const/const.ts';
import { useContentWidth } from '../useContentWidth/useContentWidth.ts';

const getSize = ({ data, contentWidth }: { data: CategoryWithOrders[]; contentWidth: number }) => {
  const amount = Math.max(Math.floor(contentWidth / maxCardWidth), 5);
  return data.slice(0, amount);
};

export const useDataSize = () => {
  const { width } = useScreenSize();
  const contentWidth = useContentWidth();
  const categories = useCategories();

  const data = useMemo(() => {
    return width > Breakpoints.XXXL
      ? getSize({ data: categories, contentWidth })
      : width > Breakpoints.XXL
        ? categories?.slice(0, 4)
        : categories?.slice(0, 3);
  }, [width, contentWidth, categories]);

  return { data };
};
