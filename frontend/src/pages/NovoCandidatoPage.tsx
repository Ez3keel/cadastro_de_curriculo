import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ApiError } from '../api/client';
import { criarCandidato } from '../api/candidatos';
import { Alert } from '../components/Alert';
import { CandidatoForm } from '../components/CandidatoForm';
import { PdfUpload } from '../components/PdfUpload';
import type { CandidatoFormValues } from '../validation/candidatoSchema';
import type { CurriculoExtracao } from '../types/curriculo';

type ErrosDeCampo = Partial<Record<keyof CandidatoFormValues, string>>;

function mapearErrosDeCampo(erros: Record<string, string[]>): ErrosDeCampo {
  const resultado: ErrosDeCampo = {};
  for (const [campo, mensagens] of Object.entries(erros)) {
    const chave = (campo.charAt(0).toLowerCase() + campo.slice(1)) as keyof CandidatoFormValues;
    resultado[chave] = mensagens[0];
  }
  return resultado;
}

export function NovoCandidatoPage() {
  const navigate = useNavigate();

  const [valoresIniciais, setValoresIniciais] = useState<Partial<CandidatoFormValues>>();
  const [camposNaoEncontrados, setCamposNaoEncontrados] = useState<string[]>([]);
  const [avisoExtracao, setAvisoExtracao] = useState<string | null>(null);
  const [erroExtracao, setErroExtracao] = useState<string | null>(null);
  const [erroSubmit, setErroSubmit] = useState<string | null>(null);
  const [errosDeCampo, setErrosDeCampo] = useState<ErrosDeCampo>();
  const [enviando, setEnviando] = useState(false);

  function handleExtraido(dados: CurriculoExtracao) {
    setErroExtracao(null);
    setValoresIniciais({
      nomeCompleto: dados.nomeCompleto ?? '',
      email: dados.email ?? '',
      telefone: dados.telefone ?? '',
    });
    setCamposNaoEncontrados(dados.camposNaoEncontrados);
    setAvisoExtracao(
      dados.camposNaoEncontrados.length > 0
        ? 'Alguns dados não foram encontrados no currículo. Confira e complete o formulário.'
        : null,
    );
  }

  function handleErroExtracao(mensagem: string) {
    setErroExtracao(mensagem);
    setAvisoExtracao(null);
  }

  async function handleSubmit(dados: CandidatoFormValues) {
    setEnviando(true);
    setErroSubmit(null);
    setErrosDeCampo(undefined);

    try {
      const candidato = await criarCandidato({
        nomeCompleto: dados.nomeCompleto,
        email: dados.email,
        telefone: dados.telefone || undefined,
        areaInteresse: dados.areaInteresse || undefined,
        resumoProfissional: dados.resumoProfissional || undefined,
      });

      navigate(`/candidatos/${candidato.id}`, {
        state: { mensagem: 'Candidato cadastrado com sucesso.' },
      });
    } catch (error) {
      if (error instanceof ApiError && error.errors) {
        setErrosDeCampo(mapearErrosDeCampo(error.errors));
      } else if (error instanceof ApiError) {
        setErroSubmit(error.message);
      } else {
        setErroSubmit('Ocorreu um erro inesperado.');
      }
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="mx-auto max-w-2xl p-6">
      <Link to="/" className="mb-6 inline-block text-sm text-indigo-600 hover:underline">
        &larr; Voltar para a listagem
      </Link>

      <h1 className="mb-6 text-2xl font-semibold text-gray-900">Novo candidato</h1>

      {erroExtracao && <Alert tipo="erro">{erroExtracao}</Alert>}
      {avisoExtracao && <Alert tipo="aviso">{avisoExtracao}</Alert>}

      <PdfUpload onExtraido={handleExtraido} onErro={handleErroExtracao} />

      <CandidatoForm
        valoresIniciais={valoresIniciais}
        camposNaoEncontrados={camposNaoEncontrados}
        erroGeral={erroSubmit}
        errosDeCampo={errosDeCampo}
        enviando={enviando}
        onSubmit={handleSubmit}
      />
    </div>
  );
}
