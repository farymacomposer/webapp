import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useQueueGroupView] = buildSelector((state) => state?.queue.groupView);
