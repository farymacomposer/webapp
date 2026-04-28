import { Button } from '@shared/ui/Button';
import cls from '@widgets/Sidebar/ui/Sidebar/Sidebar.module.scss';
import { memo } from 'react';

export const AddTrackButton = memo(() => {
  const addTrack = () => undefined;

  return (
    <Button
      fullWidth
      onClick={addTrack}
      className={cls.btn}
      color="neon-indigo"
      variant="filled"
      size="xl"
    >
      закинуть трек на разнос
    </Button>
  );
});
