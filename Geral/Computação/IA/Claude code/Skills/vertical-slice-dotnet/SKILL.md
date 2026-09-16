---
name: vertical-slice-dotnet
description: Referência de implementação para features VSA neste projeto. Consultada durante a fase Green do ciclo TDD — a skill tdd-dotnet é a orquestradora e deve ser lida antes desta. Define como escrever ou modificar cada arquivo de um slice: Command/Query, Handler, Validator e Endpoint. Usada tanto na criação de features novas quanto na modificação de features existentes.
---

# Vertical Slice Architecture — .NET + PostgreSQL

## Por que esta skill existe

Vertical Slice Architecture organiza o código por **funcionalidade**, não por camada técnica. Em vez de uma pasta `Controllers/`, uma pasta `Services/` e uma pasta `Repositories/` espalhando os pedaços de uma única funcionalidade por todo o projeto, cada funcionalidade vive inteira em uma única pasta — auto-contida, fácil de entender, fácil de deletar sem medo de quebrar outra coisa.

O ganho prático: quando alguém pede "adicionar X", a resposta é sempre "crie uma pasta nova". Não existe a pergunta "em qual camada isso entra?" que aparece toda hora em arquitetura em camadas.

## Stack assumida por esta skill

- **.NET 10**, ASP.NET Core Minimal APIs, onde os endpoints utiliam o carter
- **PostgreSQL** via **EF Core** com o provider Npgsql
- **MediatR** para o dispatch de Commands e Queries
- **FluentValidation** para validação de entrada
- **NUnit** para testes

Se adapte a estrutura de pastas e o fluxo — mas mantenha o princípio de slice auto-contido.

## A estrutura de pastas — regra fixa

Todo slice mora neste caminho, sem exceção:

```
{Projeto}.Api/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/
```

Onde:
- **{Projeto}** é o nome do projeto da API (ex: `AplicativoPet`)
- **{Modulo}** é o agregado/entidade de domínio no plural ou singular consistente com o resto do projeto (ex: `Pet`, `Usuarios`)
- **{Command|Query}** é literal — Commands mudam estado, Queries apenas leem. Nunca misture os dois na mesma pasta.
- **{NomeDaAcao}** é o nome da operação específica, no Imperativo para Command (`CriarPet`, `AtualizarEndereco`) ou descritivo para Query (`BuscarUsuarioPorId`, `ListarPetsPorTutor`)

Todos os nomes de arquivos, features, métodos e variáveis novas deverão ser em português do Brasil, salvo exceções de palavras reservadas como Command/Query, Response/Request.

Exemplos reais, exatamente como devem aparecer no projeto:

```
AplicativoPet.Api/Features/Pet/Command/CriarPet/
AplicativoPet.Api/Features/Pet/Command/AtualizarPet/
AplicativoPet.Api/Features/Pet/Query/BuscarPetPorId/
AplicativoPet.Api/Features/Usuarios/Query/BuscarUsuarioPorId/
AplicativoPet.Api/Features/Usuarios/Command/CriarUsuario/
```

Repare que `Command` e `Query` ficam no singular e com inicial maiúscula — siga essa convenção exata para consistência em todo o projeto.

## Os cinco arquivos de cada slice

O arquivo de teste é criado **primeiro** (Red), os quatro arquivos da API vêm depois (Green) — esta é a ordem TDD:

**Em `AplicativoPet.Tests/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/`** ← criado primeiro:
1. `{NomeDaAcao}Tests.cs`

**Em `AplicativoPet.Api/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/`** ← criados depois:
2. `{NomeDaAcao}Command.cs` (ou `Query.cs`)
3. `{NomeDaAcao}Handler.cs`
4. `{NomeDaAcao}Validator.cs`
5. `{NomeDaAcao}Endpoint.cs`

O projeto de testes espelha exatamente a estrutura de pastas da API. Isso garante rastreabilidade direta: ao deletar um slice, sabe-se exatamente qual pasta de teste deletar junto. Pacotes de teste (`NUnit`, `EF InMemory`) não vão para produção — por isso o arquivo de testes vive em projeto separado.

