# Desenvolvimento

## Organização e execução do trabalho

Durante o desenvolvimento do projeto, a implementação foi organizada em 10 etapas sequenciais,
seguindo as orientações definidas no arquivo `CLAUDE.md` eu optei em criar o arquivo `CLAUDE.md` porque o desenvolvimento flui melhor com o contexto do projeto. Cada etapa foi concluída individualmente,
com execução de build e testes unitários antes da realização de cada commit, permitindo acompanhar a evolução
do projeto de forma controlada e reduzir a possibilidade de introdução de erros entre as etapas.

## Principais decisões técnicas

Além da decisão sobre a extração de texto dos PDFs, outras decisões técnicas me guiaram durante a implementação:

- **Arquitetura em camadas em um único projeto** (Apresentação, Aplicação, Domínio, Dados), em vez
  de vários projetos separados, por se tratar de um domínio pequeno em que a separação clara de
  responsabilidades já é suficiente, sem o custo adicional de gerenciar múltiplos projetos.
- **Controllers em vez de Minimal APIs**, por ser um padrão mais familiar que uso no meu dia a dia e vejo isso 
  em times .NET e por organizar melhor os endpoints por recurso (`CandidatosController`, `CurriculosController`).
- **Ausência do padrão Repository**, já que o `DbContext` do Entity Framework Core já cumpre esse
  papel (Repository + Unit of Work); uma camada adicional apenas repassaria chamadas sem agregar
  valor real.
- **Interfaces apenas onde havia ganho concreto**: `ICandidatoService` (para testar os controllers
  isoladamente) e `ICurriculoTextExtractor` (para permitir trocar ou complementar a extração de PDF
  com OCR no futuro, sem alterar o restante do código). Classes como `CurriculoParser` e os
  validadores não receberam interface por não terem essa necessidade e são testadas diretamente.
- **Middleware global de tratamento de exceções**, centralizando a conversão de erros de negócio em
  respostas HTTP padronizadas (`ProblemDetails`), evitando `try/catch` repetido nos controllers.
- **Validação duplicada entre frontend (Zod) e backend (FluentValidation)**: o frontend oferece
  feedback imediato ao usuário, mas o backend permanece como fonte da verdade, já que a API pode ser
  usada por outros clientes além do frontend.
- **Não armazenar o arquivo PDF enviado**, apenas os dados extraídos e confirmados no formulário, o
  que reduz o escopo do projeto e o tratamento de dados pessoais.
- **Extração de PDF em endpoint separado**, que apenas retorna os dados encontrados sem gravar no
  banco, permitindo que o usuário revise e complete as informações antes de salvar, e garantindo que
  o cadastro manual nunca dependa do sucesso da extração.

Um dos principais desafios técnicos encontrados ocorreu na extração de informações de arquivos PDF.
Inicialmente, foi utilizado o recurso `page.Text`, disponibilizado pela biblioteca PdfPig. Entretanto,
durante os testes com um PDF de exemplo, foi identificado que o conteúdo extraído não preservava
corretamente as quebras de linha. Isso interferia diretamente na lógica utilizada para
identificar informações como nome, e-mail e telefone, fazendo com que diferentes conteúdos fossem
concatenados e prejudicando a interpretação dos dados. A correção adotada está descrita na seção
"O que precisou corrigir, adaptar ou descartar".

## Ferramentas de IA utilizadas

Utilizei o Claude Code com o modelo Sonnet 5, foi utilizada como ferramenta de apoio durante todo o processo de desenvolvimento, justamente por facilitar a implementação pois o projeto é desenvolvido de uma forma muito rápida, penso nisso porque antes eu poderia levar semanas dependendo do projeto e hoje consigo criar um roteiro que vai ser desenvolvido e consigo focar mais na parte de testes e implementações para validar o que está sendo feito e conseguir entregar a aplicação completa sem problemas.

## Em quais etapas a IA ajudou

A IA auxiliou principalmente na geração e implementação do código a partir das especificações
definidas, na identificação e correção de problemas e na criação de recursos para testes.

