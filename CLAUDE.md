# Cadastro de Currículos — Desafio técnico CIEE/PR

> Documento de especificação e instruções para o desenvolvimento assistido por IA (Claude Code).
> Descrição do escopo, a arquitetura, as decisões técnicas e a forma de trabalho do projeto.

---

## 1. Visão geral

Aplicação para a equipe de recrutamento cadastrar e consultar candidatos.

Duas formas de cadastro compartilham **o mesmo formulário e as mesmas regras de validação**:

1. **Manual:** a pessoa preenche o formulário e salva. O envio de PDF é opcional.
2. **Com PDF:** a pessoa envia um currículo. O backend extrai o texto e tenta identificar nome, e-mail e telefone. Os dados encontrados preenchem o formulário, que pode ser corrigido e complementado antes de salvar.

Após salvo, o candidato aparece em uma listagem com acesso a uma tela de detalhes.

**Premissas**

- A ausência de PDF ou uma falha na leitura **nunca** impede o cadastro manual.
- Prioridade: solução **simples, funcional e fácil de explicar e evoluir**. Nada de camadas, padrões ou abstrações sem necessidade concreta.

---

## 2. Stack tecnológica

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core Web API — **.NET 10 (LTS)**, C#, **Controllers** |
| Persistência | Entity Framework Core + **SQL Server** (migrations) |
| Validação (back) | FluentValidation |
| Leitura de PDF | UglyToad.PdfPig |
| Logging | Serilog (console; arquivo em Development) |
| Documentação da API | OpenAPI/Swagger (apenas em Development) |
| Frontend | React + TypeScript + Vite |
| Rotas (front) | React Router |
| Formulários (front) | React Hook Form + Zod |
| Estilo | Tailwind CSS |
| HTTP (front) | `fetch` nativo encapsulado em `src/api/` |
| Testes back | xUnit |
| Testes front | Vitest + Testing Library |
| Ambiente | Docker Compose (opcional) **e** execução local, ambos documentados |

As versões exatas de SDKs e pacotes devem ser registradas no README.

---

## 3. Arquitetura

### 3.1 Visão geral

O sistema segue uma **arquitetura em camadas** com separação entre frontend (SPA) e backend (API REST).

O backend é uma **Web API com Controllers**, isto é, o padrão MVC sem views no servidor. O "V" (View) é o frontend React, que consome a API em JSON.

```
┌──────────────────────────┐        HTTP/JSON        ┌──────────────────────────────────────────┐
│   Frontend (React SPA)   │ ──────────────────────► │            Backend (ASP.NET Core)        │
│                          │                         │                                          │
│  pages → components      │                         │  Middleware (erros, logging)             │
│        ↓                 │                         │        ↓                                 │
│  api/ (fetch)            │                         │  Camada de Apresentação  (Controllers)   │
│  validation/ (Zod)       │                         │        ↓                                 │
└──────────────────────────┘                         │  Camada de Aplicação     (Services)      │
                                                     │        ↓                                 │
                                                     │  Camada de Dados         (DbContext/EF)  │
                                                     └───────────────────┬──────────────────────┘
                                                                         ↓
                                                                   SQL Server
```

### 3.2 Camadas do backend

| Camada | Pastas | Responsabilidade | Não deve |
|---|---|---|---|
| **Apresentação** | `Controllers/`, `Dtos/`, `Middleware/` | Receber requisições HTTP, validar entrada, mapear DTO ↔ serviço, devolver o status HTTP correto | Conter regra de negócio ou acessar o `DbContext` diretamente |
| **Aplicação** | `Services/`, `Validators/` | Regras de negócio (e-mail único, orquestração da extração), validação | Conhecer detalhes de HTTP (`IActionResult`, `HttpContext`) |
| **Domínio** | `Models/` | Entidades (`Candidato`) | Depender de qualquer outra camada |
| **Dados** | `Data/` | `AppDbContext`, configurações de mapeamento (Fluent API), migrations, seed | Conter regra de negócio |
| **Infraestrutura** | `Services/Pdf/` | Leitura de PDF (biblioteca externa) atrás de uma interface | Vazar tipos da biblioteca (PdfPig) para outras camadas |

**Regra de dependência:** cada camada só conhece a camada abaixo dela. Controllers chamam Services; Services usam o `DbContext` e os extratores; ninguém chama "para cima".

**Sem Repository Pattern:** o `DbContext` do EF Core já implementa Repository e Unit of Work. Uma camada extra só repassaria chamadas.

### 3.3 Interfaces

Interfaces são usadas **apenas onde há ganho concreto**:

| Tipo | Interface? | Motivo |
|---|---|---|
| `CandidatoService` | ✅ `ICandidatoService` | Permite testar controllers isoladamente |
| `PdfPigTextExtractor` | ✅ `ICurriculoTextExtractor` | Permite trocar ou complementar com OCR sem alterar o restante |
| `CurriculoParser` | ❌ | Classe pura, sem I/O; testada diretamente |
| Validators | ❌ | Registrados pelo próprio FluentValidation |

