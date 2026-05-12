import { useState, useEffect } from 'react';
import { Breakpoints } from '../../../const/breakpoints.ts';

const debounce = (fn: () => void, delay: number) => {
  let timeout: ReturnType<typeof setTimeout>;
  return () => {
    clearTimeout(timeout);
    timeout = setTimeout(fn, delay);
  };
};

export const useScreenSize = (delay = 10) => {
  const [width, setWidth] = useState(window.innerWidth);

  useEffect(() => {
    const handleResize = debounce(() => {
      setWidth(window.innerWidth);
    }, delay);

    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, [delay]);

  return {
    width,
    isMobile: width < Breakpoints.S,
    isTablet: width >= Breakpoints.S && width < Breakpoints.LG,
    isDesktop: width >= Breakpoints.LG,
  };
};