A IA auxiliou na investigação do problema de extração de PDF, permitindo identificar que a
abordagem utilizada inicialmente não era adequada para a necessidade do projeto. Esse processo
demonstrou que a utilização da IA não se limitou à geração automática de código: foi necessário
realizar testes, analisar os resultados, identificar comportamentos inesperados e validar as
soluções propostas. Algumas implementações inicialmente geradas precisaram ser descartadas ou
modificadas após os testes, especialmente na parte de extração e interpretação dos PDFs.

Além da implementação e depuração, a IA também auxiliou na geração de um arquivo PDF de exemplo
utilizado nos testes manuais e na configuração do ambiente Docker Compose, permitindo validar o
funcionamento do projeto de ponta a ponta, desde a construção dos serviços até a execução dos fluxos
através do navegador e do nginx.

Dessa forma, a IA atuou como uma ferramenta de desenvolvimento e suporte técnico, acelerando a
implementação, auxiliando na investigação de erros e contribuindo para a criação e execução dos
testes. Entretanto, a validação minha permaneceu necessária para identificar comportamentos
incorretos, avaliar os resultados e decidir quais soluções deveriam ser mantidas, modificadas ou
descartadas.

## O que precisou corrigir, adaptar ou descartar

A implementação inicial da extração de texto de PDF, baseada em `page.Text`, foi descartada por não
preservar as quebras de linha do documento. Como solução, a implementação foi modificada para
utilizar as palavras obtidas por meio de `page.GetWords()`, reconstruindo as linhas do documento com
base na posição vertical dos elementos.

Durante essa correção, também foi necessário superar outro problema: inicialmente, tentou-se agrupar
as palavras comparando suas posições exatas, porém pequenas diferenças na posição dos glifos faziam
com que palavras pertencentes à mesma linha fossem tratadas como linhas diferentes. A solução final
passou a utilizar uma tolerância de posição para realizar o agrupamento de forma mais confiável.

Posteriormente, a extração foi testada com 5 currículos fictícios adicionais, de layouts variados.
Quatro foram identificados corretamente, mas um — de layout em duas colunas, com uma barra lateral de
contato ao lado do conteúdo principal — teve o nome extraído incorretamente ("CONTATO Mariana Costa"
em vez de "Mariana Costa"), porque o cabeçalho da barra lateral estava na mesma altura vertical do
nome, na outra coluna, e o agrupamento de linha (por posição vertical) misturava as duas colunas. A
correção passou a detectar um espaço horizontal bem maior que o espaço normal entre palavras como
indício de quebra de coluna, separando esse trecho em duas linhas distintas. Um teste automatizado
usando esse PDF como exemplo foi adicionado para não regredir esse cenário.

## Como a solução foi verificada

A qualidade da implementação foi verificada por diferentes mecanismos. No backend foram
desenvolvidos 28 testes utilizando xUnit, enquanto o frontend contou com 10 testes utilizando
Vitest. Também foram realizados testes manuais utilizando `POSTMAN` e o navegador, contemplando tanto
fluxos de sucesso quanto situações de erro, como arquivos inválidos, PDFs sem texto, tentativa de
cadastro de e-mail duplicado e recursos inexistentes (404). Por fim, foi realizado um teste completo
do ambiente Docker Compose, envolvendo build, inicialização dos serviços e execução do sistema pelo
navegador.

A extração de PDF foi ainda validada manualmente com 6 currículos fictícios de layouts diferentes
(um único bloco de texto, campos em tabela, duas colunas, e diferentes formatações de nome), o que
permitiu encontrar e corrigir o problema de layout em colunas descrito na seção anterior.

## Tempo aproximado dedicado

O desafio foi desenvolvido ao longo de aproximadamente 4 horas de trabalho.

## Dificuldades, limitações e melhorias futuras

Apesar dos resultados obtidos, algumas limitações permaneceram. A extração de texto de PDFs utiliza
uma abordagem heurística e, portanto, pode apresentar limitações diante de documentos com estruturas
muito diferentes. Como evolução futura, uma das possibilidades seria incorporar OCR para permitir o
processamento de PDFs digitalizados ou que não possuam uma camada de texto adequada, além da
implementação de outros itens previstos no planejamento do projeto (paginação e busca na listagem,
edição e exclusão de candidatos, autenticação e autorização, e CI com GitHub Actions rodando os
testes).