### 3.4 Estrutura de pastas

```
/
├── backend/
│   ├── Curriculos.sln
│   ├── src/Curriculos.Api/
│   │   ├── Controllers/        CandidatosController, CurriculosController
│   │   ├── Data/               AppDbContext, Configurations/, Migrations/, Seed/
│   │   ├── Dtos/               Requests e responses
│   │   ├── Exceptions/         Exceções de negócio (ex.: EmailDuplicadoException)
│   │   ├── Middleware/         ExceptionHandlingMiddleware
│   │   ├── Models/             Candidato
│   │   ├── Services/           ICandidatoService, CandidatoService
│   │   │   └── Pdf/            ICurriculoTextExtractor, PdfPigTextExtractor,
│   │   │                       CurriculoParser, PdfFileValidator
│   │   ├── Validators/         CandidatoRequestValidator
│   │   └── Program.cs
│   └── tests/Curriculos.Api.Tests/
│       ├── Parsers/
│       ├── Validators/
│       └── Services/
├── frontend/
│   └── src/
│       ├── api/                client.ts (wrapper do fetch), candidatos.ts, curriculos.ts
│       ├── components/         CandidatoForm, PdfUpload, Alert, ...
│       ├── pages/              ListagemPage, NovoCandidatoPage, DetalhesPage
│       ├── validation/         candidatoSchema.ts (Zod)
│       ├── types/
│       └── main.tsx, App.tsx
├── database/
│   └── schema.sql              Gerado a partir das migrations
├── docs/
│   └── curriculo-exemplo.pdf
├── docker-compose.yml
├── .env.example
├── .gitignore
├── CLAUDE.md
├── DESENVOLVIMENTO.md
└── README.md
```

---

## 4. Modelo de dados

### Tabela `Candidatos`

| Campo | Tipo | Regra |
|---|---|---|
| `Id` | `Guid`, PK, gerado pela aplicação | |
| `NomeCompleto` | `nvarchar(150)` | Obrigatório |
| `Email` | `nvarchar(254)` | Obrigatório, formato válido, **único** (índice unique) |
| `Telefone` | `nvarchar(20)` | Opcional |
| `AreaInteresse` | `nvarchar(100)` | Opcional |
| `ResumoProfissional` | `nvarchar(2000)` | Opcional |
| `CriadoEm` | `datetime2` | Preenchido pelo backend (UTC) |

- Mapeamento via Fluent API em `Data/Configurations/`, e não por atributos na entidade.
- O arquivo PDF **não** é armazenado; apenas os dados do formulário.
- O e-mail é salvo normalizado (trim + minúsculas) para a regra de unicidade funcionar.

---

## 5. API

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `POST` | `/api/candidatos` | Cria candidato (JSON) | `201` + `Location`, `400`, `409` |
| `GET` | `/api/candidatos` | Lista candidatos (id, nome, e-mail, área, data), do mais recente para o mais antigo | `200` |
| `GET` | `/api/candidatos/{id}` | Detalhes do candidato | `200`, `404` |
| `POST` | `/api/curriculos/extrair` | Recebe PDF (multipart, campo `arquivo`) e retorna os dados extraídos **sem salvar** | `200`, `400`, `422` |

**Resposta da extração**

```json
{
  "nomeCompleto": "Maria Souza",
  "email": null,
  "telefone": "(41) 99999-9999",
  "camposNaoEncontrados": ["email"]
}
```

**Convenções**

- Erros no formato **ProblemDetails** (`application/problem+json`), com mensagens em português.
- Erros de validação no formato `ValidationProblemDetails`, com a lista de erros por campo, para o front exibir junto a cada input.
- CORS liberado apenas para a origem do frontend (configurável).

---

## 6. Regras de validação

As mesmas regras existem no front (Zod) e no back (FluentValidation). **O backend é a fonte da verdade.**

| Campo | Regras |
|---|---|
| Nome completo | Obrigatório, máx. 150 caracteres |
| E-mail | Obrigatório, formato válido, máx. 254 caracteres |
| Telefone | Opcional, máx. 20; aceita formatos brasileiros comuns sem validação rígida |
| Área de interesse | Opcional, máx. 100 |
| Resumo profissional | Opcional, máx. 2000 |

Aplicar trim em todos os campos antes de validar.

### Validação do arquivo (`PdfFileValidator`)

- Arquivo presente na rota de extração.
- Extensão `.pdf` **e** assinatura iniciando com `%PDF-` (não confiar só no content-type).
- Tamanho máximo de **5 MB**, com o limite também configurado no Kestrel/`FormOptions`.

---

