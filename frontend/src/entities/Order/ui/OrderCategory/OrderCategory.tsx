import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Button, type ButtonColor } from '@shared/ui/Button';
import { Text } from '@shared/ui/Text';
import { memo } from 'react';
import cls from './OrderCategory.module.scss';

export type CategoryView = 'square' | 'rectangle' | 'button';

interface OrderCardBaseProps {
  className?: string;
  view?: CategoryView;
  name: string;
  color?: ButtonColor;
}

type OrderCardSquareProps = OrderCardBaseProps;

interface OrderCardRectangleProps extends OrderCardBaseProps {
  id: string;
}

interface OrderCardButtonProps extends OrderCardBaseProps {
  onClick: (id: string) => () => void;
}

export type OrderCardProps = OrderCardSquareProps | OrderCardRectangleProps | OrderCardButtonProps;

export const OrderCategory = memo((props: OrderCardProps) => {
  const { className, view = 'square', name, color = 'magenta' } = props;

  if (view === 'button') {
    const { onClick } = props as OrderCardButtonProps;

    return (
      <Button
        className={classNames(cls.btn, {}, [className])}
        color={color}
        variant="filled"
        onClick={onClick(name)}
      >
        {name}
      </Button>
    );
  }

  if (view === 'rectangle') {
    const { id } = props as OrderCardRectangleProps;

    return (
      <div id={id} className={classNames(cls.category, {}, [className, cls[view], cls[color]])}>
        <Text size={'18'} align="center" weight="bold">
          {name}
        </Text>
      </div>
    );
  }

  return (
    <div className={classNames(cls.category, {}, [className, cls[view], cls[color]])}>
      <Text size={'12'} align="center" weight="bold">
        {name}
      </Text>
    </div>
  );
});
