import { queueActions, useOrders } from '@entities/Queue';
import { OpenSideQueueButton } from '@features/openSideQueue';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { HStack } from '@shared/ui/Stack';
import { type FlexGap } from '@shared/ui/Stack/Flex/Flex.tsx';
import { memo, useCallback } from 'react';
import { gap as gapConst } from '../../const/const.ts';
import { useCardSize } from '../../model/lib/useCardHeight/useCardSize.ts';
import { useDataSize } from '../../model/lib/useDataSize/useDataSize.ts';
import { BottomQueueCard } from '../BottomQueueCard/BottomQueueCard.tsx';
import cls from './BottomQueue.module.scss';

export const BottomQueue = memo(() => {
  const orders = useOrders();
  const dispatch = useAppDispatch();

  const { data } = useDataSize();
  const { height: openHeight } = useCardSize();

  const onOpenSideQueue = useCallback(() => {
    dispatch(queueActions.changeOpen(true));
  }, [dispatch]);

  const gap = String(gapConst) as FlexGap;

  return (
    <HStack className={cls.wrapper} align="end" gap={gap} max>
      <OpenSideQueueButton onClick={onOpenSideQueue} />
      <HStack justify="start" align="end" className={cls.cardsRow} gap={gap}>
        {data.map((el, i) => (
          <BottomQueueCard
            key={el.id}
            order={orders[el.id]}
            openHeight={openHeight}
            first={i === 0}
          />
        ))}
      </HStack>
    </HStack>
  );
});
