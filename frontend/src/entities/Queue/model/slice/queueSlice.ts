import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import { getOpenCategoriesOrders } from '../libs/getOpenCategoriesOrders/getOpenCategoriesOrders.ts';
import {
  type ActiveCategoryId,
  type ActiveWaveId,
  type CategoryWithOrders,
  type OrdersListSchema,
  type QueueGroupView,
  type QueueSchema,
  type OpenCardId,
} from '../types/queue.ts';

const initialState: QueueSchema = {
  orders: {},
  categories: [],
  waves: [],
  open: false,
  groupView: 'order',
  activeCategoryId: null,
  activeWaveId: null,
  openCardId: null,
};

export const queueSlice = createSlice({
  name: 'queue',
  initialState,
  reducers: {
    changeOrders: (state, { payload }: PayloadAction<OrdersListSchema>) => {
      state.orders = payload;
    },

    changeCategories: (state, { payload }: PayloadAction<CategoryWithOrders[]>) => {
      state.categories = getOpenCategoriesOrders({ categories: payload, prev: state.categories });
    },

    changeWaves: (state, { payload }: PayloadAction<CategoryWithOrders[]>) => {
      state.waves = getOpenCategoriesOrders({ categories: payload, prev: state.waves });
    },

    changeOrdersWithCategories: (
      state,
      {
        payload,
      }: PayloadAction<{
        orders: OrdersListSchema;
        categories: CategoryWithOrders[];
        waves: CategoryWithOrders[];
      }>,
    ) => {
      state.orders = payload.orders;
      state.categories = getOpenCategoriesOrders({
        categories: payload.categories,
        prev: state.categories,
      });
      state.waves = getOpenCategoriesOrders({
        categories: payload.waves,
        prev: state.waves,
      });
    },

    changeOpen: (state, { payload }: PayloadAction<boolean>) => {
      state.open = payload;
    },

    changeQueueGroupView: (state, { payload }: PayloadAction<QueueGroupView>) => {
      state.groupView = payload;
    },

    changeActiveCategoryId: (state, { payload }: PayloadAction<ActiveCategoryId>) => {
      state.activeCategoryId = payload;
    },

    changeActiveWaveId: (state, { payload }: PayloadAction<ActiveWaveId>) => {
      state.activeWaveId = payload;
    },

    changeOpenCardId: (state, { payload }: PayloadAction<OpenCardId>) => {
      state.openCardId = payload;
    },

    changeNumberOfCategoryOpenCards: (
      state,
      { payload }: PayloadAction<{ categoryId: number; type?: 'show' | 'hide' }>,
    ) => {
      const { categoryId, type = 'show' } = payload;
      const categoryType = state.groupView;
      if (categoryType === 'order') {
        state.categories = getOpenCategoriesOrders({
          categories: state.categories,
          prev: state.categories,
          openCategoryId: categoryId,
          type,
        });
      } else {
        state.waves = getOpenCategoriesOrders({
          categories: state.waves,
          prev: state.waves,
          openCategoryId: categoryId,
          type,
        });
      }
    },
  },
});

export const { actions: queueActions } = queueSlice;
export const { reducer: queueReducer } = queueSlice;
