export type QueueGroupView = 'order' | 'waves';

export type ActiveCategoryId = number | null;

export type ActiveWaveId = number | null;

export interface QueueSchema {
  groupView: QueueGroupView;
  activeCategoryId: ActiveCategoryId;
  activeWaveId: ActiveWaveId;
}
