import { Breakpoints } from '@shared/const/breakpoints.ts';
import { mockData } from '../../mockData/mockData.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';

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
