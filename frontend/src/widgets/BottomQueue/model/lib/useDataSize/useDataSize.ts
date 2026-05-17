import { Breakpoints } from '@shared/const/breakpoints.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { mockData } from '../../mockData/mockData.ts';

export const useDataSize = () => {
  const { width } = useScreenSize();

  const data =
    width > Breakpoints.XXXL
      ? mockData.slice(0, 5)
      : width > Breakpoints.XXL
        ? mockData.slice(0, 4)
        : mockData.slice(0, 3);

  return data;
};
