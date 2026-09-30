import { apiClient } from './client';
import type { Candidato, CandidatoListItem, CandidatoRequest } from '../types/candidato';

export function listarCandidatos() {
  return apiClient.get<CandidatoListItem[]>('/api/candidatos');
}

export function obterCandidato(id: number) {
  return apiClient.get<Candidato>(`/api/candidatos/${id}`);
}

export function criarCandidato(dados: CandidatoRequest) {
  return apiClient.post<Candidato>('/api/candidatos', dados);
}
