import Message from '@shared/assets/icons/message.svg';
import Spotify from '@shared/assets/icons/spotify.svg';
import Youtube from '@shared/assets/icons/youtube.svg';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { AppImage } from '@shared/ui/AppImage';
import { Icon } from '@shared/ui/Icon';
import { HStack, VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import { ReactElement, useCallback, useState } from 'react';
import { type Order } from '../../model/types/order.ts';
import { OrderCategory } from '../OrderCategory/OrderCategory.tsx';
import cls from './OrderCard.module.scss';
import { type OrderSmallVisibility } from './OrderCard.tsx';

export interface OrderCardProps {
  className?: string;
  order: Order;
  visibility: OrderSmallVisibility;
  onClick?: () => void;
  style?: React.CSSProperties;
  children?: ReactElement;
}

export const OrderCardSmall = (props: OrderCardProps) => {
  const { className, order, visibility, onClick, style, children } = props;
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
              hover
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
              hover
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
              hover
            />
          )}
        </HStack>
        <Text className={cls.price} size="18">
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
    <VStack
      onClick={onClick}
      className={classNames(cls.card, {}, [className, cls.small])}
      style={style}
    >
      {visibility === 'open' && openContent}
      <HStack className={cls.textBlock} gap="14" align="center">
        <OrderCategory name={order.category} fullHeight={visibility === 'open'} />
        <VStack gap="4">
          <Text
            className={classNames(cls.name, { [cls.openCardName]: visibility === 'open' }, [])}
            size="18"
            weight="bold"
          >
            {order.title}
          </Text>
          {visibility === 'open' && (
            <Text className={cls.user} size="18" style="italic">
              {'от ' + order.user}
            </Text>
          )}
        </VStack>
      </HStack>
      {children}
    </VStack>
  );
};
