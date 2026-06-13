import { OrderCard } from '@entities/Order';
import type { Order } from '@entities/Order/model/types/order.ts';
import { useIsBottomQueueOpen } from '@entities/Queue/model/selectors/getIsBottomQueueOpen/getIsBottomQueueOpen.ts';
import { useOpenCardId } from '@entities/Queue/model/selectors/getOpenCardId/getOpenCardId.ts';
import Arrow from '@shared/assets/icons/arrow.svg';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { Icon } from '@shared/ui/Icon';
import { Overlay } from '@shared/ui/Overlay';
import { memo, useCallback } from 'react';
import cls from './BottomQueueCard.module.scss';
import { queueActions } from '@/entities/Queue';

interface IProps {
  order: Order;
  openHeight: number;
  first: boolean;
}

export const BottomQueueCard = memo(({ order, openHeight, first }: IProps) => {
  const dispatch = useAppDispatch();
  const isBottomQueueOpen = useIsBottomQueueOpen();
  const openCardId = useOpenCardId();

  const isOpen = openCardId === order.id;
  const visibility = openCardId === order.id ? 'open' : 'close';

  const mods = { [cls.open]: isOpen, [cls.overOverlay]: isBottomQueueOpen };

  const onClick = useCallback(() => {
    dispatch(queueActions.changeOpenCardId(order.id));
  }, [dispatch, order]);

  const onClose = useCallback(() => {
    dispatch(queueActions.changeOpenCardId(null));
  }, [dispatch]);

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
        children={!first ? arrow : undefined}
      />
      {isOpen && <Overlay onClick={onClose} />}
    </>
  );
});
