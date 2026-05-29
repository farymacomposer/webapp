import { OrderCategoriesList } from '@entities/Order';
import { GroupView } from '@features/changeGroupView';
import { useHorizontalDrag } from '@shared/lib/hooks/useHorizontalDrag/useHorizontalDrag.tsx';
import { Search } from '@shared/ui/Search';
import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { memo, useCallback } from 'react';
import cls from './SideQueueHeader.module.scss';

export const SideQueueHeader = memo(() => {
  const scrollRef = useHorizontalDrag<HTMLDivElement>();

  const onCategoryClick = useCallback(
    (id: number) => () => {
      document
        ?.getElementById(`${id}-category`)
        ?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    },
    [],
  );

  return (
    <VStack className={cls.wrapper} gap="16" max>
      <HStack justify="between" gap="36" max>
        <Text size="18">50&nbsp;треков</Text>
        <HStack ref={scrollRef} className={cls.row} justify="between" gap="36" max>
          <OrderCategoriesList onClick={onCategoryClick} />
        </HStack>
      </HStack>
      <HStack justify="between" max>
        <HStack gap="4">
          <GroupView />
        </HStack>
        <Search label="Поиск по треку или нику" />
      </HStack>
    </VStack>
  );
});
