import { QueueCategoriesList } from '@entities/Queue';
import { GroupView } from '@features/changeGroupView';
import { Search } from '@shared/ui/Search';
import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { memo, useCallback } from 'react';
import cls from './SideQueueHeader.module.scss';

export const SideQueueHeader = memo(() => {
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
        <QueueCategoriesList onClick={onCategoryClick} />
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
