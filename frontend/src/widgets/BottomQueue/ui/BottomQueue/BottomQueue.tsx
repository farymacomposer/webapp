import { OpenSideQueueButton } from '@features/openSideQueue';
import { Breakpoints } from '@shared/const/breakpoints.ts';
import { useScreenSize } from '@shared/lib/hooks/useScreenSize';
import { HStack } from '@shared/ui/Stack';
import { memo, useCallback, useState } from 'react';
import { mockData } from '../../model/mockData/mockData';
import { BottomQueueCard } from '../BottomQueueCard/BottomQueueCard.tsx';
import cls from './BottomQueue.module.scss';

interface IProps {
  onOpenSideQueue: () => void;
}

export const BottomQueue = memo(({ onOpenSideQueue }: IProps) => {
  const [visibilityId, setVisibilityId] = useState<number | null>(null);
  const { width } = useScreenSize();

  const data =
    width > Breakpoints.XXXL
      ? mockData
      : width > Breakpoints.XXL
        ? mockData.slice(0, 4)
        : mockData.slice(0, 3);

  const onChangeVisibility = useCallback(
    (id: number | null) => () => {
      setVisibilityId(id);
    },
    [setVisibilityId],
  );

  return (
    <HStack className={cls.wrapper} gap="20" max>
      <OpenSideQueueButton onClick={onOpenSideQueue} />
      <HStack justify="start" className={cls.cardsRow} gap="20">
        {data.map((el) => (
          <BottomQueueCard
            key={el.id}
            order={el}
            isOpen={visibilityId === el.id}
            onClick={onChangeVisibility(el.id)}
            onClose={onChangeVisibility(null)}
          />
        ))}
      </HStack>
    </HStack>
  );
});
