import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useQueueOpenState] = buildSelector((state) => state?.queue.open);
