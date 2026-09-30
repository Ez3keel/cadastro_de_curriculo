import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { CandidatoForm } from './CandidatoForm';

describe('CandidatoForm', () => {
  it('exibe erros de validação ao submeter o formulário vazio', async () => {
    const onSubmit = vi.fn();
    const user = userEvent.setup();

    render(<CandidatoForm onSubmit={onSubmit} />);

    await user.click(screen.getByRole('button', { name: /salvar candidato/i }));

    expect(await screen.findByText('O nome completo é obrigatório.')).toBeInTheDocument();
    expect(await screen.findByText('O e-mail é obrigatório.')).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('preenche os campos a partir dos dados extraídos do PDF', async () => {
    const onSubmit = vi.fn();

    render(
      <CandidatoForm
        onSubmit={onSubmit}
        valoresIniciais={{ nomeCompleto: 'Ana Beatriz Costa', email: 'ana.costa@exemplo.com' }}
        camposNaoEncontrados={['telefone']}
      />,
    );

    await waitFor(() => {
      expect(screen.getByLabelText(/nome completo/i)).toHaveValue('Ana Beatriz Costa');
    });
    expect(screen.getByLabelText(/e-mail/i)).toHaveValue('ana.costa@exemplo.com');
    expect(screen.getByText('Não encontrado no PDF. Confira e preencha.')).toBeInTheDocument();
  });

  it('chama onSubmit com os dados preenchidos quando o formulário é válido', async () => {
    const onSubmit = vi.fn();
    const user = userEvent.setup();

    render(<CandidatoForm onSubmit={onSubmit} />);

    await user.type(screen.getByLabelText(/nome completo/i), 'Maria Souza');
    await user.type(screen.getByLabelText(/e-mail/i), 'maria@exemplo.com');
    await user.click(screen.getByRole('button', { name: /salvar candidato/i }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(1));
    expect(onSubmit.mock.calls[0][0]).toMatchObject({
      nomeCompleto: 'Maria Souza',
      email: 'maria@exemplo.com',
    });
  });

  it('exibe a mensagem de erro geral quando informada', () => {
    render(<CandidatoForm onSubmit={vi.fn()} erroGeral="Já existe um candidato com este e-mail." />);

    expect(screen.getByRole('alert')).toHaveTextContent('Já existe um candidato com este e-mail.');
  });
});
