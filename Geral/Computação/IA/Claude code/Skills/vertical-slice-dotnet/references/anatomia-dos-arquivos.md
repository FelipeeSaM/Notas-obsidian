# Anatomia dos Arquivos de um Slice

Este documento detalha o conteúdo esperado de cada um dos cinco arquivos de um slice, com o raciocínio por trás de cada decisão. Leia isto quando estiver escrevendo um slice e precisar de detalhe além do resumo no SKILL.md.

## O tipo Result<T>

Este projeto não usa exceções para fluxo de controle esperado (registro não encontrado, validação de negócio, conflito). Handlers retornam `Result<T>`, que representa sucesso ou falha de forma explícita. O tipo vive em `AplicativoPet.Api/Comum/Result.cs`:

```csharp
namespace AplicativoPet.Api.Comum;

public class Result<T>
{
    public bool Sucesso { get; }
    public T? Valor { get; }
    public string? Erro { get; }
    public ResultadoTipoErro? TipoErro { get; }

    private Result(bool sucesso, T? valor, string? erro, ResultadoTipoErro? tipoErro)
    {
        Sucesso = sucesso;
        Valor = valor;
        Erro = erro;
        TipoErro = tipoErro;
    }

    public static Result<T> Ok(T valor) => new(true, valor, null, null);

    public static Result<T> Falha(string erro, ResultadoTipoErro tipoErro = ResultadoTipoErro.RegraDeNegocio)
        => new(false, default, erro, tipoErro);
}

public enum ResultadoTipoErro
{
    NaoEncontrado,
    Conflito,
    RegraDeNegocio
}
```

O `ResultadoTipoErro` permite que o Endpoint traduza a falha para o status HTTP correto (404, 409, 400) sem o Handler precisar saber nada sobre HTTP.

---

## 1. {NomeDaAcao}Command.cs ou Query.cs

Contém três coisas: o `Request` (DTO que chega do cliente), o `Command`/`Query` (o que entra no pipeline do MediatR) e o `Response` (o que sai).

**Command:**
```csharp
using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Pet.Command.CriarPet;

public record CriarPetRequest(
    string Nome,
    string Raca,
    int Idade,
    Guid TutorId
);

public record CriarPetCommand(
    string Nome,
    string Raca,
    int Idade,
    Guid TutorId
) : ICommand<Result<CriarPetResponse>>;

public record CriarPetResponse(Guid PetId);
```

**Query:**
```csharp
using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Usuarios.Query.BuscarUsuarioPorId;

public record BuscarUsuarioPorIdQuery(Guid TutorId) : IQuery<Result<BuscarUsuarioPorIdResponse>>;

public record BuscarUsuarioPorIdResponse(Guid TutorId, string Nome, string Email, DateTime DataCadastro);
```

Queries simples (busca por id) não têm `Request` separado — o parâmetro vem direto da rota.

Por que `record`: imutabilidade por padrão evita modificações no meio do pipeline do MediatR, e a igualdade estrutural facilita os testes.

---

## 2. {NomeDaAcao}Handler.cs

O coração do slice. Commands implementam `ICommandHandler<TCommand, Result<T>>`. Queries implementam `IQueryHandler<TQuery, Result<T>>`. Usa primary constructor para injeção de dependência.

**Command Handler:**
```csharp
using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Pet.Command.CriarPet;

public class CriarPetHandler(AplicativoPetDbContext db)
    : ICommandHandler<CriarPetCommand, Result<CriarPetResponse>>
{
    public async Task<Result<CriarPetResponse>> Handle(
        CriarPetCommand request, CancellationToken cancellationToken)
    {
        var tutorExiste = await db.Tutores
            .AnyAsync(t => t.TutorId == request.TutorId, cancellationToken);

        if (!tutorExiste)
            return Result<CriarPetResponse>.Falha("Tutor não encontrado.", ResultadoTipoErro.NaoEncontrado);

        var pet = new Pet
        {
            PetId = Guid.NewGuid(),
            Nome = request.Nome,
            TutorId = request.TutorId
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(cancellationToken);

        return Result<CriarPetResponse>.Ok(new CriarPetResponse(pet.PetId));
    }
}
```

