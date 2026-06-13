import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Button, type ButtonColor } from '@shared/ui/Button';
import { Text } from '@shared/ui/Text';
import { memo } from 'react';
import { rectangleCategoryNameHeight } from '../../model/consts/sizes.ts';
import cls from './OrderCategory.module.scss';

export type CategoryView = 'square' | 'rectangle' | 'button';

interface OrderCardBaseProps {
  id: number;
  className?: string;
  view?: CategoryView;
  name: string;
  color?: ButtonColor;
  fullHeight?: boolean;
  active?: boolean;
}

type OrderCardSquareProps = OrderCardBaseProps;

interface OrderCardRectangleProps extends OrderCardBaseProps {}

interface OrderCardButtonProps extends OrderCardBaseProps {
  onClick: (id: number) => () => void;
}

export type OrderCardProps = OrderCardSquareProps | OrderCardRectangleProps | OrderCardButtonProps;

export const OrderCategory = memo((props: OrderCardProps) => {
  const {
    className,
    view = 'square',
    id,
    name,
    color = 'magenta',
    fullHeight,
    active = true,
  } = props;

  if (view === 'button') {
    const { onClick } = props as OrderCardButtonProps;

    return (
      <Button
        className={classNames(cls.btn, {}, [className])}
        color={active ? color : 'dark-gray'}
        variant="filled"
        onClick={onClick(id)}
      >
        <span>{name}</span>
      </Button>
    );
  }

  if (view === 'rectangle') {
    return (
      <div
        id={String(id) + '-category'}
        className={classNames(cls.category, {}, [className, cls[view], cls[color]])}
        style={{ height: rectangleCategoryNameHeight + 'px' }}
      >
        <Text size={'14'} align="center" weight="bold">
          {name}
        </Text>
      </div>
    );
  }

  return (
    <div
      className={classNames(cls.category, { [cls.fullHeight]: fullHeight }, [
        className,
        cls[view],
        cls[color],
      ])}
    >
      <Text size={'12'} align="center" weight="bold">
        {name}
      </Text>
    </div>
  );
});
