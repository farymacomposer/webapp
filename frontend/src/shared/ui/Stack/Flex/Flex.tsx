import { type DetailedHTMLProps, forwardRef, type HTMLAttributes, type ReactNode } from 'react';
import cls from './Flex.module.scss';
import { classNames, type Mods } from '@/shared/lib/classNames/classNames';

export type FlexJustify = 'start' | 'center' | 'end' | 'between';
export type FlexAlign = 'start' | 'center' | 'end' | 'stretch';
export type FlexDirection = 'row' | 'column';
export type FlexWrap = 'nowrap' | 'wrap';
export type FlexGap = '4' | '8' | '10' | '14' | '16' | '20' | '22' | '24' | '32';

const justifyClasses: Record<FlexJustify, string> = {
  start: cls.justifyStart,
  center: cls.justifyCenter,
  end: cls.justifyEnd,
  between: cls.justifyBetween,
};

const alignClasses: Record<FlexAlign, string> = {
  start: cls.alignStart,
  center: cls.alignCenter,
  end: cls.alignEnd,
  stretch: cls.stretch,
};

const directionClasses: Record<FlexDirection, string> = {
  row: cls.directionRow,
  column: cls.directionColumn,
};

const gapClasses: Record<FlexGap, string> = {
  4: cls.gap4,
  8: cls.gap8,
  10: cls.gap10,
  14: cls.gap14,
  16: cls.gap16,
  20: cls.gap20,
  22: cls.gap22,
  24: cls.gap24,
  32: cls.gap32,
};

type DivProps = DetailedHTMLProps<HTMLAttributes<HTMLDivElement>, HTMLDivElement>;

export interface FlexProps extends DivProps {
  className?: string;
  children: ReactNode;
  justify?: FlexJustify;
  align?: FlexAlign;
  direction: FlexDirection;
  wrap?: FlexWrap;
  gap?: FlexGap;
  /**
   * Блок растягивается по ширине
   */
  max?: boolean;
  /**
   * Блок растягивается по высоте
   */
  maxHeight?: boolean;
}

export const Flex = forwardRef<HTMLDivElement, FlexProps>((props, ref) => {
  const {
    className,
    children,
    justify = 'start',
    align = 'center',
    direction = 'row',
    wrap = 'nowrap',
    gap,
    max,
    maxHeight,
    ...otherProps
  } = props;

  const classes = [
    className,
    justifyClasses[justify],
    alignClasses[align],
    directionClasses[direction],
    cls[wrap],
    gap && gapClasses[gap],
  ];

  const mods: Mods = {
    [cls.max]: max,
    [cls.maxHeight]: maxHeight,
  };

  return (
    <div ref={ref} className={classNames(cls.flex, mods, classes)} {...otherProps}>
      {children}
    </div>
  );
});
