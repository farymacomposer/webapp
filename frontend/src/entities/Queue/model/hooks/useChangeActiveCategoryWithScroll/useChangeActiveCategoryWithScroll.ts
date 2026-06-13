import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { type RefObject, useEffect, useState } from 'react';
import { headerHeight } from '../../const/sizes.ts';
import { useOrders } from '../../selectors/getOrders/getOrders.ts';
import { useQueueGroupView } from '../../selectors/getQueueGroupView/getQueueGroupView.ts';
import { queueActions } from '../../slice/queueSlice.ts';

interface IProps {
  refs: RefObject<Record<string, HTMLElement | null>>;
  containerRef?: RefObject<HTMLElement | null>;
}

export const useChangeActiveCategoryWithScroll = ({ refs, containerRef }: IProps) => {
  const [activeId, setActiveId] = useState<string | null>(null);

  const view = useQueueGroupView();
  const orders = useOrders();

  const dispatch = useAppDispatch();

  useEffect(() => {
    const container = containerRef?.current;

    if (!container) return;

    let ticking = false;

    const handleScroll = () => {
      if (ticking) return;

      ticking = true;

      requestAnimationFrame(() => {
        const containerRect = container.getBoundingClientRect();

        const elements = Object.values(refs.current).filter(Boolean) as HTMLElement[];

        let currentElement: HTMLElement | null = null;

        for (const el of elements) {
          const rect = el.getBoundingClientRect();

          const relativeBottom = rect.bottom - (containerRect.top - headerHeight);

          const isVisible = relativeBottom > headerHeight;

          if (isVisible) {
            currentElement = el;
            break;
          }
        }

        const nextId = currentElement ? currentElement.id : null;

        if (nextId !== activeId) {
          setActiveId(nextId);
        }

        ticking = false;
      });
    };

    handleScroll();

    container.addEventListener('scroll', handleScroll, {
      passive: true,
    });

    return () => {
      container.removeEventListener('scroll', handleScroll);
    };
  }, [orders, activeId, containerRef, refs]);

  useEffect(() => {
    if (!activeId) return;

    if (view === 'order') {
      dispatch(queueActions.changeActiveCategoryId(Number(activeId)));
    } else {
      dispatch(queueActions.changeActiveWaveId(Number(activeId)));
    }
  }, [activeId, view, dispatch]);

  return activeId;
};
