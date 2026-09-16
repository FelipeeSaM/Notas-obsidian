---
name: tdd-dotnet
description: Skill orquestradora de criação e modificação de features neste projeto. Lida PRIMEIRO, antes da vertical-slice-dotnet. Define o ciclo TDD completo tanto para features novas quanto para alterações em features existentes. Use sempre que o usuário pedir uma nova feature OU usar palavras como "modifique", "altere", "adicione um campo", "atualize", "corrija" em relação a qualquer feature, rota, endpoint, command ou query.
---

# TDD + NUnit — AplicativoPet

## Como esta skill orquestra a vertical-slice-dotnet

Esta skill é a **orquestradora**: define quando e em que ordem cada arquivo é criado. A `vertical-slice-dotnet` é a **referência de implementação**: define como cada arquivo deve ser escrito. A relação é de hierarquia, não de igualdade.

| Esta skill (`tdd-dotnet`) | `vertical-slice-dotnet` |
|---|---|
| Orquestra o processo completo | Detalha a implementação de cada arquivo |
| Governa o Red: testes primeiro | É consultada durante o Green |
| Define quando invocar a VSA | Responde sobre estrutura de código |

A sequência é sempre:

```
Red   → {NomeDaAcao}Tests.cs                          (esta skill)
Green → consulte vertical-slice-dotnet para cada arquivo:
        Command/Query → Handler → Validator → Endpoint
Gate  → dotnet build + dotnet test
```

Durante o Green, para cada arquivo, consulte a `vertical-slice-dotnet`. Se houver conflito entre as duas skills, esta skill vence — ela define o processo, a outra define o detalhe.

## O ciclo Red → Green → Refactor

### Red — escreva o teste que vai falhar

Antes de criar qualquer arquivo em `AplicativoPet.Api/Features/`, crie o arquivo de teste em:

```
AplicativoPet.Tests/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/{NomeDaAcao}Tests.cs
```

O teste **deve falhar** — inclusive pode não compilar, porque o Handler que ele referencia ainda não existe. Isso é correto: o Red confirma que o teste realmente vai verificar algo, não apenas passar em vazio.

### Green — implemente o mínimo para os testes passarem

Crie os arquivos da feature seguindo a skill `vertical-slice-dotnet` na ordem:

1. `{NomeDaAcao}Command.cs` / `{NomeDaAcao}Query.cs` — para o build compilar novamente
2. `{NomeDaAcao}Handler.cs` — para os testes passarem (Green)
3. `{NomeDaAcao}Validator.cs`
4. `{NomeDaAcao}Endpoint.cs`

Implemente apenas o suficiente para os testes passarem. Sem otimizações prematuras, sem abstrações extras.

### Refactor — melhore sem quebrar

Com os testes verdes, refatore livremente: renomeie variáveis, ajuste projeções EF Core, extraia lógica duplicada. Os testes são a rede de segurança — se quebrarem, você foi longe demais.

## Estrutura de pastas — espelhamento obrigatório

```
AplicativoPet.Tests/                              ← Red (criado primeiro)
└── Features/
    └── {Modulo}/
        └── {Command|Query}/
            └── {NomeDaAcao}/
                └── {NomeDaAcao}Tests.cs

AplicativoPet.Api/                                ← Green (criado depois)
└── Features/
    └── {Modulo}/
        └── {Command|Query}/
            └── {NomeDaAcao}/
                ├── {NomeDaAcao}Command.cs
                ├── {NomeDaAcao}Handler.cs
                ├── {NomeDaAcao}Validator.cs
                └── {NomeDaAcao}Endpoint.cs
```

Se a feature precisar de um novo modelo de domínio, crie-o em `AplicativoPet.Api/Modelos/` herdando `EntidadeBase` antes do arquivo de teste — o teste precisará referenciar a entidade no Arrange.

## Anatomia do arquivo de teste

### Pacotes (já no AplicativoPet.Tests.csproj)

- `NUnit` — framework
- `NUnit3TestAdapter` — integração com o runner do dotnet
- `Microsoft.NET.Test.Sdk` — descoberta de testes
- `Microsoft.EntityFrameworkCore.InMemory` — banco isolado por teste

### Template completo

