import { memo } from 'react';
import { TextField } from '../TextField';

interface SearchProps {
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
   * Текст по умолчанию внутри инпута
   */
  label?: string;
  /**
   * Дополнительный класс
   */
  className?: string;
}

export const Search = memo((props: SearchProps) => {
  return <TextField {...props} />;
});
