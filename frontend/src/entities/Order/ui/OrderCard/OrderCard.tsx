import { memo } from 'react';
import { type Order } from '../../model/types/order.ts';
import { OrderCardBig } from './OrderCardBig.tsx';
import { OrderCardSmall } from './OrderCardSmall.tsx';

export type OrderView = 'big' | 'small';

export type OrderSmallVisibility = 'close' | 'open';

export interface OrderCardBaseProps {
  className?: string;
  order: Order;
  view: OrderView;
}

interface BigOrderCardProps extends OrderCardBaseProps {
  view: 'big';
}

interface SmallOrderCardProps extends OrderCardBaseProps {
  view: 'small';
  visibility: OrderSmallVisibility;
  onClick?: () => void;
}

type OrderCardProps = BigOrderCardProps | SmallOrderCardProps;

export const OrderCard = memo((props: OrderCardProps) => {
  const { className, order, view = 'small' } = props;

  if (view === 'small') {
    const orderProps = props as SmallOrderCardProps;
    return (
      <OrderCardSmall
        className={className}
        order={order}
        visibility={orderProps.visibility}
        onClick={orderProps.onClick}
      />
    );
  }

  return <OrderCardBig className={className} order={order} />;
});
