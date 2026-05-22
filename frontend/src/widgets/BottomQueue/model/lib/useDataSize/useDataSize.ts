import { Breakpoints } from '@shared/const/breakpoints.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { mockData } from '../../mockData/mockData.ts';
import { useMemo } from 'react';
import { Order } from '@entities/Order/model/types/order.ts';
import { useContentWidth } from '../useContentWidth/useContentWidth.ts';
import { maxCardWidth } from '../../../const/const.ts';

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
