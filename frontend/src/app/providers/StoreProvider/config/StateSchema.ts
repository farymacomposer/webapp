import { type QueueSchema } from '@entities/Queue';
import { type BottomQueueSchema } from '@widgets/BottomQueue';

export interface StateSchema {
  queue: QueueSchema;
  bottomQueue: BottomQueueSchema;
}
