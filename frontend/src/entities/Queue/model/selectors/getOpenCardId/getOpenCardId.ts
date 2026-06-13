import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useOpenCardId] = buildSelector((state) => state?.queue.openCardId);