A dependência entre os arquivos da API segue a ordem de criação: o Handler implementa a interface que o Command declara, o Validator valida o Command, o Endpoint invoca via MediatR. Os Tests exercitam o Handler diretamente. Ver skill `tdd-dotnet` para template e regras dos testes.

Veja `references/anatomia-dos-arquivos.md` para o detalhamento de cada um com explicação linha a linha do que deve conter.

Veja `examples/CriarPet/` para um slice completo de Command e `examples/BuscarUsuarioPorId/` para um slice completo de Query — use-os como referência de estilo, nomenclatura e estrutura de código real.

Veja `references/postgresql-particularidades.md` quando o slice envolver tipos específicos do Postgres (JSONB, full-text search), convenção de nomenclatura snake_case, ou qualquer dúvida sobre como o EF Core se comporta de forma diferente com Npgsql em relação a outros providers.

## Regras inegociáveis

- **DbContext é injetado diretamente no Handler.** Não existe `IRepository<T>` genérico neste projeto. O Handler é o único lugar que toca o `DbContext`.
- **Validator nunca acessa banco de dados.** Validação de unicidade, existência de FK ou qualquer regra que precise consultar dados fica no Handler, não no Validator. O Validator valida apenas a forma dos dados de entrada (campos obrigatórios, formato, tamanho, ranges).
- **Command muda estado, Query apenas lê.** Nunca um Command retorna uma listagem grande de dados, nunca uma Query persiste algo no banco.
- **Um Handler nunca chama outro Handler diretamente.** Se uma operação precisa disparar outra, use `ISender.Send()` do MediatR — isso mantém o desacoplamento que justifica o padrão.
- **Nada de classes base abstratas para Handlers ou Endpoints.** Pequena repetição entre slices é aceitável e até desejável — ela mantém cada slice lendo de cima a baixo sem você precisar pular para uma classe base para entender o que acontece.
- **Cada slice é independente.** Um slice não referencia tipos internos de outro slice. Se dois slices precisam compartilhar algo (um Value Object, uma entidade de domínio), esse algo vive fora de `Features/`, no projeto de domínio compartilhado, como a pasta `Comum/`.
- **Migrations do EF Core seguem o nome da ação que as originou.** Ex: `dotnet ef migrations add CriarTabelaPets`, nunca nomes genéricos como `Update1`.
- **Sempre que precisar de um novo using que não tenha sido declarado antes no arquivo GlobalUsings.cs, declare-o unicamente lá.**

## Fluxo de criação de um novo slice

> **Esta skill é consultada durante a fase Green da skill `tdd-dotnet`.** Se você chegou aqui sem ler a `tdd-dotnet` primeiro, volte e leia — ela orquestra o processo completo. Os passos abaixo descrevem *como* escrever cada arquivo; a `tdd-dotnet` define *quando* e *em que ordem* criá-los.

Quando o usuário pedir uma nova funcionalidade, siga esta sequência:

1. **Identifique o módulo e a ação.** Pergunte se não estiver claro: qual entidade/agregado é (`Pet`, `Usuarios`, `Consultas`...) e qual é a ação (`Criar`, `Atualizar`, `BuscarPorId`, `Listar`...).
2. **Determine Command ou Query.** Muda dados → Command. Só lê → Query.
3. **Se precisar de novo modelo de domínio**, crie-o em `AplicativoPet.Api/Modelos/` herdando `EntidadeBase`.
4. **Escreva os Tests primeiro** (Red) — em `AplicativoPet.Tests/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/{NomeDaAcao}Tests.cs`. O build vai falhar até o próximo passo. Ver template completo na skill `tdd-dotnet`.
5. **Escreva o Command/Query** como `record` imutável — Commands implementam `ICommand<Result<T>>`, Queries implementam `IQuery<Result<T>>` (interfaces de `Blocos.Nucleo.CQRS`). Ver `references/anatomia-dos-arquivos.md` para o tipo `Result<T>`.
6. **Escreva o Handler** — Commands implementam `ICommandHandler<TCommand, Result<T>>`, Queries implementam `IQueryHandler<TQuery, Result<T>>`. Injete `AplicativoPetDbContext` via construtor primário. Sempre propague `CancellationToken`. Nunca lance exceção para cenário esperado — retorne `Result<T>.Falha(...)`. Após este passo, os testes devem passar (Green).
7. **Escreva o Validator** herdando `AbstractValidator<TCommand>`, cobrindo apenas validação estrutural. Cada `.WithMessage()` aplica-se ao validador imediatamente anterior na cadeia. Auto-descoberto via `AddValidatorsFromAssembly` — nenhum registro manual necessário.
8. **Escreva o Endpoint** implementando `ICarterModule` com `public void AddRoutes(IEndpointRouteBuilder app)`. Receba o `Request` do body, construa o Command/Query, envie via `ISender`, traduza `Result<T>` para status HTTP com `resultado.Sucesso ? ... : resultado.TipoErro switch { ... }`.
9. **Nenhum registro manual no `Program.cs`** — Carter auto-descobre todas as implementações de `ICarterModule` via `MapCarter()`. Validators são auto-descobertos via `AddValidatorsFromAssembly`. Handlers são auto-descobertos pelo MediatR via `RegisterServicesFromAssembly`.
10. **Rode o gate** antes de considerar concluído (ver seção abaixo).

## Modificando arquivos de um slice existente

Quando o pedido for "modifique", "altere", "adicione um campo" ou similar, a sequência de arquivos a atualizar segue esta lógica — consulte-a durante o Green da skill `tdd-dotnet`:

| Arquivo | Quando atualizar | O que muda |
|---|---|---|
| `Modelos/{Modelo}.cs` | Campo novo que precisa ser persistido | Adiciona propriedade à entidade |
| `{NomeDaAcao}Command.cs` | Campo novo na entrada da operação | Adiciona parâmetro ao `record` |
| `{NomeDaAcao}Handler.cs` | Sempre que Command ou modelo mudar | Mapeia o campo novo, ajusta lógica |
| `{NomeDaAcao}Validator.cs` | Campo novo com regra de validação | Adiciona `RuleFor` para o campo |
| `{NomeDaAcao}Endpoint.cs` | Raramente — apenas se a rota ou response mudar | Ajusta `.Produces<>()` se o Response mudou |

### Regras para modificações

- **Nunca remova um campo sem verificar se outros slices o referenciam.** Um campo da entidade pode ser lido por Queries mesmo que o Command que o criou seja o único a escrevê-lo.
- **Renomear um campo na entidade exige migration.** Migrations de renomeação (`RenameColumn`) precisam ser escritas à mão — o EF Core gera um Drop + Add por padrão, o que causa perda de dados.
- **Só gere migration se `Modelos/` mudou.** Alterar apenas o Command, Handler, Validator ou Endpoint não afeta o banco.
- **Nome da migration descreve a mudança, não o slice.** Use `AdicionaSobrenomeAoTutor`, não `AtualizaRegistrarUsuario`.

## Gate de validação

Antes de considerar a tarefa concluída, rode:

```bash
dotnet build AplicativoPet.Api/AplicativoPet.Api.csproj
dotnet test AplicativoPet.Tests/AplicativoPet.Tests.csproj
```

Os dois precisam passar sem erros novos. Se o modelo de domínio mudou, gere a migration antes do gate e confirme com `~/.dotnet/tools/dotnet-ef migrations list --project AplicativoPet.Api/AplicativoPet.Api.csproj`.

## Quando o pedido é ambíguo

Se o usuário pedir algo como "cria uma rota para gerenciar pets" sem especificar a ação exata, não assuma — pergunte qual operação específica ele quer primeiro (criar, listar, buscar por id, atualizar, deletar), porque cada uma é um slice próprio. Não crie um slice genérico "GerenciarPets" — isso quebra o princípio de uma ação por slice.
