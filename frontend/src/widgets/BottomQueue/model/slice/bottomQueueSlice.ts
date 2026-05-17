import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { BottomQueueSchema } from '../types/bottomQueue.ts';

const initialState: BottomQueueSchema = {
  openCardHeight: 0,
};

export const bottomQueueSlice = createSlice({
  name: 'bottomQueue',
  initialState,
  reducers: {
    changeBottomQueueCardHeight: (state, { payload }: PayloadAction<number>) => {
      state.openCardHeight = payload;
    },
  },
});

export const { actions: bottomQueueActions } = bottomQueueSlice;
export const { reducer: bottomQueueReducer } = bottomQueueSlice;
