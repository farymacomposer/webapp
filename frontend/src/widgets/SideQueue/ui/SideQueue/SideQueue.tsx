import { OrderCardsCategoryList } from '@entities/Order';
import { useQueueGroupView } from '@entities/Queue';
import { Modal } from '@shared/ui/Modal';
import { memo, useRef } from 'react';
import { mockDataOrders, mockDataWaves } from '../../model/mockData/mockData';
import { SideQueueHeader } from '../SideQueueHeader/SideQueueHeader.tsx';
import cls from './SideQueue.module.scss';

interface IProps {
  isOpen: boolean;
  onClose: () => void;
}

export const SideQueue = memo((props: IProps) => {
  const { isOpen, onClose } = props;
  const activeView = useQueueGroupView();

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
