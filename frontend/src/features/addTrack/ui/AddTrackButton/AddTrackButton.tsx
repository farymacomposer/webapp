import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Button } from '@shared/ui/Button';
import { type FC, memo } from 'react';
import cls from './AddTrackButton.module.scss';

interface IProps {
  className?: string;
}

export const AddTrackButton: FC<IProps> = memo(({ className }) => {
  const addTrack = () => undefined;

  return (
    <Button
      fullWidth
      onClick={addTrack}
      className={classNames(cls.btn, {}, [className])}
      color="neon-indigo"
      variant="filled"
      size="xl"
    >
      закинуть трек на разнос
    </Button>
  );
});
