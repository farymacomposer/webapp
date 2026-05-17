import { FC, memo, useCallback } from 'react';
import cls from './GroupView.module.scss';
import { HStack } from '@shared/ui/Stack';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Button } from '@shared/ui/Button';
import { queueActions, QueueGroupView, useQueueGroupView } from '@entities/Queue';
import { useAppDispatch } from '@shared/lib/hooks/useAppDispatch';

interface IProps {
  className?: string;
}

export const GroupView: FC<IProps> = memo(({ className }) => {
  const dispatch = useAppDispatch();

  const activeView = useQueueGroupView();
  const changeView = useCallback(
    (view: QueueGroupView) => () => {
      dispatch(queueActions.changeQueueGroupView(view));
    },
    [],
  );

  return (
    <HStack gap="4" className={classNames(cls.wrapper, {}, [className])}>
      <Button
        onClick={changeView('order')}
        className={classNames(cls.btn, {}, [className])}
        color={activeView === 'order' ? 'orange' : 'inactive-color'}
        fontColor={activeView === 'order' ? 'font-white' : 'font-gray'}
        variant={activeView === 'order' ? 'outline' : 'clear'}
        size="m"
      >
        по порядку
      </Button>
      <Button
        onClick={changeView('waves')}
        className={classNames(cls.btn, {}, [className])}
        color={activeView === 'waves' ? 'orange' : 'inactive-color'}
        fontColor={activeView === 'waves' ? 'font-white' : 'font-gray'}
        variant={activeView === 'waves' ? 'outline' : 'clear'}
        size="m"
      >
        по волнам
      </Button>
    </HStack>
  );
});