## 7. Extração de dados do PDF

1. `PdfPigTextExtractor` (implementa `ICurriculoTextExtractor`) extrai o texto de todas as páginas.
2. Se o texto tiver menos de ~50 caracteres, o PDF é tratado como sem texto (provavelmente escaneado). A API retorna `422` orientando o preenchimento manual.
3. `CurriculoParser` recebe o texto e identifica:
   - **E-mail:** regex padrão; o primeiro encontrado.
   - **Telefone:** regex para formatos BR (`(41) 99999-9999`, `41999999999`, `+55 41 99999-9999`, fixo com 8 dígitos).
   - **Nome:** heurística. Primeira linha não vazia com 2 a 6 palavras, apenas letras (com acentos) e espaços, ignorando títulos como "Currículo", "Curriculum Vitae" e "Resumo".
4. Campos não identificados vão para `camposNaoEncontrados`.

As limitações da extração devem ser documentadas no README e no DESENVOLVIMENTO.md.

---

## 8. Tratamento de erros e logging

### Middleware global (`ExceptionHandlingMiddleware`)

Centraliza a conversão de exceções em respostas HTTP. Controllers não usam `try/catch` para erros de negócio.

| Exceção | Status | Mensagem |
|---|---|---|
| `EmailDuplicadoException` | `409` | "Já existe um candidato com este e-mail." |
| `DbUpdateException` por violação do índice unique (concorrência) | `409` | Mesma mensagem acima |
| `ArquivoInvalidoException` | `400` | "Arquivo inválido. Envie um PDF de até 5 MB." |
| `FalhaLeituraPdfException` | `422` | "Não foi possível ler o texto do PDF. Preencha os dados manualmente." |
| Qualquer outra | `500` | "Ocorreu um erro inesperado." (sem stack trace na resposta) |

### Logging (Serilog)

- Saída no console em todos os ambientes; arquivo (`logs/`, rolagem diária) em Development.
- Configuração via `appsettings.json` (seção `Serilog`).
- Log de requisições HTTP com `UseSerilogRequestLogging`.
- Registrar falhas de extração e erros não tratados.
- **Não registrar dados pessoais** (nome, e-mail, telefone, conteúdo do currículo), em respeito à LGPD. Logar apenas IDs, nomes de arquivo e tamanhos.
- A pasta `logs/` fica no `.gitignore`.

---

## 9. Frontend

### Telas

| Rota | Tela | Comportamento |
|---|---|---|
| `/` | Listagem | Tabela com nome, e-mail, área e data; botão "Novo candidato"; clique na linha abre os detalhes; estados de carregamento, vazio e erro |
| `/candidatos/novo` | Novo candidato | Upload opcional de PDF acima do formulário. Ao enviar, chama a extração, preenche os campos encontrados e destaca os não identificados. O `CandidatoForm` é o mesmo nos dois fluxos |
| `/candidatos/:id` | Detalhes | Todos os campos, botão de voltar e tratamento de 404 |

### Mensagens ao usuário

- "Candidato cadastrado com sucesso."
- "Arquivo inválido. Envie um PDF de até 5 MB."
- "Não foi possível ler o texto do PDF. Preencha os dados manualmente."
- "Alguns dados não foram encontrados no currículo. Confira e complete o formulário."
- "Já existe um candidato com este e-mail."
- Erros de validação exibidos junto a cada campo, tanto os do Zod quanto os retornados pela API.

### Convenções

- `candidatoSchema.ts` (Zod) espelha as regras da seção 6 e é usado pelo React Hook Form via `zodResolver`.
- Tamanho e tipo do arquivo também são checados no cliente antes do upload; a validação definitiva é no backend.
- `src/api/client.ts` encapsula o `fetch`: URL base via `VITE_API_URL`, parse de ProblemDetails e erros tipados.
- Tailwind com visual simples e limpo; layout responsivo básico.

---

## 10. Configuração e execução

O projeto deve funcionar **com e sem Docker**. O Docker é só uma forma de empacotar; nenhum código pode depender dele.

| Configuração | Onde |
|---|---|
| Connection string | `ConnectionStrings:DefaultConnection` (sobrescrevível por `ConnectionStrings__DefaultConnection`) |
| Aplicar migrations ao iniciar | `Database:ApplyMigrationsOnStartup` (padrão `false`; `true` no Docker) |
| Seed de dados | `Database:SeedData` (padrão `false`) |
| Origem permitida no CORS | `Cors:AllowedOrigin` |
| URL da API no front | `VITE_API_URL` |

- `appsettings.json` **sem credenciais reais**. Fornecer `appsettings.Development.example.json` e `.env.example`.
- Localmente: `dotnet ef database update`.
- Script SQL: `dotnet ef migrations script --idempotent -o database/schema.sql`.

### Seed de dados

