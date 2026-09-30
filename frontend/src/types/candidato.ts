export interface CandidatoListItem {
  id: number;
  nomeCompleto: string;
  email: string;
  areaInteresse: string | null;
  criadoEm: string;
}

export interface Candidato {
  id: number;
  nomeCompleto: string;
  email: string;
  telefone: string | null;
  areaInteresse: string | null;
  resumoProfissional: string | null;
  criadoEm: string;
}

export interface CandidatoRequest {
  nomeCompleto: string;
  email: string;
  telefone?: string;
  areaInteresse?: string;
  resumoProfissional?: string;
}
