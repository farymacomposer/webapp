import { memo, useEffect, useRef, useState } from 'react';
import { classNames, Mods } from '../../lib/classNames/classNames.ts';
import cls from './TextField.module.scss';

export type TextFieldSize = 'big' | 'small';

interface TextFieldProps {
  /**
   * Значение
   */
  value?: string | number;
  /**
   * Функция для изменения значения
   */
  onChange?: (value: string) => void;
  /**
   * Автофокус
   */
  autofocus?: boolean;
  /**
   * Нередактируемое состояние
   */
  readonly?: boolean;
  /**
   * Текст по умолчанию внутри инпута
   */
  label?: string;
  /**
   * Размер
   */
  size?: TextFieldSize;
  /**
   * Дополнительный класс
   */
  className?: string;
}

export const TextField = memo((props: TextFieldProps) => {
  const { value, onChange, autofocus, readonly, className, label, size = 'small' } = props;
  const ref = useRef<HTMLInputElement>(null);
  const [isFocused, setIsFocused] = useState(false);

  useEffect(() => {
    if (autofocus) {
      setIsFocused(true);
      ref.current?.focus();
    }
  }, [autofocus]);

  const onChangeHandler = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange?.(e.target.value);
  };

  const onBlur = () => {
    setIsFocused(false);
  };

  const onFocus = () => {
    setIsFocused(true);
  };

  const additionalClasses = [className, cls[size]];

  const mods: Mods = {
    [cls.readonly]: readonly,
    [cls.focused]: isFocused,
  };

  return (
    <div className={classNames(cls.wrapper, {}, [])}>
      <input
        ref={ref}
        value={value}
        onChange={onChangeHandler}
        onFocus={onFocus}
        onBlur={onBlur}
        readOnly={readonly}
        className={classNames(cls.textfield, mods, additionalClasses)}
        placeholder={label}
      />
    </div>
  );
});
