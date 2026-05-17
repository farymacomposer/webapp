import { QueueSchema } from '@entities/Queue';
import { BottomQueueSchema } from '@widgets/BottomQueue';

export interface StateSchema {
  queue: QueueSchema;
  bottomQueue: BottomQueueSchema;
}