```csharp
// AplicativoPet.Tests/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/{NomeDaAcao}Tests.cs
using AplicativoPet.Api.Banco;
using AplicativoPet.Api.Comum;
using AplicativoPet.Api.Features.{Modulo}.{Command|Query}.{NomeDaAcao};
using AplicativoPet.Api.Modelos;       // quando precisar criar entidades no Arrange
using Blocos.Nucleo.Enums;             // quando precisar de enums de domínio
using Microsoft.EntityFrameworkCore;

namespace AplicativoPet.Tests.Features.{Modulo}.{Command|Query}.{NomeDaAcao};

[TestFixture]
public class {NomeDaAcao}Tests
{
    private AplicativoPetDbContext _db = null!;
    private {NomeDaAcao}Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AplicativoPetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())   // banco isolado por teste
            .Options;

        _db = new AplicativoPetDbContext(options);
        _handler = new {NomeDaAcao}Handler(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Handle_{ContextoDoArranjo}_{ResultadoEsperado}()
    {
        // Arrange
        var command = /* ... */;

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(resultado.Sucesso, Is.True);
        Assert.That(resultado.Valor!.{Propriedade}, Is.EqualTo({ValorEsperado}));
    }
}
```

### Ciclo de vida do fixture

| Atributo | Quando executa | Uso neste projeto |
|---|---|---|
| `[SetUp]` | Antes de cada `[Test]` | Recria `_db` e `_handler` limpos |
| `[TearDown]` | Após cada `[Test]` | Descarta `_db` para liberar memória |
| `[OneTimeSetUp]` | Uma vez por fixture | Não usar — `[SetUp]` garante isolamento |

## Modelo de asserções — constraint-based

Use sempre `Assert.That()`. Nunca use o estilo clássico (`Assert.AreEqual`, `Assert.IsTrue`) — ele está obsoleto no NUnit 4.

```csharp
// CORRETO
Assert.That(resultado.Sucesso, Is.True);
Assert.That(resultado.Valor!.TutorId, Is.Not.EqualTo(Guid.Empty));
Assert.That(resultado.TipoErro, Is.EqualTo(ResultadoTipoErro.NaoEncontrado));
Assert.That(tutorSalvo!.Senha, Is.Not.EqualTo(command.Senha));

// ERRADO — não usar
Assert.IsTrue(resultado.Sucesso);
Assert.AreEqual(ResultadoTipoErro.NaoEncontrado, resultado.TipoErro);
```

### Assert.Multiple — valide várias propriedades sem parar no primeiro erro

```csharp
Assert.Multiple(() =>
{
    Assert.That(resultado.Valor!.TutorId, Is.Not.EqualTo(Guid.Empty));
    Assert.That(resultado.Valor.Nome, Is.EqualTo("Felipe"));
    Assert.That(resultado.Valor.Email, Is.EqualTo("felipe@teste.com"));
});
```

Use `Assert.Multiple` quando o teste verifica várias propriedades de uma resposta. Sem ele, o primeiro `Assert.That` que falha interrompe o teste e você não sabe quais outros campos também estão incorretos.

## Cobertura mínima obrigatória por slice

| Cenário | Assertion esperada |
|---|---|
| Caminho feliz | `resultado.Sucesso == true` + propriedades do `Valor` corretas |
| Entidade não encontrada | `resultado.Sucesso == false` + `TipoErro == ResultadoTipoErro.NaoEncontrado` |
| Conflito / duplicidade | `resultado.Sucesso == false` + `TipoErro == ResultadoTipoErro.Conflito` |
| Validator com regra não trivial | `resultado.IsValid == false` + `PropertyName` correto |

Não escreva teste de Validator quando ele só tem `NotEmpty` — isso não cobre nenhuma lógica real.

## Convenção de nomenclatura

```
Handle_{ContextoDoArranjo}_{ResultadoEsperado}
Validator_{CampoInvalido}_{ResultadoEsperado}
```

Exemplos reais do projeto:
- `Handle_DadosValidos_RetornaSucessoComTutorId`
- `Handle_DadosValidos_SenhaSalvaComoHash`
- `Handle_EmailJaCadastrado_RetornaFalhaComConflito`
- `Handle_UsuarioInexistente_RetornaFalhaComTipoNaoEncontrado`
- `Validator_NomeVazio_FalhaNaValidacao`

## Testes assíncronos

Sempre assine `async Task` — nunca `async void`:

```csharp
// CORRETO
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    Assert.That(resultado.Sucesso, Is.True);
}

// ERRADO — o teste pode encerrar antes da asserção rodar
[Test]
public async void Handle_DadosValidos_RetornaSucesso() { ... }
```

