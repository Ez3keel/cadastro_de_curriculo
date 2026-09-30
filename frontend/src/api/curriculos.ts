import { apiClient } from './client';
import type { CurriculoExtracao } from '../types/curriculo';

export function extrairCurriculo(arquivo: File) {
  const formData = new FormData();
  formData.append('arquivo', arquivo);

  return apiClient.post<CurriculoExtracao>('/api/curriculos/extrair', formData);
}
