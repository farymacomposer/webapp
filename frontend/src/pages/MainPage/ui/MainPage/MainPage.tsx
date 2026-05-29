import { VStack } from '@shared/ui/Stack';
import { BottomQueue } from '@widgets/BottomQueue';
import { Page } from '@widgets/Page';
import { SideQueue } from '@widgets/SideQueue';
import { TwitchPlayer } from '@widgets/TwitchPlayer';
import cls from './MainPage.module.scss';

const MainPage = () => {
  return (
    <Page>
      <VStack className={cls.wrapper} gap="10">
        <TwitchPlayer />
        <BottomQueue />
        <SideQueue />
      </VStack>
    </Page>
  );
};

export default MainPage;