## Regras inegociáveis

- **Testes primeiro.** Um arquivo de teste que não compila é Red válido — é o ponto de partida correto.
- **Banco isolado por teste.** `Guid.NewGuid().ToString()` no nome do banco garante que nenhum dado vaza entre testes.
- **Testes testam o Handler, não o Endpoint.** O Endpoint é cola HTTP — não contém lógica de negócio para testar unitariamente.
- **Um comportamento por `[Test]`.** Não misture dois cenários no mesmo método.
- **Sem mocks.** `UseInMemoryDatabase` é suficiente e mantém os testes legíveis.
- **`async Task`, nunca `async void`.**
- **`Assert.That()` sempre.** Constraint-based, nunca estilo clássico.

## Referências

- `references/anti-patterns.md` — erros comuns em testes NUnit com exemplos ERRADO/CORRETO adaptados para este projeto
- `references/patterns.md` — padrões avançados: parametrização, `Assert.Multiple`, coleções, ciclo de vida

## Sequência completa — feature nova

```
[Opcional] Novo modelo de domínio:
  0. AplicativoPet.Api/Modelos/{Modelo}.cs  (herda EntidadeBase)

Red (testes primeiro):
  1. AplicativoPet.Tests/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/{NomeDaAcao}Tests.cs
     → dotnet build falha (esperado)

Green (consulte vertical-slice-dotnet para cada arquivo):
  2. {NomeDaAcao}Command.cs / {NomeDaAcao}Query.cs  → build volta
  3. {NomeDaAcao}Handler.cs                          → testes passam
  4. {NomeDaAcao}Validator.cs
  5. {NomeDaAcao}Endpoint.cs

Gate:
  dotnet build AplicativoPet.Api/AplicativoPet.Api.csproj
  dotnet test AplicativoPet.Tests/AplicativoPet.Tests.csproj
```

## Sequência completa — modificando uma feature existente

Gatilhos: "modifique", "altere", "adicione um campo", "atualize", "corrija", ou qualquer pedido que mude algo em um slice já existente.

O ciclo TDD continua valendo — a diferença é que você **atualiza** arquivos existentes em vez de criar do zero, e o Red é "fazer o teste existente falhar com a mudança esperada".

```
[Se o modelo de domínio muda — ex: novo campo na entidade]:
  0. AplicativoPet.Api/Modelos/{Modelo}.cs  → adiciona a propriedade

Red (atualize o teste existente primeiro):
  1. AplicativoPet.Tests/Features/{Modulo}/{Command|Query}/{NomeDaAcao}/{NomeDaAcao}Tests.cs
     → atualiza o helper CriarCommand() / CriarQuery() com o novo campo
     → atualiza ou adiciona Assert para o novo comportamento
     → dotnet build falha (campo ainda não existe na implementação)

Green (consulte vertical-slice-dotnet para cada arquivo afetado):
  2. {NomeDaAcao}Command.cs   → adiciona o campo ao record
  3. {NomeDaAcao}Handler.cs   → mapeia o campo novo na lógica
     → dotnet test passa
  4. {NomeDaAcao}Validator.cs → adiciona RuleFor para o campo (se necessário)
  5. {NomeDaAcao}Endpoint.cs  → raramente muda (Request já passa tudo)

[Se o modelo de domínio mudou — migration obrigatória]:
  6. ~/.dotnet/tools/dotnet-ef migrations add {DescricaoDaMudanca} \
       --project AplicativoPet.Api/AplicativoPet.Api.csproj
     Ex: AdicionaSobrenomeAoTutor, RemoveTelefoneDoPet

Gate:
  dotnet build AplicativoPet.Api/AplicativoPet.Api.csproj
  dotnet test AplicativoPet.Tests/AplicativoPet.Tests.csproj
```

### Regras específicas para modificações

- **Nunca altere a implementação antes do teste.** Mesmo em modificações, o Red vem primeiro — atualize o teste, veja falhar, depois atualize o código.
- **Atualize o helper `CriarCommand()`** com o novo campo antes de qualquer outra coisa. O compilador vai apontar todos os lugares que precisam mudar.
- **Migration com nome descritivo.** O nome deve descrever a mudança, não a feature: `AdicionaSobrenomeAoTutor`, não `AtualizaRegistrarUsuario`.
- **Só gere migration se o modelo de domínio mudou.** Adicionar campo apenas ao Command/Request não requer migration.
