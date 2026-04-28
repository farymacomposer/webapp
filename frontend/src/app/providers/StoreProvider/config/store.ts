import { configureStore } from '@reduxjs/toolkit';

const rootReducer = (state = {}) => state;

export const store = configureStore({
  reducer: rootReducer,
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
