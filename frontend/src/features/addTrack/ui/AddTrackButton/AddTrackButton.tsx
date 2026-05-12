import { Button } from '@shared/ui/Button';
import cls from './AddTrackButton.module.scss';
import { FC, memo } from 'react';
import { classNames } from '@shared/lib/classNames/classNames.ts';

interface IProps {
  className?: string;
}

export const AddTrackButton: FC<IProps> = memo(({ className }) => {
  const addTrack = () => {
    console.log(addTrack);
  };

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
