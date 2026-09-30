# Cadastro de Currículos

Aplicação para a equipe de recrutamento cadastrar e consultar candidatos, com duas formas de
cadastro que compartilham o mesmo formulário e as mesmas regras de validação:

1. **Manual** — a pessoa preenche o formulário e salva.
2. **Com PDF** — a pessoa envia um currículo; o backend extrai o texto e tenta identificar nome,
   e-mail e telefone, que preenchem o formulário e podem ser corrigidos antes de salvar.

A ausência de PDF ou uma falha na leitura nunca impede o cadastro manual.

## Tecnologias e versões

| Camada | Tecnologia | Versão |
|---|---|---|
| Backend | .NET / ASP.NET Core (Controllers) | 10.0.400 (SDK) |
| Persistência | Entity Framework Core (SQL Server) | 10.0.12 |
| Validação | FluentValidation | 12.1.1 |
| Leitura de PDF | UglyToad.PdfPig | 1.7.0-custom-5 |
| Logging | Serilog.AspNetCore | 10.0.0 |
| Testes backend | xUnit | (via template .NET 10) |
| Frontend | React + TypeScript + Vite | React 19.3.0, TS 6.0.3, Vite 8.3.1 |
| Roteamento | React Router | 7.18.4 |
| Formulários | React Hook Form + Zod | 7.89.0 / 4.6.5 |
| Estilo | Tailwind CSS | 4.3.3 |
| Testes frontend | Vitest / @testing-library/react | 5.0.3 / 16.3.3 |
| Banco de dados | SQL Server | 2022 (imagem `mssql/server:2022-latest`) |

## Estrutura do repositório

```
backend/     API ASP.NET Core (Controllers, EF Core, FluentValidation, PdfPig)
frontend/    SPA React + TypeScript + Vite
database/    schema.sql gerado a partir das migrations
docs/        currículo de exemplo em PDF para testar a importação
docker-compose.yml
```

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (testado com 10.0.400)
- [Node.js 22+](https://nodejs.org/) (testado com 24.14.1) e npm
- SQL Server (local, ou via Docker — veja abaixo)
- Docker e Docker Compose (opcional, para subir tudo de uma vez)

## Executando com Docker Compose (mais simples)

1. Copie o arquivo de variáveis de ambiente e ajuste a senha do SQL Server:

   ```bash
   cp .env.example .env
   ```

2. Suba os três serviços (SQL Server, API e frontend):

   ```bash
   docker compose up --build
   ```

   A API aplica as migrations e roda o seed de dados automaticamente ao iniciar
   (`Database:ApplyMigrationsOnStartup` e `Database:SeedData` ligados apenas no container).

3. Acesse:
   - Frontend: http://localhost:8081 (porta configurável via `WEB_PORT` no `.env`)
   - API: http://localhost:5299 (porta configurável via `API_PORT` no `.env`)

4. Para derrubar o ambiente: `docker compose down` (adicione `-v` para também apagar o volume do banco).

## Executando localmente, sem Docker

### Banco de dados

Suba um SQL Server à sua escolha (local, ou um container avulso, por exemplo):

```bash
docker run -d --name curriculos-sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

### Backend

```bash
cd backend
cp src/Curriculos.Api/appsettings.Development.example.json src/Curriculos.Api/appsettings.Development.json
# edite appsettings.Development.json com a connection string do seu SQL Server
dotnet ef database update --project src/Curriculos.Api --startup-project src/Curriculos.Api
dotnet run --project src/Curriculos.Api
```

A API sobe por padrão em `http://localhost:5299` (ver `src/Curriculos.Api/Properties/launchSettings.json`).
O Swagger fica disponível em `/openapi/v1.json` apenas em Development.

Para popular o banco com 3 candidatos fictícios, defina `Database:SeedData=true` (por exemplo, via
variável de ambiente `Database__SeedData=true`) antes de rodar — a inserção só ocorre se a tabela
estiver vazia.

### Frontend

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

A aplicação sobe em `http://localhost:5173` e espera a API em `VITE_API_URL` (padrão
`http://localhost:5299`, configurável em `.env.local`).

> A origem do frontend precisa estar liberada no CORS da API — configurável em
> `Cors:AllowedOrigin` (`appsettings.Development.json`).

## Configuração (sem credenciais reais no repositório)

| Configuração | Onde | Padrão |
|---|---|---|
| Connection string | `ConnectionStrings:DefaultConnection` (ou `ConnectionStrings__DefaultConnection`) | — |
| Aplicar migrations ao iniciar | `Database:ApplyMigrationsOnStartup` | `false` (`true` no Docker) |
| Seed de dados | `Database:SeedData` | `false` |
| Origem permitida no CORS | `Cors:AllowedOrigin` | — |
| URL da API no frontend | `VITE_API_URL` | — |

Arquivos de exemplo sem credenciais reais: `backend/src/Curriculos.Api/appsettings.Development.example.json`,
`frontend/.env.example` e `.env.example` (raiz, usado pelo Docker Compose).

## Criando a estrutura do banco manualmente

Além das migrations do EF Core, o script `database/schema.sql` (gerado com
`dotnet ef migrations script --idempotent`) pode ser executado diretamente em um SQL Server para
criar a estrutura sem depender do .NET:

```bash
sqlcmd -S localhost -U sa -P "SuaSenhaForte123!" -i database/schema.sql
```

## Testes

Backend (xUnit):

```bash
cd backend
dotnet test
```

Frontend (Vitest + Testing Library):

```bash
cd frontend
npm test
```

## API

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/candidatos` | Cria um candidato |
| `GET` | `/api/candidatos` | Lista os candidatos (mais recente primeiro) |
| `GET` | `/api/candidatos/{id}` | Detalhes de um candidato |
| `POST` | `/api/curriculos/extrair` | Recebe um PDF (`multipart/form-data`, campo `arquivo`) e retorna os dados extraídos, sem salvar |

Erros são retornados no formato `application/problem+json` (`ProblemDetails`/`ValidationProblemDetails`),
com mensagens em português.

## Limitações conhecidas da extração de PDF

- Funciona apenas com PDFs que contêm texto selecionável; PDFs escaneados (imagem) retornam
  `422` orientando o preenchimento manual.
- A identificação de nome é heurística (primeira linha com 2 a 6 palavras, só letras e espaços,
  ignorando títulos como "Currículo" ou "Resumo") e pode falhar em layouts fora desse padrão
  (ex.: nome ao lado de uma foto, currículos em colunas, nome em caixa alta misturado a outros
  elementos no cabeçalho).
- E-mail e telefone usam expressões regulares comuns a formatos brasileiros; formatos incomuns
  podem não ser reconhecidos.
- Quando um campo não é encontrado, ele fica em branco no formulário para preenchimento manual —
  nenhum dado é assumido ou inventado.

## Fora do escopo

- Edição e exclusão de candidatos
- Autenticação e autorização
- Paginação e busca na listagem
- Armazenamento do arquivo PDF enviado

## Documentação do desenvolvimento

O relato do processo de desenvolvimento, decisões técnicas e uso de IA está em
[DESENVOLVIMENTO.md](DESENVOLVIMENTO.md).
