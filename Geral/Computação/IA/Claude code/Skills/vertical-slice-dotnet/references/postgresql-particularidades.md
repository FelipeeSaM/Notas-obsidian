# PostgreSQL com EF Core (Npgsql) — Particularidades

Este projeto usa PostgreSQL através do provider `Npgsql.EntityFrameworkCore.PostgreSQL`. Existem diferenças de comportamento em relação ao SQL Server que merecem atenção ao escrever Handlers.

## Configuração do DbContext

Se o usuário ainda não tiver isso configurado, o registro do Npgsql no `Program.cs` é:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

A connection string do Postgres tem formato diferente do SQL Server:

```
Host=localhost;Port=5432;Database=aplicativopet;Username=postgres;Password=senha
```

## Convenção de nomenclatura: snake_case

O Postgres tradicionalmente usa `snake_case` para nomes de tabela e coluna, diferente do `PascalCase` que o EF Core usa por padrão em C#. Se o projeto seguir essa convenção (comum em times que vêm de outros ecossistemas como Rails ou Django), configure isso uma única vez no `OnModelCreating` do `AppDbContext` usando o pacote `EFCore.NamingConventions`:

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());
```

Com isso, uma entidade `Pet` com propriedade `TutorId` vira a tabela `pets` com coluna `tutor_id` automaticamente — sem precisar de `[Column("tutor_id")]` em cada propriedade. Se o projeto do usuário já usa `PascalCase` direto (Postgres aceita, só exige aspas em queries manuais), não force essa convenção.

## Tipos específicos do Postgres úteis em Handlers

Se um slice precisar desses recursos, aqui está como usá-los via EF Core:

**JSONB** — para campos com estrutura flexível (ex: metadados de um Pet que variam por espécie):

```csharp
public class Pet
{
    // ... outras propriedades
    public Dictionary<string, object> Metadados { get; set; } = new();
}

// No OnModelCreating:
modelBuilder.Entity<Pet>()
    .Property(p => p.Metadados)
    .HasColumnType("jsonb");
```

**Full-text search nativo** — quando uma Query precisa de busca textual (ex: `BuscarPetsPorNome`), prefira o full-text search do Postgres a um `LIKE '%texto%'`, que não usa índice:

```csharp
// No Handler de uma Query de busca:
var resultados = await db.Pets
    .Where(p => EF.Functions.ToTsVector("portuguese", p.Nome)
        .Matches(EF.Functions.ToTsQuery("portuguese", termoBusca)))
    .ToListAsync(cancellationToken);
```

Isso requer um índice GIN na coluna para performance — trate como uma migration separada quando o slice precisar disso.

## Geração de Guid como chave primária

O Postgres não gera `Guid` automaticamente como o SQL Server faz com `NEWSEQUENTIALID()`. Neste projeto, o padrão é gerar o `Guid` no próprio Handler com `Guid.NewGuid()` antes de adicionar a entidade — como já mostrado em `examples/CriarPet/CriarPetHandler.cs`. Não delegue a geração da chave para o banco a menos que o projeto tenha uma razão específica para isso (como ordenação sequencial via `uuid-ossp` com `uuid_generate_v1mc()`).

## Migrations

Comandos do EF Core para gerar e aplicar migrations contra o Postgres são os mesmos de qualquer provider — a diferença está só no SQL gerado internamente:

```bash
dotnet ef migrations add CriarTabelaPets --project AplicativoPet.Api
dotnet ef database update --project AplicativoPet.Api
```

Sempre nomeie a migration com o nome da ação de domínio que a originou (`CriarTabelaPets`, `AdicionarColunaTelefoneEmUsuarios`), nunca nomes genéricos como `Update1` ou `Migration20240101`.

## Erros comuns ao migrar de SQL Server para Postgres

Se o usuário ou o código existente trouxer hábitos de SQL Server, fique atento a:

- `NVARCHAR(MAX)` não existe — em Postgres, `text` já é ilimitado e eficiente. Não defina `MaxLength` desnecessariamente em campos de texto livre.
- `GETUTCDATE()` não existe — use `DateTime.UtcNow` no código C# ou `now() at time zone 'utc'` em SQL nativo.
- Case sensitivity em nomes — Postgres é case-sensitive para identificadores entre aspas. Se a convenção do projeto usa nomes sem aspas (o padrão), o Postgres normaliza tudo para minúsculas automaticamente, o que é o comportamento esperado e não deve causar surpresas.
