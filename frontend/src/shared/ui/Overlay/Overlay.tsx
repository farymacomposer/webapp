import { memo, useEffect, useState } from 'react';
import cls from './Overlay.module.scss';
import { classNames, type Mods } from '@/shared/lib/classNames/classNames';

interface OverlayProps {
  className?: string;
  /**
   * Колбэк для закрытия
   */
  onClick?: () => void;
}

export const Overlay = memo((props: OverlayProps) => {
  const { className, onClick } = props;
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    setVisible(false);

    const raf = requestAnimationFrame(() => {
      setVisible(true);
    });

    return () => cancelAnimationFrame(raf);
  }, []);

  const handleClose = () => {
    setVisible(false);
    onClick?.();
  };

  const mods: Mods = {
    [cls.visible]: visible,
  };

  return <div onClick={handleClose} className={classNames(cls.overlay, mods, [className])} />;
});
