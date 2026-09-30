import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { listarCandidatos } from '../api/candidatos';
import type { CandidatoListItem } from '../types/candidato';

function formatarData(data: string) {
  return new Date(data).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
}

export function ListagemPage() {
  const [candidatos, setCandidatos] = useState<CandidatoListItem[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    listarCandidatos()
      .then(setCandidatos)
      .catch(() => setErro('Não foi possível carregar a listagem de candidatos.'))
      .finally(() => setCarregando(false));
  }, []);

  return (
    <div className="mx-auto max-w-4xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Candidatos</h1>
        <Link
          to="/candidatos/novo"
          className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
        >
          Novo candidato
        </Link>
      </div>

      {carregando && <p className="text-gray-500">Carregando...</p>}

      {erro && <p className="text-red-600">{erro}</p>}

      {!carregando && !erro && candidatos.length === 0 && (
        <p className="text-gray-500">Nenhum candidato cadastrado ainda.</p>
      )}

      {!carregando && !erro && candidatos.length > 0 && (
        <table className="w-full overflow-hidden rounded-md border border-gray-200 bg-white text-left text-sm">
          <thead className="bg-gray-50 text-gray-600">
            <tr>
              <th className="px-4 py-3 font-medium">Nome</th>
              <th className="px-4 py-3 font-medium">E-mail</th>
              <th className="px-4 py-3 font-medium">Área de interesse</th>
              <th className="px-4 py-3 font-medium">Cadastrado em</th>
            </tr>
          </thead>
          <tbody>
            {candidatos.map((candidato) => (
              <tr
                key={candidato.id}
                onClick={() => navigate(`/candidatos/${candidato.id}`)}
                className="cursor-pointer border-t border-gray-100 hover:bg-gray-50"
              >
                <td className="px-4 py-3 text-gray-900">{candidato.nomeCompleto}</td>
                <td className="px-4 py-3 text-gray-600">{candidato.email}</td>
                <td className="px-4 py-3 text-gray-600">{candidato.areaInteresse ?? '-'}</td>
                <td className="px-4 py-3 text-gray-600">{formatarData(candidato.criadoEm)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
