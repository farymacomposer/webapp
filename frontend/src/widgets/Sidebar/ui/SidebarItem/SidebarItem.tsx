import { memo } from 'react';
import { type SidebarItemType } from '../../model/types/sidebar';
import cls from './SidebarItem.module.scss';
import { classNames } from '@/shared/lib/classNames/classNames';
import { AppLink } from '@/shared/ui/AppLink';

interface SidebarItemProps {
  item: SidebarItemType;
}

export const SidebarItem = memo(({ item }: SidebarItemProps) => {
  return (
    <AppLink to={item.path} className={classNames(cls.item, {})} activeClassName={cls.active}>
      <span className={cls.link}>{item.text}</span>
    </AppLink>
  );
});
