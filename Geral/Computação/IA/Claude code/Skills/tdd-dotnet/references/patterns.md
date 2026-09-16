# Padrões Avançados NUnit — AplicativoPet

---

## 1. Assert.Multiple — todas as falhas de uma vez

Sem `Assert.Multiple`, o primeiro `Assert.That` que falha interrompe o teste. Com ele, todas as asserções são avaliadas e todas as falhas são reportadas juntas.

```csharp
[Test]
public async Task Handle_UsuarioExistente_RetornaSucessoComDadosCorretos()
{
    // Arrange
    var tutor = new Tutor
    {
        TutorId = Guid.NewGuid(),
        Nome = "Felipe",
        Email = "felipe@teste.com",
        Telefone = "11999999999",
        TipoUsuarioEnum = TipoUsuarioEnum.Basico,
        DataNascimento = new DateTime(1995, 6, 15, 0, 0, 0, DateTimeKind.Utc)
    };
    _db.Tutores.Add(tutor);
    await _db.SaveChangesAsync();

    // Act
    var resultado = await _handler.Handle(
        new BuscarUsuarioPorIdQuery(tutor.TutorId), CancellationToken.None);

    // Assert
    Assert.That(resultado.Sucesso, Is.True);
    Assert.Multiple(() =>
    {
        Assert.That(resultado.Valor!.TutorId, Is.EqualTo(tutor.TutorId));
        Assert.That(resultado.Valor.Nome, Is.EqualTo("Felipe"));
        Assert.That(resultado.Valor.Email, Is.EqualTo("felipe@teste.com"));
        Assert.That(resultado.Valor.Telefone, Is.EqualTo("11999999999"));
    });
}
```

Use `Assert.Multiple` sempre que verificar mais de uma propriedade da resposta. Use `Assert.That` simples quando verificar apenas `Sucesso` ou `TipoErro`.

---

## 2. TestCase — parametrização inline

Use `[TestCase]` para cobrir múltiplas variações de entrada com o mesmo comportamento esperado, sem duplicar o corpo do teste.

```csharp
// Validator: campos inválidos que devem falhar
[TestCase("", "Email", Description = "Email vazio")]
[TestCase("nao-e-email", "Email", Description = "Email malformado")]
[TestCase("a@", "Email", Description = "Email incompleto")]
public void Validator_EmailInvalido_FalhaNaValidacao(string email, string propriedadeEsperada)
{
    var validator = new RegistrarUsuarioValidator();
    var command = CriarCommand(email: email);

    var resultado = validator.Validate(command);

    Assert.That(resultado.IsValid, Is.False);
    Assert.That(resultado.Errors, Has.Some.Property("PropertyName").EqualTo(propriedadeEsperada));
}

// Handler: IDs diferentes que não existem no banco
[TestCase("00000000-0000-0000-0000-000000000001")]
[TestCase("00000000-0000-0000-0000-000000000002")]
public async Task Handle_IdInexistente_RetornaNaoEncontrado(string guidStr)
{
    var resultado = await _handler.Handle(
        new BuscarUsuarioPorIdQuery(Guid.Parse(guidStr)), CancellationToken.None);

    Assert.That(resultado.TipoErro, Is.EqualTo(ResultadoTipoErro.NaoEncontrado));
}
```

---

## 3. TestCaseSource — parametrização com dados complexos

Use `[TestCaseSource]` quando os dados de entrada são objetos ou quando a descrição do caso precisa ser mais elaborada.

```csharp
private static IEnumerable<TestCaseData> ComandosInvalidos()
{
    yield return new TestCaseData(CriarCommand(nome: ""))
        .SetName("NomeVazio")
        .SetDescription("Deve falhar quando nome está vazio");

    yield return new TestCaseData(CriarCommand(email: "invalido"))
        .SetName("EmailMalformado")
        .SetDescription("Deve falhar quando email não tem formato válido");

    yield return new TestCaseData(CriarCommand(senha: "123"))
        .SetName("SenhaCurta")
        .SetDescription("Deve falhar quando senha tem menos de 6 caracteres");
}

[TestCaseSource(nameof(ComandosInvalidos))]
public void Validator_DadosInvalidos_FalhaNaValidacao(RegistrarUsuarioCommand command)
{
    var validator = new RegistrarUsuarioValidator();
    var resultado = validator.Validate(command);

    Assert.That(resultado.IsValid, Is.False);
}
```

---

## 4. Asserções em coleções

```csharp
// Verifica que todos os elementos atendem a uma condição
Assert.That(pets, Is.All.Property("TutorId").EqualTo(tutorId));

// Verifica que ao menos um elemento atende
Assert.That(pets, Has.Some.Property("Nome").EqualTo("Rex"));

// Verifica que nenhum elemento tem propriedade nula
Assert.That(pets, Has.None.Property("Nome").Null);

// Verifica quantidade
Assert.That(pets, Has.Count.EqualTo(3));
Assert.That(pets, Has.Count.GreaterThan(0));

// Verifica ordenação
Assert.That(pets.Select(p => p.Nome), Is.Ordered.Ascending);
```

---

## 5. Verificando que uma entidade foi persistida no banco

