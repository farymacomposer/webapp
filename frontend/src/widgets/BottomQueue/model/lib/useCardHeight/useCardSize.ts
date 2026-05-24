import { Breakpoints } from '@shared/const/breakpoints.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { useEffect, useState } from 'react';
import { gap, heightContentWithoutImage, maxCardWidth } from '../../../const/const.ts';
import { useContentWidth } from '../useContentWidth/useContentWidth.ts';

export const useCardSize = () => {
  const [height, setHeight] = useState(0);
  const [width, setWidth] = useState(0);

  const { width: screenWidth } = useScreenSize();
  const contentWidth = useContentWidth();

  useEffect(() => {
    const cardsAmount = screenWidth > Breakpoints.XXXL ? 5 : screenWidth > Breakpoints.XXL ? 4 : 3;
    const cardWidth = Math.min(
      (contentWidth - gap * (cardsAmount - 1)) / cardsAmount,
      maxCardWidth,
    );

    const newHeight = heightContentWithoutImage + cardWidth;
    setHeight(newHeight);
    setWidth(cardWidth);
  }, [screenWidth, contentWidth]);

  return { width, height };
};
