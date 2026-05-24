import Cross from '@shared/assets/icons/cross.svg';
import { type ReactNode } from 'react';
import { Icon } from '../Icon';
import { Overlay } from '../Overlay';
import { Portal } from '../Portal';
import cls from './Modal.module.scss';
import { classNames, type Mods } from '@/shared/lib/classNames/classNames';
import { useModal } from '@/shared/lib/hooks/useModal/useModal';

interface ModalProps {
  className?: string;
  children?: ReactNode;
  isOpen?: boolean;
  onClose?: () => void;
  lazy?: boolean;
  left?: boolean;
  right?: boolean;
  fullHeight?: boolean;
  closeIcon?: boolean;
}

const ANIMATION_DELAY = 300;

export const Modal = (props: ModalProps) => {
  const { className, children, isOpen, onClose, lazy, left, right, fullHeight, closeIcon } = props;

  const { close, isClosing, isMounted } = useModal({
    animationDelay: ANIMATION_DELAY,
    onClose,
    isOpen,
  });

  const mods: Mods = {
    [cls.opened]: isOpen,
    [cls.isClosing]: isClosing,
    [cls.left]: left,
    [cls.right]: right,
    [cls.fullHeight]: fullHeight,
  };

  if (lazy && !isMounted) {
    return null;
  }

  return (
    <Portal element={document.getElementById('app') ?? document.body}>
      <div className={classNames(cls.modal, mods, ['app_modal'])}>
        <Overlay onClick={close} />
        <div className={classNames(cls.content, {}, [className])}>
          {closeIcon && onClose && (
            <Icon
              width={22}
              height={22}
              className={cls.closeIcon}
              Svg={Cross}
              type="button"
              onClick={onClose}
            />
          )}
          {children}
        </div>
      </div>
    </Portal>
  );
};