Além de verificar o `Result<T>`, confirme que o dado foi realmente salvo — especialmente no caminho feliz de Commands.

```csharp
[Test]
public async Task Handle_DadosValidos_PersisteTutorNoBanco()
{
    var command = CriarCommand();

    var resultado = await _handler.Handle(command, CancellationToken.None);

    // Verifica o retorno
    Assert.That(resultado.Sucesso, Is.True);

    // Confirma que foi salvo no banco
    var tutorSalvo = await _db.Tutores.FindAsync(resultado.Valor!.TutorId);
    Assert.That(tutorSalvo, Is.Not.Null);
    Assert.That(tutorSalvo!.Email, Is.EqualTo(command.Email));
    Assert.That(tutorSalvo.Nome, Is.EqualTo(command.Nome));
}
```

---

## 6. Helper privado para montar o Command/Query

Extraia a criação do command para um método privado com parâmetros opcionais. Isso evita repetição e deixa cada teste declarar apenas o que é relevante para ele.

```csharp
// Método helper no fundo da classe de teste
private static RegistrarUsuarioCommand CriarCommand(
    string nome = "Felipe",
    string email = "tutor@teste.com",
    string senha = "Senh@123",
    string confirmacaoSenha = "Senh@123",
    string telefone = "11999999999",
    TipoUsuarioEnum tipo = TipoUsuarioEnum.Basico,
    DateTime? dataNascimento = null) =>
    new(
        Nome: nome,
        Email: email,
        Senha: senha,
        ConfirmacaoSenha: confirmacaoSenha,
        Telefone: telefone,
        TipoUsuario: tipo,
        DataNascimento: dataNascimento ?? new DateTime(1995, 6, 15, 0, 0, 0, DateTimeKind.Utc)
    );
```

Uso:
```csharp
// Teste de caminho feliz — usa todos os defaults
var command = CriarCommand();

// Teste de conflito — troca só o email
var command = CriarCommand(email: "outro@teste.com");

// Teste de validator — troca só o campo sob teste
var command = CriarCommand(nome: "");
```

---

## 7. Arrange com entidade no banco antes do Act

Para Queries ou Commands que dependem de uma entidade existente, insira-a diretamente no `_db` no Arrange — sem passar pelo Handler de criação.

```csharp
[Test]
public async Task Handle_UsuarioExistente_RetornaSucessoComDadosCorretos()
{
    // Arrange — insere direto, sem passar pelo RegistrarUsuarioHandler
    var tutor = new Tutor
    {
        TutorId = Guid.NewGuid(),
        Nome = "Felipe",
        Email = "felipe@teste.com",
        Senha = "hash-qualquer",
        Telefone = "11999999999",
        TipoUsuarioEnum = TipoUsuarioEnum.Basico,
        DataNascimento = new DateTime(1995, 6, 15, 0, 0, 0, DateTimeKind.Utc)
    };
    _db.Tutores.Add(tutor);
    await _db.SaveChangesAsync();

    // Act
    var resultado = await _handler.Handle(
        new BuscarUsuarioPorIdQuery(tutor.TutorId), CancellationToken.None);

    // Assert
    Assert.That(resultado.Sucesso, Is.True);
    Assert.That(resultado.Valor!.Nome, Is.EqualTo("Felipe"));
}
```

Não chame outros Handlers no Arrange — isso cria dependência entre slices dentro do teste e esconde qual Handler está falhando quando o teste quebra.

---

## 8. Testando o Validator diretamente (sem Handler)

```csharp
[Test]
public void Validator_NomeVazio_FalhaNaValidacao()
{
    var validator = new RegistrarUsuarioValidator();
    var command = CriarCommand(nome: "");

    var resultado = validator.Validate(command);

    Assert.That(resultado.IsValid, Is.False);
    Assert.That(resultado.Errors, Has.Some.Property("PropertyName").EqualTo("Nome"));
}

[Test]
public void Validator_ConfirmacaoSenhaDiferente_FalhaNaValidacao()
{
    var validator = new RegistrarUsuarioValidator();
    var command = CriarCommand(senha: "Senh@123", confirmacaoSenha: "Diferente@456");

    var resultado = validator.Validate(command);

    Assert.That(resultado.IsValid, Is.False);
    Assert.That(resultado.Errors,
        Has.Some.Property("PropertyName").EqualTo("ConfirmacaoSenha"));
}
```

Instancie o Validator diretamente — não via MediatR pipeline. O objetivo é testar apenas as regras de validação, isoladas do Handler.

---

## 9. Tolerância em comparações de DateTime

```csharp
// Verificar que DataCadastro foi preenchida com o horário atual
[Test]
public async Task Handle_DadosValidos_SalvaDataCadastroCorreta()
{
    var antes = DateTime.UtcNow;
    var resultado = await _handler.Handle(CriarCommand(), CancellationToken.None);
    var depois = DateTime.UtcNow;

    var tutor = await _db.Tutores.FindAsync(resultado.Valor!.TutorId);

    Assert.That(tutor!.DataCadastro, Is.InRange(antes, depois));
}

// Verificar igualdade com tolerância de 1 segundo
Assert.That(tutor.DataCadastro,
    Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
```
