import { Link } from 'react-router-dom';

export function NovoCandidatoPage() {
  return (
    <div className="mx-auto max-w-2xl p-6">
      <Link to="/" className="mb-6 inline-block text-sm text-indigo-600 hover:underline">
        &larr; Voltar para a listagem
      </Link>
      <p className="text-gray-500">Formulário de cadastro em construção.</p>
    </div>
  );
}
