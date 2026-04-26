import { OrderCard } from '@entities/Order';
import type { Order } from '@entities/Order/model/types/order.ts';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Overlay } from '@shared/ui/Overlay';
import { memo } from 'react';
import cls from './BottomQueueCard.module.scss';

interface IProps {
  order: Order;
  isOpen: boolean;
  onClick: () => void;
  onClose: () => void;
}

export const BottomQueueCard = memo(({ order, isOpen, onClick, onClose }: IProps) => {
  const visibility = isOpen ? 'open' : 'close';

  const mods = { [cls.open]: isOpen };

  return (
    <>
      <OrderCard
        view="small"
        visibility={visibility}
        className={classNames(cls.card, mods, [])}
        order={order}
        onClick={onClick}
      />
      {isOpen && <Overlay onClick={onClose} />}
    </>
  );
});
