import { queueActions } from '@entities/Queue';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';
import { useInitialEffect } from '@shared/lib/hooks/useInitialEffect';
import { VStack } from '@shared/ui/Stack';
import { BottomQueue } from '@widgets/BottomQueue';
import { Page } from '@widgets/Page';
import { SideQueue } from '@widgets/SideQueue';
import { TwitchPlayer } from '@widgets/TwitchPlayer';
import { mockOrders, mockCategories, mockWaves } from '../../model/mockData/mockData.ts';
import cls from './MainPage.module.scss';

const MainPage = () => {
  const dispatch = useAppDispatch();

  useInitialEffect(() => {
    dispatch(
      queueActions.changeOrdersWithCategories({
        orders: mockOrders,
        categories: mockCategories,
        waves: mockWaves,
      }),
    );
  });

  return (
    <Page>
      <VStack className={cls.wrapper} gap="10">
        <div className={cls.twitchOverlay}></div>
        <TwitchPlayer />
        <BottomQueue />
        <SideQueue />
      </VStack>
    </Page>
  );
};

export default MainPage;
