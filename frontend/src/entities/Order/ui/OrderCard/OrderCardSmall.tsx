import Message from '@shared/assets/icons/message.svg';
import Spotify from '@shared/assets/icons/spotify.svg';
import Youtube from '@shared/assets/icons/youtube.svg';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { useModal } from '@shared/lib/hooks/useModal';
import { AppImage } from '@shared/ui/AppImage';
import { Icon } from '@shared/ui/Icon';
import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { memo, useCallback, useState } from 'react';
import { type Order } from '../../model/types/order.ts';
import { OrderCategory } from '../OrderCategory/OrderCategory.tsx';
import cls from './OrderCard.module.scss';
import { type OrderSmallVisibility } from './OrderCard.tsx';

export interface OrderCardProps {
  className?: string;
  order: Order;
  visibility: OrderSmallVisibility;
  onClick?: () => void;
}

export const OrderCardSmall = memo((props: OrderCardProps) => {
  const { className, order, visibility, onClick } = props;
  const [showMessage, setShowMessage] = useState(false);

  const onClickMessage = useCallback(() => {
    setShowMessage((prev) => !prev);
  }, [setShowMessage]);

  const messageMods = { [cls.show]: showMessage };
  const imageMods = { [cls.transparent]: showMessage };

  const openContent = (
    <>
      <HStack className={cls.iconsBlock} gap="14" justify="between" align="center" max>
        <HStack gap="14">
          {order.youtubeLink && (
            <Icon
              className={cls.youtubeLink}
              Svg={Youtube}
              type="link"
              link={order.youtubeLink}
              width={30}
              height={25}
              target="_blank"
            />
          )}
          {order.spotifyLink && (
            <Icon
              className={cls.spotifyLink}
              Svg={Spotify}
              type="link"
              link={order.spotifyLink}
              width={25}
              height={25}
              target="_blank"
            />
          )}
          {order.comment && (
            <Icon
              className={classNames(cls.messageLink, messageMods, [])}
              Svg={Message}
              type="button"
              width={30}
              height={25}
              onClick={onClickMessage}
            />
          )}
        </HStack>
        <Text className={cls.name} size="18">
          {order.price + '₽'}
        </Text>
      </HStack>
      <div className={cls.imgWrapper}>
        <AppImage className={classNames(cls.img, imageMods, [])} src={order.img} />
        {showMessage && order.comment && (
          <div className={cls.commentWrapper}>
            <Text className={cls.comment} size="18" style="italic">
              {order.comment}
            </Text>
          </div>
        )}
      </div>
    </>
  );

  return (
    <VStack onClick={onClick} className={classNames(cls.card, {}, [className, cls.small])}>
      {visibility === 'open' && openContent}
      <HStack className={cls.textBlock} gap="14" align="start">
        <OrderCategory name={order.category} />
        <Text className={cls.name} size="16" weight="bold">
          {order.title}
        </Text>
      </HStack>
    </VStack>
  );
});
