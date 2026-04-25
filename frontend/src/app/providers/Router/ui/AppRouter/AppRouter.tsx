import { PageLoader } from '@widgets/PageLoader';
import { memo, Suspense } from 'react';
import { Route, Routes } from 'react-router-dom';
import { routeConfig } from '../../config/routeConfig.tsx';

const AppRouter = () => {
  return (
    <Routes>
      {Object.values(routeConfig).map((route) => (
        <Route
          key={route.path}
          path={route.path}
          element={<Suspense fallback={<PageLoader />}>{route.element}</Suspense>}
        />
      ))}
    </Routes>
  );
};

export default memo(AppRouter);
