import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useActiveCategories] = buildSelector((state) =>
  state?.queue.groupView === 'order' ? state?.queue.categories : state?.queue.waves,
);
