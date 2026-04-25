import { OrderCategoriesList } from '@entities/Order';
import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { FC, memo, useCallback } from 'react';
import cls from './SideQueueHeader.module.scss';

interface ISideQueueHeaderProps {}

export const SideQueueHeader: FC<ISideQueueHeaderProps> = memo(({}) => {
  const onClick = useCallback(
    (id: string) => () => {
      console.log(1);
      document?.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    },
    [],
  );

  return (
    <VStack className={cls.wrapper} gap="22" max>
      <HStack justify="between" gap="20" max>
        <Text>50&nbsp;треков</Text>
        <OrderCategoriesList onClick={onClick} />
      </HStack>
      <HStack justify="between" max>
        <HStack gap="4">группировка</HStack>
        поиск
      </HStack>
    </VStack>
  );
});
