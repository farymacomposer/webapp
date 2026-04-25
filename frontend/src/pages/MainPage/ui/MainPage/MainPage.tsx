import { VStack } from '@shared/ui/Stack';
import { BottomQueue } from '@widgets/BottomQueue';
import { Page } from '@widgets/Page';
import { SideQueue } from '@widgets/SideQueue';
import { TwitchPlayer } from '@widgets/TwitchPlayer';
import { useCallback, useState } from 'react';
import cls from './MainPage.module.scss';

const MainPage = () => {
  const [isOpenSideQueue, setIsOpenSideQueue] = useState(false);

  const onOpen = useCallback(() => {
    setIsOpenSideQueue(true);
  }, [setIsOpenSideQueue]);

  const onClose = useCallback(() => {
    setIsOpenSideQueue(false);
  }, [setIsOpenSideQueue]);

  return (
    <Page>
      <VStack className={cls.wrapper} gap="10">
        <TwitchPlayer />
        <BottomQueue onOpenSideQueue={onOpen} />
        <SideQueue isOpen={isOpenSideQueue} onClose={onClose} />
      </VStack>
    </Page>
  );
};

export default MainPage;
