import { Breakpoints } from '@shared/const/breakpoints.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { useEffect, useState } from 'react';

const sizeSidebar = 340 + 10; // size + gap
const sizeOpenBtn = 74;
const gap = 20;
const heightContentWithoutImage = 190;

export const useCardSize = () => {
  const [height, setHeight] = useState(0);
  const [width, setWidth] = useState(0);

  const { width: screenWidth } = useScreenSize();

  useEffect(() => {
    const cardsAmount = screenWidth > Breakpoints.XXXL ? 5 : screenWidth > Breakpoints.XXL ? 4 : 3;
    const cardWidth = Math.min(
      (screenWidth - sizeSidebar - sizeOpenBtn - gap - gap * (cardsAmount - 1)) / cardsAmount,
      374,
    );

    const newHeight = heightContentWithoutImage + cardWidth;
    setHeight(newHeight);
    setWidth(cardWidth);
  }, [screenWidth]);

  return { width, height };
};
