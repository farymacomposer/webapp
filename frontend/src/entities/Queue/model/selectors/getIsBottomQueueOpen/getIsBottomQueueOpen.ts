import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useIsBottomQueueOpen] = buildSelector((state) => !!state?.queue.openCardId);
