import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import { type BottomQueueSchema, type OpenCardId } from '../types/bottomQueue.ts';

const initialState: BottomQueueSchema = {
  openCardId: null,
};

export const bottomQueueSlice = createSlice({
  name: 'bottomQueue',
  initialState,
  reducers: {
    changeOpenCardId: (state, { payload }: PayloadAction<OpenCardId>) => {
      state.openCardId = payload;
    },
  },
});

export const { actions: bottomQueueActions } = bottomQueueSlice;
export const { reducer: bottomQueueReducer } = bottomQueueSlice;
