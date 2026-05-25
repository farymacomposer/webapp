import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import {
  type ActiveCategoryId,
  type ActiveWaveId,
  type QueueGroupView,
  type QueueSchema,
} from '../types/queue.ts';

const initialState: QueueSchema = {
  groupView: 'order',
  activeCategoryId: null,
  activeWaveId: null,
};

export const queueSlice = createSlice({
  name: 'queue',
  initialState,
  reducers: {
    changeQueueGroupView: (state, { payload }: PayloadAction<QueueGroupView>) => {
      state.groupView = payload;
    },

    changeActiveCategoryId: (state, { payload }: PayloadAction<ActiveCategoryId>) => {
      state.activeCategoryId = payload;
    },

    changeActiveWaveId: (state, { payload }: PayloadAction<ActiveWaveId>) => {
      state.activeWaveId = payload;
    },
  },
});

export const { actions: queueActions } = queueSlice;
export const { reducer: queueReducer } = queueSlice;
