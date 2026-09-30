import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { candidatoSchema, type CandidatoFormValues } from '../validation/candidatoSchema';
import { Alert } from './Alert';

interface CandidatoFormProps {
  valoresIniciais?: Partial<CandidatoFormValues>;
  camposNaoEncontrados?: string[];
  erroGeral?: string | null;
  errosDeCampo?: Partial<Record<keyof CandidatoFormValues, string>>;
  enviando?: boolean;
  onSubmit: (dados: CandidatoFormValues) => void;
}

const valoresPadrao: CandidatoFormValues = {
  nomeCompleto: '',
  email: '',
  telefone: '',
  areaInteresse: '',
  resumoProfissional: '',
};

function classeDoCampo(temErro: boolean) {
  return `mt-1 block w-full rounded-md border px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-1 ${
    temErro
      ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
      : 'border-gray-300 focus:border-indigo-500 focus:ring-indigo-500'
  }`;
}

export function CandidatoForm({
  valoresIniciais,
  camposNaoEncontrados = [],
  erroGeral,
  errosDeCampo,
  enviando = false,
  onSubmit,
}: CandidatoFormProps) {
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<CandidatoFormValues>({
    resolver: zodResolver(candidatoSchema),
    defaultValues: valoresPadrao,
  });

  useEffect(() => {
    if (valoresIniciais) {
      reset((valoresAtuais) => ({ ...valoresAtuais, ...valoresIniciais }));
    }
  }, [valoresIniciais, reset]);

  useEffect(() => {
    if (!errosDeCampo) {
      return;
    }

    for (const [campo, mensagem] of Object.entries(errosDeCampo)) {
      setError(campo as keyof CandidatoFormValues, { type: 'server', message: mensagem });
    }
  }, [errosDeCampo, setError]);

  function naoEncontradoNoPdf(campo: string) {
    return camposNaoEncontrados.includes(campo);
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4 rounded-md border border-gray-200 bg-white p-6">
      {erroGeral && <Alert tipo="erro">{erroGeral}</Alert>}

      <div>
        <label htmlFor="nomeCompleto" className="block text-sm font-medium text-gray-700">
          Nome completo
        </label>
        <input
          id="nomeCompleto"
          {...register('nomeCompleto')}
          className={classeDoCampo(!!errors.nomeCompleto)}
        />
        {errors.nomeCompleto && <p className="mt-1 text-sm text-red-600">{errors.nomeCompleto.message}</p>}
        {!errors.nomeCompleto && naoEncontradoNoPdf('nomeCompleto') && (
          <p className="mt-1 text-sm text-yellow-700">Não encontrado no PDF. Confira e preencha.</p>
        )}
      </div>

      <div>
        <label htmlFor="email" className="block text-sm font-medium text-gray-700">
          E-mail
        </label>
        <input id="email" type="email" {...register('email')} className={classeDoCampo(!!errors.email)} />
        {errors.email && <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>}
        {!errors.email && naoEncontradoNoPdf('email') && (
          <p className="mt-1 text-sm text-yellow-700">Não encontrado no PDF. Confira e preencha.</p>
        )}
      </div>

      <div>
        <label htmlFor="telefone" className="block text-sm font-medium text-gray-700">
          Telefone
        </label>
        <input id="telefone" {...register('telefone')} className={classeDoCampo(!!errors.telefone)} />
        {errors.telefone && <p className="mt-1 text-sm text-red-600">{errors.telefone.message}</p>}
        {!errors.telefone && naoEncontradoNoPdf('telefone') && (
          <p className="mt-1 text-sm text-yellow-700">Não encontrado no PDF. Confira e preencha.</p>
        )}
      </div>

      <div>
        <label htmlFor="areaInteresse" className="block text-sm font-medium text-gray-700">
          Área de interesse
        </label>
        <input id="areaInteresse" {...register('areaInteresse')} className={classeDoCampo(!!errors.areaInteresse)} />
        {errors.areaInteresse && <p className="mt-1 text-sm text-red-600">{errors.areaInteresse.message}</p>}
      </div>

      <div>
        <label htmlFor="resumoProfissional" className="block text-sm font-medium text-gray-700">
          Resumo profissional
        </label>
        <textarea
          id="resumoProfissional"
          rows={5}
          {...register('resumoProfissional')}
          className={classeDoCampo(!!errors.resumoProfissional)}
        />
        {errors.resumoProfissional && (
          <p className="mt-1 text-sm text-red-600">{errors.resumoProfissional.message}</p>
        )}
      </div>

      <button
        type="submit"
        disabled={enviando}
        className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {enviando ? 'Salvando...' : 'Salvar candidato'}
      </button>
    </form>
  );
}
