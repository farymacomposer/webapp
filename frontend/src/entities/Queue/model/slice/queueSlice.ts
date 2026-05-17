import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { QueueGroupView, QueueSchema } from '../types/queue.ts';

const initialState: QueueSchema = {
  groupView: 'order',
};

export const queueSlice = createSlice({
  name: 'queue',
  initialState,
  reducers: {
    changeQueueGroupView: (state, { payload }: PayloadAction<QueueGroupView>) => {
      state.groupView = payload;
    },
  },
});

export const { actions: queueActions } = queueSlice;
export const { reducer: queueReducer } = queueSlice;
