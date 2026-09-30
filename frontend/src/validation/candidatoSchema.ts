import { z } from 'zod';

function campoOpcional(maxLength: number, mensagemTamanho: string) {
  return z
    .string()
    .trim()
    .max(maxLength, mensagemTamanho)
    .optional()
    .or(z.literal(''));
}

export const candidatoSchema = z.object({
  nomeCompleto: z
    .string()
    .trim()
    .min(1, 'O nome completo é obrigatório.')
    .max(150, 'O nome completo deve ter no máximo 150 caracteres.'),
  email: z
    .string()
    .trim()
    .min(1, 'O e-mail é obrigatório.')
    .email('Informe um e-mail válido.')
    .max(254, 'O e-mail deve ter no máximo 254 caracteres.'),
  telefone: campoOpcional(20, 'O telefone deve ter no máximo 20 caracteres.'),
  areaInteresse: campoOpcional(100, 'A área de interesse deve ter no máximo 100 caracteres.'),
  resumoProfissional: campoOpcional(2000, 'O resumo profissional deve ter no máximo 2000 caracteres.'),
});

export type CandidatoFormValues = z.infer<typeof candidatoSchema>;
