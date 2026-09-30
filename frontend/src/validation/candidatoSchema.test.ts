import { describe, expect, it } from 'vitest';
import { candidatoSchema } from './candidatoSchema';

const dadosValidos = {
  nomeCompleto: 'Maria Souza',
  email: 'maria@exemplo.com',
  telefone: '(41) 99999-9999',
  areaInteresse: 'Desenvolvimento',
  resumoProfissional: 'Resumo de teste.',
};

describe('candidatoSchema', () => {
  it('aceita dados válidos', () => {
    const resultado = candidatoSchema.safeParse(dadosValidos);

    expect(resultado.success).toBe(true);
  });

  it('rejeita quando o nome completo está vazio', () => {
    const resultado = candidatoSchema.safeParse({ ...dadosValidos, nomeCompleto: '' });

    expect(resultado.success).toBe(false);
  });

  it('rejeita quando o e-mail está vazio', () => {
    const resultado = candidatoSchema.safeParse({ ...dadosValidos, email: '' });

    expect(resultado.success).toBe(false);
  });

  it('rejeita e-mail com formato inválido', () => {
    const resultado = candidatoSchema.safeParse({ ...dadosValidos, email: 'nao-e-um-email' });

    expect(resultado.success).toBe(false);
  });

  it('aceita quando os campos opcionais não são informados', () => {
    const resultado = candidatoSchema.safeParse({
      nomeCompleto: 'Maria Souza',
      email: 'maria@exemplo.com',
    });

    expect(resultado.success).toBe(true);
  });

  it('rejeita nome completo acima de 150 caracteres', () => {
    const resultado = candidatoSchema.safeParse({ ...dadosValidos, nomeCompleto: 'a'.repeat(151) });

    expect(resultado.success).toBe(false);
  });
});
