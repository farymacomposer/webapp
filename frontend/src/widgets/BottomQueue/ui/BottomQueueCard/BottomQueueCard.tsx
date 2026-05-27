import { OrderCard } from '@entities/Order';
import type { Order } from '@entities/Order/model/types/order.ts';
import Arrow from '@shared/assets/icons/arrow.svg';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { Icon } from '@shared/ui/Icon';
import { Overlay } from '@shared/ui/Overlay';
import { memo, useCallback } from 'react';
import { useIsBottomQueueOpen } from '../../model/selectors/getIsBottomQueueOpen/getIsBottomQueueOpen.ts';
import { useOpenCardId } from '../../model/selectors/getOpenCardId/getOpenCardId.ts';
import { bottomQueueActions } from '../../model/slice/bottomQueueSlice.ts';
import cls from './BottomQueueCard.module.scss';

interface IProps {
  order: Order;
  openHeight: number;
}

export const BottomQueueCard = memo(({ order, openHeight }: IProps) => {
  const dispatch = useAppDispatch();
  const isBottomQueueOpen = useIsBottomQueueOpen();
  const openCardId = useOpenCardId();

  const isOpen = openCardId === order.id;
  const visibility = openCardId === order.id ? 'open' : 'close';

  const mods = { [cls.open]: isOpen, [cls.overOverlay]: isBottomQueueOpen };

  const onClick = useCallback(() => {
    dispatch(bottomQueueActions.changeOpenCardId(order.id));
  }, [dispatch, order]);

  const onClose = useCallback(() => {
    dispatch(bottomQueueActions.changeOpenCardId(null));
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
        children={order.id !== 1 ? arrow : undefined} //todo
      />
      {isOpen && <Overlay onClick={onClose} />}
    </>
  );
});
