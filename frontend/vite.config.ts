import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import svgr from 'vite-plugin-svgr';

export default defineConfig({
  plugins: [
    react(),
    svgr({
      svgrOptions: {
        icon: true,
      },
      include: '**/*.svg',
    }),
  ],
  server: {
    port: 3000,
    open: true,
  },
  resolve: {
    alias: [
      { find: '@', replacement: '/src' },
      { find: '@app', replacement: '/src/app' },
      { find: '@pages', replacement: '/src/pages' },
      { find: '@widgets', replacement: '/src/widgets' },
      { find: '@features', replacement: '/src/features' },
      { find: '@entities', replacement: '/src/entities' },
      { find: '@shared', replacement: '/src/shared' },
    ],
  },
  css: {
    modules: {
      scopeBehaviour: 'local',
      generateScopedName: '[name]__[local]___[hash:base64:5]',
    },
    preprocessorOptions: {
      scss: {
        additionalData: `@use '@/app/styles/variables/mixins' as *;`,
      },
    },
  },
});
