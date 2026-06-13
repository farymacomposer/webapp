import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useCategories] = buildSelector((state) => state?.queue.categories);
