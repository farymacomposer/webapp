import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { useEffect, useState } from 'react';
import { gap, sizeOpenBtn, sizeSidebar } from '../../../const/const.ts';

export const useContentWidth = () => {
  const [width, setWidth] = useState(0);

  const { width: screenWidth } = useScreenSize();

  useEffect(() => {
    const contentWidth = screenWidth - sizeSidebar - sizeOpenBtn - gap;

    setWidth(contentWidth);
  }, [screenWidth]);

  return width;
};
