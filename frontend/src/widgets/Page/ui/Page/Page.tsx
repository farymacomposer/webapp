import { memo, type ReactNode } from 'react';
import cls from './Page.module.scss';
import { classNames } from '@/shared/lib/classNames/classNames';
import { type TestProps } from '@/shared/types/tests';

interface PageProps extends TestProps {
  className?: string;
  children: ReactNode;
}

export const Page = memo((props: PageProps) => {
  const { className, children } = props;

  return (
    <main
      className={classNames(cls.page, {}, [className])}
      data-testid={props['data-testid'] ?? 'Page'}
    >
      {children}
    </main>
  );
});
