import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useActiveCategoryId] = buildSelector((state) => state?.queue.activeCategoryId);
