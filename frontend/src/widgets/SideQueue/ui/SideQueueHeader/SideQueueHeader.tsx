import { OrderCategoriesList } from '@entities/Order';
import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { memo, useCallback } from 'react';
import cls from './SideQueueHeader.module.scss';
import { GroupView } from '@features/changeGroupView';
import { useHorizontalDrag } from '@shared/lib/hooks/useHorizontalDrag/useHorizontalDrag.tsx';

export const SideQueueHeader = memo(() => {
  const scrollRef = useHorizontalDrag<HTMLDivElement>();

  const onClick = useCallback(
    (id: string) => () => {
      document?.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    },
    [],
  );

  return (
    <VStack className={cls.wrapper} gap="22" max>
      <HStack ref={scrollRef} className={cls.row} justify="between" gap="20" max>
        <Text>50&nbsp;треков</Text>
        <OrderCategoriesList onClick={onClick} />
      </HStack>
      <HStack justify="between" max>
        <HStack gap="4">
          <GroupView />
        </HStack>
      </HStack>
    </VStack>
  );
});
