import { combineReducers, configureStore } from '@reduxjs/toolkit';
import { queueReducer } from '@entities/Queue';
import { bottomQueueReducer } from '@widgets/BottomQueue';

const rootReducer = combineReducers({
  queue: queueReducer,
  bottomQueue: bottomQueueReducer,
});

export const store = configureStore({
  reducer: rootReducer,
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