**Query Handler:**
```csharp
using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Usuarios.Query.BuscarUsuarioPorId;

public class BuscarUsuarioPorIdHandler(AplicativoPetDbContext db)
    : IQueryHandler<BuscarUsuarioPorIdQuery, Result<BuscarUsuarioPorIdResponse>>
{
    public async Task<Result<BuscarUsuarioPorIdResponse>> Handle(
        BuscarUsuarioPorIdQuery request, CancellationToken cancellationToken)
    {
        var tutor = await db.Tutores
            .AsNoTracking()
            .Where(t => t.TutorId == request.TutorId)
            .Select(t => new BuscarUsuarioPorIdResponse(t.TutorId, t.Nome, t.Email, t.DataCadastro))
            .FirstOrDefaultAsync(cancellationToken);

        return tutor is null
            ? Result<BuscarUsuarioPorIdResponse>.Falha("Usuário não encontrado.", ResultadoTipoErro.NaoEncontrado)
            : Result<BuscarUsuarioPorIdResponse>.Ok(tutor);
    }
}
```

Pontos a observar:
- `AplicativoPetDbContext db` é o único parâmetro do construtor primário — sem abstração de repositório.
- Regras de negócio que dependem do banco (existência de FK, unicidade) ficam aqui, nunca no Validator.
- `CancellationToken` é sempre propagado para as chamadas assíncronas ao EF Core.
- Queries usam `.AsNoTracking()` e projetam direto no `.Select()` — o Postgres traz só as colunas necessárias.
- O Handler nunca lança exceção para cenário esperado — retorna `Result<T>.Falha(...)`.
- Caso o guard clause de, por exemplo, tutor is null, retorne ResultadoTipoErro.NaoEncontrado. Caso seja alguma inconsistência entre dados, utilize o ResultadoTipoErro.Falha

---

## 3. {NomeDaAcao}Validator.cs

Herda `AbstractValidator<TCommand>`. Cobre exclusivamente validação estrutural — nunca consulta o banco.
Para os casos de .MaximumLength(), reflita o .HasMaximumLength() da mesma propriedade nos arquivos de configuração /banco/ConfigurarcoesModelo/{nomeModelo}Config.cs
```csharp
namespace AplicativoPet.Api.Features.Pet.Command.CriarPet;

public class CriarPetValidator : AbstractValidator<CriarPetCommand>
{
    public CriarPetValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome não pode ser vazio")
            .MaximumLength(100).WithMessage("Nome não pode ultrapassar 100 caracteres");

        RuleFor(x => x.Idade)
            .InclusiveBetween(0, 40).WithMessage("Idade deve estar entre 0 e 40 anos");

        RuleFor(x => x.TutorId)
            .NotEmpty().WithMessage("TutorId deve ser informado");
    }
}
```

Cada `.WithMessage()` se aplica ao validador imediatamente anterior na cadeia — não ao conjunto. Queries simples de busca por id geralmente não precisam de Validator.

---

## 4. {NomeDaAcao}Endpoint.cs

Implementa `ICarterModule`. Carter auto-descobre todas as implementações via `MapCarter()` no `Program.cs` — nenhum registro manual é necessário. Não tem lógica de negócio: deserializa, envia para o MediatR e traduz `Result<T>` para o status HTTP correto.
A rota deve sempre acompanhar a ação. Ex: /api/pet/buscar/ | /api/usuario/registrar
. Se o usuário pedir para criar uma nova funcionalidade de remover, e que apenas mude um valor de, por exemplo, ativo para inativo, use o MapPut ao invés de MapDelete.

