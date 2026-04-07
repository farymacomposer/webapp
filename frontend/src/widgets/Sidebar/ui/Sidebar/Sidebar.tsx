import { TwitchChat } from '@widgets/TwitchChat';
import { memo, useMemo } from 'react';
import { useSidebarItems } from '../../model/selectors/getSidebarItems';
import { SidebarItem } from '../SidebarItem/SidebarItem';
import cls from './Sidebar.module.scss';
import { classNames } from '@/shared/lib/classNames/classNames';
import { HStack, VStack } from '@/shared/ui/Stack';
import { useLocation } from 'react-router-dom';
import { getRouteMain } from '@shared/const/router.ts';
import { AddTrackButton } from '@features/addTrack';

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

  return (
    <aside data-testid="sidebar" className={classNames(cls.sidebar, {}, [className])}>
      <VStack className={cls.sidebar}>
        <HStack className={cls.items} role="navigation" justify="between" gap="4" max>
          {itemsList}
        </HStack>
        {isMainPage && (
          <>
            <AddTrackButton />
            <TwitchChat />
          </>
        )}
      </VStack>
    </aside>
  );
});
