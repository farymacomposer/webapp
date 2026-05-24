import React, { memo } from 'react';
import { AppLink } from '../AppLink';
import cls from './Icon.module.scss';
import { classNames } from '@/shared/lib/classNames/classNames';

type SvgProps = Omit<React.SVGProps<SVGSVGElement>, 'onClick'>;

export type IconType = 'not-clickable' | 'link' | 'button';

interface IconBaseProps extends SvgProps {
  type: IconType;
  className?: string;
  Svg: React.ComponentType<React.SVGProps<SVGSVGElement>>;
  hover?: boolean;
}

interface NonClickableIconProps extends IconBaseProps {
  type: 'not-clickable';
}

interface ClickableButtonProps extends IconBaseProps {
  type: 'button';
  onClick: () => void;
}

interface ClickableLinkProps extends IconBaseProps {
  type: 'link';
  link: string;
}

type IconProps = NonClickableIconProps | ClickableButtonProps | ClickableLinkProps;

export const Icon = memo((props: IconProps) => {
  const {
    type = 'not-clickable',
    className,
    Svg,
    width = 32,
    height = 32,
    hover,
    ...otherProps
  } = props;

  const mods = {
    [cls.pointer]: type === 'button' || type === 'link',
    [cls.hover]: hover,
  };

  const icon = (
    <Svg
      className={classNames(cls.icon, mods, [className])}
      width={width}
      height={height}
      {...otherProps}
      onClick={undefined}
    />
  );

  if (type === 'button') {
    const buttonProps = props as ClickableButtonProps;
    return (
      <button
        type="button"
        className={classNames(cls.button, {}, [className])}
        onClick={buttonProps.onClick}
        style={{ height, width }}
      >
        {icon}
      </button>
    );
  }

  if (type === 'link') {
    const linkProps = props as ClickableLinkProps;
    return (
      <AppLink
        className={classNames(cls.button, {}, [className])}
        to={linkProps.link}
        target={linkProps.target}
      >
        {icon}
      </AppLink>
    );
  }

  return icon;
});
