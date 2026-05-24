import { buildSelector } from '@shared/store/buildSelector.ts';

export const [useActiveWaveId] = buildSelector((state) => state?.queue.activeWaveId);