- 3 candidatos fictícios, com nomes e e-mails claramente falsos (ex.: `@exemplo.com`).
- Executa apenas com `Database:SeedData=true` **e** se a tabela estiver vazia (idempotente).

### Docker Compose

| Serviço | Detalhes |
|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest`, healthcheck, senha vinda do `.env` |
| `api` | Build do backend; sobe após o SQL Server estar saudável; migrations e seed ligados |
| `web` | Build do frontend servido por nginx, apontando para a API |

---

## 11. Testes

### Backend (xUnit)

| Alvo | Casos |
|---|---|
| `CurriculoParser` | Textos com e sem e-mail; telefones em formatos variados; nome na primeira linha; texto iniciando com "Currículo"; texto vazio |
| `CandidatoRequestValidator` | Obrigatórios, e-mail inválido, limites de tamanho, trim |
| `PdfFileValidator` | Extensão errada, assinatura inválida, acima de 5 MB, arquivo válido |
| `CandidatoService` | E-mail duplicado lança `EmailDuplicadoException` (EF Core InMemory ou SQLite in-memory) |

### Frontend (Vitest)

- `candidatoSchema`: campos obrigatórios e formato de e-mail.
- `CandidatoForm`: exibe erros de validação e preenche os campos a partir dos dados extraídos.

Comandos documentados no README: `dotnet test` e `npm test`.

---

## 12. Decisões técnicas

| Decisão | Motivo |
|---|---|
| Arquitetura em camadas em um único projeto | Domínio pequeno; separação clara sem o custo de vários projetos |
| Controllers em vez de Minimal APIs | Padrão familiar em times .NET e organização clara por recurso |
| Sem Repository Pattern | `DbContext` já cumpre esse papel |
| Interfaces só em `ICandidatoService` e `ICurriculoTextExtractor` | Testabilidade e troca futura por OCR, sem abstrações desnecessárias |
| Middleware global de erros | Respostas padronizadas e controllers enxutos |
| Serilog | Logs estruturados e saída em arquivo para diagnóstico |
| Extração em endpoint separado, sem salvar | O usuário revisa os dados antes de gravar; o fluxo manual não depende do PDF |
| Validação duplicada (Zod + FluentValidation) | Feedback imediato na tela; backend como fonte da verdade |
| PDF não armazenado | Fora do escopo e reduz o tratamento de dados pessoais |
| React Hook Form + Zod | Padrão de mercado; schema testável e reutilizável |
| `fetch` nativo | Sem dependência extra para poucas chamadas |
| Tailwind CSS | Produtividade e experiência prévia |

---

## 13. Forma de trabalho

Implementar em etapas, **uma de cada vez**, parando ao final de cada uma para revisão e commit:

1. Solução, projeto da API, projeto de testes, `.gitignore`, `.editorconfig`
2. Entidade, `AppDbContext`, configuração Fluent API, migration inicial, conexão com o SQL Server
3. Serilog e middleware global de erros
4. `CandidatoService`, endpoints de candidatos, validação e testes
5. Extração de PDF (validator de arquivo, extractor, parser), endpoint e testes
6. Seed de dados
7. Frontend: setup (Vite, Tailwind, Router), cliente HTTP, listagem e detalhes
8. Frontend: formulário, upload de PDF, mensagens e testes
9. Docker Compose e `.env.example`
10. README, `schema.sql` e PDF de exemplo

### Regras para o assistente

- Não avançar para a próxima etapa sem confirmação.
- Mensagens de commit sugeridas em português, no imperativo e curtas (ex.: "Adiciona endpoint de cadastro de candidatos").
- **Não refatorar código já escrito ou ajustado por mim sem perguntar antes.**
- Ao tomar uma decisão técnica relevante, explicar o motivo em uma ou duas frases, para registro no DESENVOLVIMENTO.md.
- O DESENVOLVIMENTO.md é escrito por mim. O assistente pode sugerir estrutura, mas não inventar o relato.
- Nomes de domínio em português (`Candidato`, `Curriculo`); termos técnicos podem ficar em inglês (`Controller`, `Service`, `Dto`).
- Não adicionar bibliotecas fora da stack da seção 2 sem perguntar.

---

## 14. Fora do escopo

- Edição e exclusão de candidatos
- Autenticação e autorização
- Paginação e busca na listagem
- Armazenamento do arquivo PDF

## 15. Melhorias futuras

- OCR para PDFs escaneados (`OcrTextExtractor` com PDFtoImage + Tesseract, como fallback)
- CI com GitHub Actions rodando os testes
- Health check (`/health`) verificando o banco
- Paginação e busca por nome na listagem
- Edição e exclusão de candidatos

## 16. Pendente de confirmação com o CIEE/PR

- Se a extração precisa suportar PDFs escaneados (OCR)
- Preferência entre Docker Compose e execução local
- Tipo de testes esperado
