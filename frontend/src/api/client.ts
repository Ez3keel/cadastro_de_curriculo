export class ApiError extends Error {
  status: number;
  errors?: Record<string, string[]>;

  constructor(status: number, message: string, errors?: Record<string, string[]>) {
    super(message);
    this.status = status;
    this.errors = errors;
  }
}

const baseUrl = import.meta.env.VITE_API_URL ?? '';

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const isFormData = options.body instanceof FormData;

  const response = await fetch(`${baseUrl}${path}`, {
    ...options,
    headers: {
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...options.headers,
    },
  });

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get('content-type') ?? '';
  const isJson = contentType.includes('json');
  const body = isJson ? await response.json() : undefined;

  if (!response.ok) {
    const mensagem = body?.title ?? 'Ocorreu um erro inesperado.';
    throw new ApiError(response.status, mensagem, body?.errors);
  }

  return body as T;
}

export const apiClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, data?: unknown) =>
    request<T>(path, {
      method: 'POST',
      body: data instanceof FormData ? data : JSON.stringify(data),
    }),
};
