import { useState } from 'react';
import { extrairCurriculo } from '../api/curriculos';
import { ApiError } from '../api/client';
import type { CurriculoExtracao } from '../types/curriculo';

const TAMANHO_MAXIMO_BYTES = 5 * 1024 * 1024;

interface PdfUploadProps {
  onExtraido: (dados: CurriculoExtracao) => void;
  onErro: (mensagem: string) => void;
}

export function PdfUpload({ onExtraido, onErro }: PdfUploadProps) {
  const [enviando, setEnviando] = useState(false);
  const [nomeArquivo, setNomeArquivo] = useState<string | null>(null);

  async function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const arquivo = event.target.files?.[0];
    event.target.value = '';
    if (!arquivo) {
      return;
    }

    const temExtensaoPdf = arquivo.name.toLowerCase().endsWith('.pdf');
    if (!temExtensaoPdf || arquivo.size > TAMANHO_MAXIMO_BYTES) {
      onErro('Arquivo inválido. Envie um PDF de até 5 MB.');
      return;
    }

    setNomeArquivo(arquivo.name);
    setEnviando(true);
    try {
      const dados = await extrairCurriculo(arquivo);
      onExtraido(dados);
    } catch (error) {
      onErro(error instanceof ApiError ? error.message : 'Não foi possível processar o currículo enviado.');
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="mb-6 rounded-md border border-dashed border-gray-300 bg-white p-4">
      <label htmlFor="arquivo-curriculo" className="block text-sm font-medium text-gray-700">
        Currículo em PDF (opcional)
      </label>
      <p className="mt-1 text-xs text-gray-500">
        Envie um PDF de até 5 MB para tentar preencher o formulário automaticamente.
      </p>
      <input
        id="arquivo-curriculo"
        type="file"
        accept=".pdf,application/pdf"
        onChange={handleChange}
        disabled={enviando}
        className="mt-2 block w-full text-sm text-gray-700 file:mr-4 file:rounded-md file:border-0 file:bg-indigo-50 file:px-4 file:py-2 file:text-sm file:font-medium file:text-indigo-700 hover:file:bg-indigo-100"
      />
      {enviando && <p className="mt-2 text-sm text-gray-500">Extraindo dados de {nomeArquivo}...</p>}
    </div>
  );
}
