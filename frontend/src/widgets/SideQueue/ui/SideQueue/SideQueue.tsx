import { queueActions, useQueueOpenState, QueueListWithCategories } from '@entities/Queue';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { Modal } from '@shared/ui/Modal';
import { memo, useCallback, useRef } from 'react';
import { SideQueueHeader } from '../SideQueueHeader/SideQueueHeader.tsx';
import cls from './SideQueue.module.scss';

export const SideQueue = memo(() => {
  const isOpen = useQueueOpenState();

  const dispatch = useAppDispatch();

  const ref = useRef<HTMLDivElement | null>(null);

  const onClose = useCallback(() => {
    dispatch(queueActions.changeOpen(false));
  }, [dispatch]);

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
        <QueueListWithCategories containerRef={ref} scrollWithChangingActiveCategory />
      </div>
    </Modal>
  );
});
