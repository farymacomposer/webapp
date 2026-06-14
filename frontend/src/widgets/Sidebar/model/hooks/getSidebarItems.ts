import { type SidebarItemType } from '../types/sidebar';
import { getRouteMain } from '@/shared/const/router';

export const useSidebarItems = () => {
  const sidebarItemsList: SidebarItemType[] = [
    {
      path: getRouteMain(),
      text: 'стрим-space',
    },
    {
      path: '/test1',
      text: 'база треков',
    },
    {
      path: '/test2',
      text: 'FAQ',
    },
  ];

  return sidebarItemsList;
};
