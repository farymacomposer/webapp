import { classNames } from '@shared/lib/classNames/classNames.ts';
import { Sidebar } from '@widgets/Sidebar';
import { memo, Suspense } from 'react';
import { AppRouter } from './providers/Router';

const App = memo(() => {
  return (
    <div id="app" className={classNames('app', {}, [])}>
      <Suspense fallback="">
        <AppRouter />
        <Sidebar />
      </Suspense>
    </div>
  );
});

export default App;
