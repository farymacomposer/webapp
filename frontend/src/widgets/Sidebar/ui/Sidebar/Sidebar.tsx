import { useQueueOpenState } from '@entities/Queue';
import { AddTrackButton } from '@features/addTrack';
import { getRouteMain } from '@shared/const/router.ts';
import { TwitchChat } from '@widgets/TwitchChat';
import { memo, useMemo } from 'react';
import { useLocation } from 'react-router-dom';
import { useIsBottomQueueOpen } from '../../../BottomQueue';
import { useSidebarItems } from '../../model/selectors/getSidebarItems';
import { SidebarItem } from '../SidebarItem/SidebarItem';
import cls from './Sidebar.module.scss';
import { classNames, type Mods } from '@/shared/lib/classNames/classNames';
import { HStack, VStack } from '@/shared/ui/Stack';

interface SidebarProps {
  className?: string;
}

export const Sidebar = memo(({ className }: SidebarProps) => {
  const sidebarItemsList = useSidebarItems();
  const { pathname } = useLocation();

  const itemsList = useMemo(
    () => sidebarItemsList.map((item) => <SidebarItem item={item} key={item.path} />),
    [sidebarItemsList],
  );

  const isMainPage = pathname === getRouteMain();
  const isSideQueueOpen = useQueueOpenState();
  const isBottomQueueOpen = useIsBottomQueueOpen();

  const mods: Mods = {
    [cls.relativeSidebar]: isMainPage,
    [cls.overOverlay]: isSideQueueOpen || isBottomQueueOpen,
  };

  return (
    <aside data-testid="sidebar" className={classNames(cls.sidebar, mods, [className])}>
      <VStack className={cls.sidebar}>
        <HStack className={cls.items} role="navigation" justify="between" gap="4" max>
          {itemsList}
        </HStack>
        {isMainPage && (
          <VStack gap="4" max maxHeight>
            <AddTrackButton className={cls.btn} />
            <TwitchChat />
          </VStack>
        )}
      </VStack>
    </aside>
  );
});
