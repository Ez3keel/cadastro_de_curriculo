import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ApiError } from '../api/client';
import { obterCandidato } from '../api/candidatos';
import type { Candidato } from '../types/candidato';

function formatarData(data: string) {
  return new Date(data).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
}

function Campo({ label, valor }: { label: string; valor: string | null }) {
  return (
    <div>
      <dt className="text-sm font-medium text-gray-500">{label}</dt>
      <dd className="mt-1 whitespace-pre-wrap text-gray-900">{valor ?? '-'}</dd>
    </div>
  );
}

export function DetalhesPage() {
  const { id } = useParams<{ id: string }>();
  const [candidato, setCandidato] = useState<Candidato | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [naoEncontrado, setNaoEncontrado] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;

    obterCandidato(Number(id))
      .then(setCandidato)
      .catch((error) => {
        if (error instanceof ApiError && error.status === 404) {
          setNaoEncontrado(true);
        } else {
          setErro('Não foi possível carregar os dados do candidato.');
        }
      })
      .finally(() => setCarregando(false));
  }, [id]);

  return (
    <div className="mx-auto max-w-2xl p-6">
      <Link to="/" className="mb-6 inline-block text-sm text-indigo-600 hover:underline">
        &larr; Voltar para a listagem
      </Link>

      {carregando && <p className="text-gray-500">Carregando...</p>}

      {naoEncontrado && <p className="text-red-600">Candidato não encontrado.</p>}

      {erro && <p className="text-red-600">{erro}</p>}

      {candidato && (
        <div className="rounded-md border border-gray-200 bg-white p-6">
          <h1 className="mb-4 text-2xl font-semibold text-gray-900">{candidato.nomeCompleto}</h1>
          <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Campo label="E-mail" valor={candidato.email} />
            <Campo label="Telefone" valor={candidato.telefone} />
            <Campo label="Área de interesse" valor={candidato.areaInteresse} />
            <Campo label="Cadastrado em" valor={formatarData(candidato.criadoEm)} />
            <div className="sm:col-span-2">
              <Campo label="Resumo profissional" valor={candidato.resumoProfissional} />
            </div>
          </dl>
        </div>
      )}
    </div>
  );
}
