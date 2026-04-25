import { memo } from 'react';
import cls from './Text.module.scss';
import { classNames } from '@/shared/lib/classNames/classNames';

export type TextVariant = 'primary' | 'error';

export type TextAlign = 'right' | 'left' | 'center';

export type TextSize = '12' | '16' | '18' | '20' | '22' | '24';

export type TextWeight = 'regular' | 'medium' | 'bold';

export type TextStyle = 'normal' | 'italic';

interface TextProps {
  /**
   * Выравнивание
   */
  align?: TextAlign;
  /**
   * Текст
   */
  children: string;
  /**
   * Размер
   */
  size?: TextSize;
  /**
   * Дополнительный класс
   */
  className?: string;
  /**
   * Id для тестов
   */
  'data-testid'?: string;
  /**
   * Начертание
   */
  style?: TextStyle;
  /**
   * Цвет текста
   */
  variant?: TextVariant;
  /**
   * Толщина
   */
  weight?: TextWeight;
}

const mapSizeToClass: Record<TextSize, string> = {
  '12': cls.size_12,
  '16': cls.size_16,
  '18': cls.size_18,
  '20': cls.size_20,
  '22': cls.size_22,
  '24': cls.size_24,
};

export const Text = memo((props: TextProps) => {
  const {
    className,
    children,
    variant = 'primary',
    align = 'left',
    size = '18',
    weight = 'regular',
    style = 'normal',
    'data-testid': dataTestId = 'Text',
  } = props;

  const sizeClass = mapSizeToClass[size];

  const additionalClasses = [
    className,
    cls[variant],
    cls[align],
    cls[weight],
    cls[style],
    sizeClass,
  ];

  return (
    <p
      className={classNames(cls.text, {}, additionalClasses)}
      data-testid={`${dataTestId}.Paragraph`}
    >
      {children}
    </p>
  );
});
