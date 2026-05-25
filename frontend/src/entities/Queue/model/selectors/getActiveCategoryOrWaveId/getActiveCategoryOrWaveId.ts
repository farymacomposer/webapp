import { buildSelector } from '@shared/store/buildSelector.ts';

// возвращается значение в зависимости от выбранного типа отображения очереди (по порядку или по волнам)
export const [useActiveCategoryOrWaveId] = buildSelector((state) => {
  if (state?.queue.groupView === 'order') {
    return state?.queue.activeCategoryId;
  }
  return state?.queue.activeWaveId;
});
