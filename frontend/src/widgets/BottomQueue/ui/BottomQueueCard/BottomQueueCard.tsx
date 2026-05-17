import { OrderCard } from '@entities/Order';
import type { Order } from '@entities/Order/model/types/order.ts';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Overlay } from '@shared/ui/Overlay';
import { memo } from 'react';
import cls from './BottomQueueCard.module.scss';
import { Icon } from '@shared/ui/Icon';
import Arrow from '@shared/assets/icons/arrow.svg';

interface IProps {
  order: Order;
  isOpen: boolean;
  onClick: () => void;
  onClose: () => void;
  openHeight: number;
}

export const BottomQueueCard = memo(({ order, isOpen, onClick, onClose, openHeight }: IProps) => {
  const visibility = isOpen ? 'open' : 'close';

  const mods = { [cls.open]: isOpen };

  const arrow = (
    <Icon width="9" height="16" className={cls.iconWrapper} Svg={Arrow} type="not-clickable" />
  );

  return (
    <>
      <OrderCard
        view="small"
        visibility={visibility}
        className={classNames(cls.card, mods, [])}
        order={order}
        onClick={onClick}
        style={isOpen ? { height: openHeight } : {}}
        children={order.id !== 1 ? arrow : undefined} //todo
      />
      {isOpen && <Overlay onClick={onClose} />}
    </>
  );
});
