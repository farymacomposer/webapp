import { OpenSideQueueButton } from '@features/openSideQueue';
import { HStack } from '@shared/ui/Stack';
import { memo, useCallback, useState } from 'react';
import { useCardSize } from '../../model/lib/useCardHeight/useCardSize.ts';
import { useDataSize } from '../../model/lib/useDataSize/useDataSize.ts';
import { BottomQueueCard } from '../BottomQueueCard/BottomQueueCard.tsx';
import cls from './BottomQueue.module.scss';
import { gap as gapConst } from '../../const/const.ts';
import { FlexGap } from '@shared/ui/Stack/Flex/Flex.tsx';

interface IProps {
  onOpenSideQueue: () => void;
}

export const BottomQueue = memo(({ onOpenSideQueue }: IProps) => {
  const [visibilityId, setVisibilityId] = useState<number | null>(null);

  const data = useDataSize();
  const { height: openHeight } = useCardSize();

  const onChangeVisibility = useCallback(
    (id: number | null) => () => {
      setVisibilityId(id);
    },
    [setVisibilityId],
  );

  const gap = String(gapConst) as FlexGap;

  return (
    <HStack className={cls.wrapper} align="end" gap={gap} max>
      <OpenSideQueueButton onClick={onOpenSideQueue} />
      <HStack justify="start" align="end" className={cls.cardsRow} gap={gap}>
        {data.map((el) => (
          <BottomQueueCard
            key={el.id}
            order={el}
            isOpen={visibilityId === el.id}
            onClick={onChangeVisibility(el.id)}
            onClose={onChangeVisibility(null)}
            openHeight={openHeight}
          />
        ))}
      </HStack>
    </HStack>
  );
});