**Command Endpoint:**
```csharp
using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Pet.Command.CriarPet;

public class CriarPetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/pet/{ação}", async (CriarPetRequest request, ISender sender) =>
        {
            var command = new CriarPetCommand(
                request.Nome,
                request.Raca,
                request.Idade,
                request.TutorId
            );

            var resultado = await sender.Send(command);

            return resultado.Sucesso
                ? Results.Created($"/api/pet/{resultado.Valor!.PetId}", resultado.Valor)
                : resultado.TipoErro switch
                {
                    ResultadoTipoErro.NaoEncontrado => Results.NotFound(resultado.Erro),
                    ResultadoTipoErro.Conflito => Results.Conflict(resultado.Erro),
                    _ => Results.BadRequest(resultado.Erro)
                };
        })
        .WithName("CriarPet")
        .WithTags("Pets")
        .Produces<CriarPetResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();
    }
}
```

**Query Endpoint:**
```csharp
using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Usuarios.Query.BuscarUsuarioPorId;

public class BuscarUsuarioPorIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/usuario/{id:guid}", async (Guid id, ISender sender) =>
        {
            var resultado = await sender.Send(new BuscarUsuarioPorIdQuery(id));

            return resultado.Sucesso
                ? Results.Ok(resultado.Valor)
                : resultado.TipoErro switch
                {
                    ResultadoTipoErro.NaoEncontrado => Results.NotFound(resultado.Erro),
                    _ => Results.BadRequest(resultado.Erro)
                };
        })
        .WithName("BuscarUsuarioPorId")
        .WithTags("Usuarios")
        .Produces<BuscarUsuarioPorIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
```

A tradução de `ResultadoTipoErro` para status HTTP fica sempre no Endpoint — é a única camada que sabe sobre HTTP.

---

## 5. {NomeDaAcao}Tests.cs

NUnit, no projeto `AplicativoPet.Tests`. Testa o Handler diretamente com banco em memória. Nomenclatura: `Handle_Cenario_ResultadoEsperado`.

```csharp
using AplicativoPet.Api.Banco;
using AplicativoPet.Api.Comum;
using AplicativoPet.Api.Features.Pet.Command.CriarPet;
using AplicativoPet.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace AplicativoPet.Tests.Features.Pet.Command.CriarPet;

[TestFixture]
public class CriarPetTests
{
    private AplicativoPetDbContext _db = null!;
    private CriarPetHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AplicativoPetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AplicativoPetDbContext(options);
        _handler = new CriarPetHandler(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Handle_TutorExistente_RetornaSucessoComIdDoPet()
    {
        var tutor = new Tutor { TutorId = Guid.NewGuid(), Nome = "Felipe", Email = "felipe@teste.com" };
        _db.Tutores.Add(tutor);
        await _db.SaveChangesAsync();

        var resultado = await _handler.Handle(
            new CriarPetCommand("Rex", "Vira-lata", 3, tutor.TutorId), CancellationToken.None);

        Assert.That(resultado.Sucesso, Is.True);
        Assert.That(resultado.Valor!.PetId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Handle_TutorInexistente_RetornaFalhaComTipoNaoEncontrado()
    {
        var resultado = await _handler.Handle(
            new CriarPetCommand("Rex", "Vira-lata", 3, Guid.NewGuid()), CancellationToken.None);

        Assert.That(resultado.Sucesso, Is.False);
        Assert.That(resultado.TipoErro, Is.EqualTo(ResultadoTipoErro.NaoEncontrado));
    }

    [Test]
    public void Validator_NomeVazio_FalhaNaValidacao()
    {
        var validator = new CriarPetValidator();

        var resultado = validator.Validate(new CriarPetCommand("", "Vira-lata", 3, Guid.NewGuid()));

        Assert.That(resultado.IsValid, Is.False);
        Assert.That(resultado.Errors, Has.Some.Property("PropertyName").EqualTo("Nome"));
    }
}
```

Cobertura mínima por slice:
- **Caminho feliz** — operação funciona como esperado
- **Falha de regra de negócio** — não encontrado, conflito, etc.
- **Falha de validação** — para validators com regras não triviais

Se o slice depender de comportamento específico do Postgres (JSONB, full-text search), prefira Testcontainers em vez de InMemory.
