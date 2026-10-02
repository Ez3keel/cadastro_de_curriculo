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
| Documentação da API | Swashbuckle.AspNetCore (Swagger UI) | 10.2.3 |
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

Não é preciso criar nenhum arquivo nem alterar configurações: o `.env` da raiz (já versionado) traz a
senha e as portas. Basta ter o Docker instalado e, na raiz do repositório, rodar:

```bash
docker compose up --build
```

Isso sobe os três serviços (SQL Server, API e frontend). A API aplica as migrations e roda o seed
de dados (3 candidatos fictícios) automaticamente ao iniciar (`Database:ApplyMigrationsOnStartup` e
`Database:SeedData` ligados apenas no container). Na primeira execução o build leva alguns minutos.

Acesse:
- Frontend: http://localhost:8081
- API: http://localhost:5299/api/candidatos (a raiz `http://localhost:5299/` não tem página, devolve 404)
- Swagger UI (documentação interativa da API): http://localhost:5299/swagger
- SQL Server (opcional, para inspecionar o banco): `localhost,1434`, usuário `sa`, senha
  `Curriculos@Docker2026` (credencial local de demonstração, usada só neste ambiente)

Para derrubar o ambiente: `docker compose down` (adicione `-v` para também apagar o volume do banco).

**Personalização (opcional):** para mudar a senha ou as portas, edite o `.env` da raiz (`SA_PASSWORD`,
`API_PORT`, `WEB_PORT`, `SQL_PORT`). O SQL Server do Docker usa por padrão a porta `1434` no host,
para não conflitar com um SQL Server local na `1433`. Se mudar a senha, atualize também a connection
string em `backend/src/Curriculos.Api/appsettings.Development.json` (usada ao rodar sem Docker).

> As credenciais versionadas neste repositório são apenas de demonstração para ambiente local.

## Executando localmente, sem Docker

### Banco de dados

A forma mais simples é subir apenas o SQL Server do Docker Compose (usa a senha e a porta do `.env`):

```bash
docker compose up -d sqlserver
```

A connection string versionada em `backend/src/Curriculos.Api/appsettings.Development.json` já aponta
para ele (`localhost,1434`). Para usar outro SQL Server (instalado na máquina, por exemplo), edite
essa connection string ou defina a variável de ambiente `ConnectionStrings__DefaultConnection`.

### Backend

```bash
cd backend
dotnet ef database update --project src/Curriculos.Api --startup-project src/Curriculos.Api
dotnet run --project src/Curriculos.Api
```

A API sobe por padrão em `http://localhost:5299` (ver `src/Curriculos.Api/Properties/launchSettings.json`).
O Swagger UI fica disponível em `/swagger`, também no Docker (habilitado em qualquer ambiente, sem autenticação — veja as melhorias futuras no DESENVOLVIMENTO.md).

Para popular o banco com 3 candidatos fictícios, defina `Database:SeedData=true` (por exemplo, via
variável de ambiente `Database__SeedData=true`) antes de rodar — a inserção só ocorre se a tabela
estiver vazia.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

A aplicação sobe em `http://localhost:5173` e espera a API em `VITE_API_URL`, já definida em
`frontend/.env.development` (`http://localhost:5299`).

> A origem do frontend precisa estar liberada no CORS da API — configurável em
> `Cors:AllowedOrigin` (`appsettings.Development.json`).

## Configuração

| Configuração | Onde | Padrão |
|---|---|---|
| Connection string | `ConnectionStrings:DefaultConnection` em `appsettings.Development.json` (local) ou `ConnectionStrings__DefaultConnection` no `docker-compose.yml` | `localhost,1434` (SQL Server do Compose) |
| Aplicar migrations ao iniciar | `Database:ApplyMigrationsOnStartup` | `false` (`true` no Docker) |
| Seed de dados | `Database:SeedData` | `false` (`true` no Docker) |
| Origem permitida no CORS | `Cors:AllowedOrigin` | `http://localhost:5173` (local) |
| URL da API no frontend | `VITE_API_URL` em `frontend/.env.development` | `http://localhost:5299` |
| Senha do SQL Server e portas do Docker | `.env` (raiz) | ver arquivo |

Os arquivos de configuração de desenvolvimento (`.env`, `appsettings.Development.json` e
`frontend/.env.development`) estão versionados de propósito, para que o projeto rode sem nenhum
ajuste manual. Eles contêm apenas credenciais de demonstração para ambiente local; o
`appsettings.json` base não tem credenciais.

## Criando a estrutura do banco manualmente

Além das migrations do EF Core, o script `database/schema.sql` (gerado com
`dotnet ef migrations script --idempotent`) pode ser executado diretamente em um SQL Server para
criar a estrutura sem depender do .NET:

```bash
sqlcmd -S localhost,1434 -U sa -P "Curriculos@Docker2026" -C -Q "CREATE DATABASE Curriculos"
sqlcmd -S localhost,1434 -U sa -P "Curriculos@Docker2026" -C -d Curriculos -i database/schema.sql
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
  (ex.: nome ao lado de uma foto, nome em caixa alta misturado a outros elementos no cabeçalho).
  Layouts de duas colunas (ex.: barra lateral de contato ao lado do conteúdo principal) são
  tratados detectando um espaço horizontal grande entre palavras vizinhas na mesma altura, mas
  colunas muito próximas ou mais de duas colunas ainda podem confundir a heurística.
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
