import { Button } from '@shared/ui/Button';
import { memo } from 'react';
import cls from './OpenSideQueueButton.module.scss';

interface IProps {
  onClick: () => void;
}

export const OpenSideQueueButton = memo(({ onClick }: IProps) => {
  return (
    <Button className={cls.btn} variant="clear" onClick={onClick}>
      +
    </Button>
  );
});
