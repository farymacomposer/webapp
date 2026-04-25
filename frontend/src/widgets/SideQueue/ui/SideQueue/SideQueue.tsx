import { OrderCardsCategoryList } from '@entities/Order';
import { Modal } from '@shared/ui/Modal';
import { memo } from 'react';
import { mockData } from '../../model/mockData/mockData';
import { SideQueueHeader } from '../SideQueueHeader/SideQueueHeader.tsx';
import cls from './SideQueue.module.scss';

interface IProps {
  isOpen: boolean;
  onClose: () => void;
}

export const SideQueue = memo((props: IProps) => {
  const { isOpen, onClose } = props;

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
      <div className={cls.container}>
        <OrderCardsCategoryList orders={mockData} />
      </div>
    </Modal>
  );
});
