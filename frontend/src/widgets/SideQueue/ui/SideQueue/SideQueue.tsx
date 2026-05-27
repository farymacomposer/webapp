import { OrderCardsCategoryList } from '@entities/Order';
import { queueActions, useQueueGroupView, useQueueOpenState } from '@entities/Queue';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { Modal } from '@shared/ui/Modal';
import { memo, useCallback, useRef } from 'react';
import { mockDataOrders, mockDataWaves } from '../../model/mockData/mockData';
import { SideQueueHeader } from '../SideQueueHeader/SideQueueHeader.tsx';
import cls from './SideQueue.module.scss';

export const SideQueue = memo(() => {
  const activeView = useQueueGroupView();
  const isOpen = useQueueOpenState();

  const dispatch = useAppDispatch();

  const onClose = useCallback(() => {
    dispatch(queueActions.changeOpen(false));
  }, [dispatch]);

  const mockData = activeView === 'order' ? mockDataOrders : mockDataWaves;

  const ref = useRef<HTMLDivElement | null>(null);

  return (
    <Modal
      className={cls.modal}
      isOpen={isOpen}
      onClose={onClose}
      lazy={true}
      left
      fullHeight
      closeIcon
    >
      <SideQueueHeader />
      <div className={cls.container} ref={ref}>
        <OrderCardsCategoryList
          orders={mockData}
          containerRef={ref}
          scrollWithChangingActiveCategory
        />
      </div>
    </Modal>
  );
});
