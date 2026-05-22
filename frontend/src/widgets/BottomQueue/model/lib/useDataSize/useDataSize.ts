import { type Order } from '@entities/Order/model/types/order.ts';
import { Breakpoints } from '@shared/const/breakpoints.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { useMemo } from 'react';
import { maxCardWidth } from '../../../const/const.ts';
import { mockData } from '../../mockData/mockData.ts';
import { useContentWidth } from '../useContentWidth/useContentWidth.ts';

const getSize = ({ data, contentWidth }: { data: Order[]; contentWidth: number }) => {
  const amount = Math.max(Math.floor(contentWidth / maxCardWidth), 5);
  return data.slice(0, amount);
};

export const useDataSize = () => {
  const { width } = useScreenSize();
  const contentWidth = useContentWidth();

  const data = useMemo(() => {
    return width > Breakpoints.XXXL
      ? getSize({ data: mockData, contentWidth })
      : width > Breakpoints.XXL
        ? mockData.slice(0, 4)
        : mockData.slice(0, 3);
  }, [width, contentWidth]);

  return data;
};
