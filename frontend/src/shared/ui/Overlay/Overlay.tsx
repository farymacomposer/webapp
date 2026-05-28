import { memo } from 'react';
import cls from './Overlay.module.scss';
import { classNames } from '@/shared/lib/classNames/classNames';

interface OverlayProps {
  className?: string;
  /**
   * Колбэк для закрытия
   */
  onClick?: () => void;
}

export const Overlay = memo((props: OverlayProps) => {
  const { className, onClick } = props;

  return <div onClick={onClick} className={classNames(cls.overlay, {}, [className])} />;
});
