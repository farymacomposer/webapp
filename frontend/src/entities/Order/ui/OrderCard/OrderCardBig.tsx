import Message from '@shared/assets/icons/message.svg';
import Youtube from '@shared/assets/icons/youtube.svg';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Icon } from '@shared/ui/Icon';
import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { memo, useCallback, useState } from 'react';
import { type Order } from '../../model/types/order.ts';
import { OrderCover } from '../OrderCover/OrderCover.tsx';
import cls from './OrderCard.module.scss';

export interface OrderCardProps {
  className?: string;
  order: Order;
}

export const OrderCardBig = memo((props: OrderCardProps) => {
  const { className, order } = props;
  const [showMessage, setShowMessage] = useState(false);

  const onClickMessage = useCallback(() => {
    setShowMessage((prev) => !prev);
  }, [setShowMessage]);

  const messageMods = { [cls.show]: showMessage };

  return (
    <HStack
      gap="10"
      justify="between"
      align="stretch"
      className={classNames(cls.card, {}, [className, cls.big])}
    >
      <OrderCover src={order.img} />
      <VStack className={cls.textBlock} gap="6">
        <VStack max>
          <Text className={cls.name} size="18" weight="bold">
            {order.title}
          </Text>
          <Text className={cls.author} size="16" style="italic">{`от ${order.user}`}</Text>
        </VStack>
        {order.youtubeLink && (
          <Icon
            className={cls.youtubeLink}
            Svg={Youtube}
            type="link"
            link={order.youtubeLink}
            width={26}
            height={26}
            target="_blank"
          />
        )}
        {order.comment && (
          <Text className={classNames(cls.comment, messageMods, [])} size="16">
            {order.comment}
          </Text>
        )}
      </VStack>
      <VStack className={cls.priceBlock}>
        <Text className={cls.price} size="20" align="right">
          {`${order.price}₽`}
        </Text>
        {order.comment && (
          <HStack className={cls.iconsBlock} gap="14">
            <Icon
              className={classNames(cls.messageLink, messageMods, [])}
              Svg={Message}
              width={28}
              height={28}
              type="button"
              onClick={onClickMessage}
            />
          </HStack>
        )}
      </VStack>
    </HStack>
  );
});
