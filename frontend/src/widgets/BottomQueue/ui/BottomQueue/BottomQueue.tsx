import { OpenSideQueueButton } from '@features/openSideQueue';
import { HStack } from '@shared/ui/Stack';
import { memo, useCallback, useState } from 'react';
import { useCardSize } from '../../model/lib/useCardHeight/useCardSize.ts';
import { useDataSize } from '../../model/lib/useDataSize/useDataSize.ts';
import { BottomQueueCard } from '../BottomQueueCard/BottomQueueCard.tsx';
import cls from './BottomQueue.module.scss';

interface IProps {
  onOpenSideQueue: () => void;
}

export const BottomQueue = memo(({ onOpenSideQueue }: IProps) => {
  const [visibilityId, setVisibilityId] = useState<number | null>(null);

  const data = useDataSize();
  const { width, height: openHeight } = useCardSize();

  const onChangeVisibility = useCallback(
    (id: number | null) => () => {
      setVisibilityId(id);
    },
    [setVisibilityId],
  );

  return (
    <HStack className={cls.wrapper} align="end" gap="20" max>
      <OpenSideQueueButton onClick={onOpenSideQueue} />
      <HStack justify="start" align="end" className={cls.cardsRow} gap="20">
        {data.map((el, i) => (
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
