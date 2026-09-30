type AlertTipo = 'sucesso' | 'erro' | 'aviso';

const estilosPorTipo: Record<AlertTipo, string> = {
  sucesso: 'bg-green-50 text-green-800 border-green-200',
  erro: 'bg-red-50 text-red-800 border-red-200',
  aviso: 'bg-yellow-50 text-yellow-800 border-yellow-200',
};

interface AlertProps {
  tipo: AlertTipo;
  children: React.ReactNode;
}

export function Alert({ tipo, children }: AlertProps) {
  return (
    <div role="alert" className={`mb-4 rounded-md border px-4 py-3 text-sm ${estilosPorTipo[tipo]}`}>
      {children}
    </div>
  );
}
