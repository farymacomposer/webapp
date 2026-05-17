import { type ButtonHTMLAttributes, type ForwardedRef, forwardRef, type ReactNode } from 'react';
import cls from './Button.module.scss';
import { classNames, type Mods } from '@/shared/lib/classNames/classNames';

export type ButtonVariant = 'clear' | 'outline' | 'filled';

export type ButtonColor =
  | 'orange'
  | 'deep-orange'
  | 'magenta'
  | 'magenta-pink'
  | 'deep-magenta'
  | 'purple'
  | 'violet'
  | 'neon-indigo'
  | 'nickel'
  | 'deep-blue'
  | 'sky-blue'
  | 'cyan-blue'
  | 'inactive-color';

export type ButtonFontColor = 'font-white' | 'font-gray';

export type ButtonSize = 'm' | 'l' | 'xl';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  className?: string;
  /**
   * Тема кнопки. Отвечает за визуал (в рамке, без стилей, c заливкой)
   */
  variant?: ButtonVariant;
  /**
   * Размер кнопки в соответствии с дизайн системой
   */
  size?: ButtonSize;
  /**
   * Флаг, отвечающий за работу кнопки
   */
  disabled?: boolean;
  /**
   * Содержимое кнопки
   */
  children?: ReactNode;
  /**
   * Увеличивает кнопку на всю свободную ширину
   */
  fullWidth?: boolean;
  /**
   * Цвет
   */
  color?: ButtonColor;
  /**
   * Цвет шрифта
   */
  fontColor?: ButtonFontColor;
  /**
   * Блок слева от текста
   */
  addonLeft?: ReactNode;
  /**
   * Блок справа от текста
   */
  addonRight?: ReactNode;
}

export const Button = forwardRef((props: ButtonProps, ref: ForwardedRef<HTMLButtonElement>) => {
  const {
    className,
    children,
    variant = 'outline',
    disabled,
    fullWidth,
    size = 'm',
    addonLeft,
    addonRight,
    color = 'orange',
    fontColor = 'font-white',
    ...otherProps
  } = props;

  const mods: Mods = {
    [cls.disabled]: disabled,
    [cls.fullWidth]: fullWidth,
    [cls.withAddon]: Boolean(addonLeft) || Boolean(addonRight),
  };

  return (
    <button
      type="button"
      className={classNames(cls.button, mods, [
        className,
        cls[variant],
        cls[size],
        cls[color],
        cls[fontColor],
      ])}
      disabled={disabled}
      {...otherProps}
      ref={ref}
    >
      {addonLeft && <div className={cls.addonLeft}>{addonLeft}</div>}
      {children}
      {addonRight && <div className={cls.addonRight}>{addonRight}</div>}
    </button>
  );
});
