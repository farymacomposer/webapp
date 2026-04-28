import { VStack } from '@shared/ui/Stack';
import { Text } from '@shared/ui/Text';
import cls from './ErrorPage.module.scss';
import { classNames } from '@/shared/lib/classNames/classNames';
import { Button } from '@/shared/ui/Button';

interface ErrorPageProps {
  className?: string;
}

export const ErrorPage = ({ className }: ErrorPageProps) => {
  const reloadPage = () => {
    location.reload();
  };

  return (
    <VStack gap="20" className={classNames(cls.errorPage, {}, [className])} align="center">
      <Text className={classNames(cls.text, {}, [className])} align="center">
        Произошла непредвиденная ошибка
      </Text>
      <Button size="l" onClick={reloadPage}>
        Обновить страницу
      </Button>
    </VStack>
  );
};
