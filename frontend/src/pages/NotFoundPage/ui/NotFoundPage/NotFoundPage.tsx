import { Page } from '@widgets/Page';
import { VStack } from '@shared/ui/Stack';
import { classNames } from '@shared/lib/classNames/classNames.ts';
import cls from './NotFoundPage.module.scss';
import { getRouteMain } from '@shared/const/router.ts';
import { AppLink } from '@shared/ui/AppLink';
import { Text } from '@shared/ui/Text';

interface NotFoundPageProps {
  className?: string;
}

export const NotFoundPage = ({ className }: NotFoundPageProps) => {
  return (
    <Page className={cls.notFoundPage}>
      <VStack gap="20" className={classNames(cls.errorPage, {}, [className])} align="center">
        <Text className={classNames(cls.text, {}, [className])} align="center">
          Страница не найдена
        </Text>
        <AppLink className={cls.link} to={getRouteMain()}>
          Вернуться на главную
        </AppLink>
      </VStack>
    </Page>
  );
};
