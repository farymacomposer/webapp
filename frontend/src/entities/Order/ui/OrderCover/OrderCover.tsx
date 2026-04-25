import { classNames } from '@shared/lib/classNames/classNames.ts';
import { AppImage } from '@shared/ui/AppImage';
import { Skeleton } from '@shared/ui/Skeleton';
import { type CSSProperties, memo, useMemo } from 'react';
import cls from '../OrderCard/OrderCard.module.scss';

interface OrderCoverProps {
  className?: string;
  src?: string;
  size?: number;
  alt?: string;
}

export const OrderCover = memo(({ className, src, size = 112, alt = 'cover' }: OrderCoverProps) => {
  const styles = useMemo<CSSProperties>(
    () => ({
      width: size,
      height: size,
    }),
    [size],
  );

  const fallback = <Skeleton width={size} height={size} border="10px" />;

  return (
    <AppImage
      className={classNames(cls.cover, {}, [className])}
      src={src}
      alt={alt}
      style={styles}
      fallback={fallback}
    />
  );
});
