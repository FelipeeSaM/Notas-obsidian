# Anti-Padrões em Testes NUnit — AplicativoPet

Cada seção mostra o erro mais comum e a forma correta para este projeto.

---

## 1. Múltiplos cenários não relacionados no mesmo `[Test]`

```csharp
// ERRADO — testa criação, busca e conflito no mesmo método
[Test]
public async Task RegistrarUsuario_Tudo()
{
    var resultado1 = await _handler.Handle(CriarCommand("a@a.com"), CancellationToken.None);
    Assert.That(resultado1.Sucesso, Is.True);

    var resultado2 = await _handler.Handle(CriarCommand("a@a.com"), CancellationToken.None);
    Assert.That(resultado2.TipoErro, Is.EqualTo(ResultadoTipoErro.Conflito));
}

// CORRETO — um comportamento por teste
[Test]
public async Task Handle_DadosValidos_RetornaSucessoComTutorId()
{
    var resultado = await _handler.Handle(CriarCommand("a@a.com"), CancellationToken.None);
    Assert.That(resultado.Sucesso, Is.True);
}

[Test]
public async Task Handle_EmailJaCadastrado_RetornaFalhaComConflito()
{
    await _handler.Handle(CriarCommand("a@a.com"), CancellationToken.None);
    var resultado = await _handler.Handle(CriarCommand("a@a.com"), CancellationToken.None);
    Assert.That(resultado.TipoErro, Is.EqualTo(ResultadoTipoErro.Conflito));
}
```

---

## 2. Banco compartilhado entre testes (testes interdependentes)

```csharp
// ERRADO — banco estático vaza dados entre testes
[TestFixture]
public class RegistrarUsuarioTests
{
    private static AplicativoPetDbContext _db = new(/* opções fixas */);

    [Test]
    public async Task Teste1() { /* insere tutor@a.com */ }

    [Test]
    public async Task Teste2() { /* falha se tutor@a.com já existe do Teste1 */ }
}

// CORRETO — banco novo por teste via Guid
[SetUp]
public void SetUp()
{
    var options = new DbContextOptionsBuilder<AplicativoPetDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())  // isolado
        .Options;
    _db = new AplicativoPetDbContext(options);
    _handler = new RegistrarUsuarioHandler(_db);
}

[TearDown]
public void TearDown() => _db.Dispose();
```

---

## 3. Estilo clássico de asserção (obsoleto no NUnit 4)

```csharp
// ERRADO — estilo clássico
Assert.IsTrue(resultado.Sucesso);
Assert.AreEqual(ResultadoTipoErro.NaoEncontrado, resultado.TipoErro);
Assert.IsNotNull(resultado.Valor);
Assert.IsInstanceOf<RegistrarUsuarioResponse>(resultado.Valor);

// CORRETO — constraint model com Assert.That
Assert.That(resultado.Sucesso, Is.True);
Assert.That(resultado.TipoErro, Is.EqualTo(ResultadoTipoErro.NaoEncontrado));
Assert.That(resultado.Valor, Is.Not.Null);
Assert.That(resultado.Valor, Is.InstanceOf<RegistrarUsuarioResponse>());
```

---

## 4. `async void` em testes (race condition silenciosa)

```csharp
// ERRADO — o runner pode encerrar antes da asserção executar
[Test]
public async void Handle_DadosValidos_RetornaSucesso()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    Assert.That(resultado.Sucesso, Is.True); // pode nunca rodar
}

// CORRETO — async Task garante que o runner aguarda
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    Assert.That(resultado.Sucesso, Is.True);
}
```

---

## 5. `.Result` ou `.GetAwaiter().GetResult()` bloqueando código async

```csharp
// ERRADO — risco de deadlock
[Test]
public void Handle_DadosValidos_RetornaSucesso()
{
    var resultado = _handler.Handle(command, CancellationToken.None).Result;
    Assert.That(resultado.Sucesso, Is.True);
}

// CORRETO
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    Assert.That(resultado.Sucesso, Is.True);
}
```

---

## 6. TearDown ausente — resource leak

```csharp
// ERRADO — _db nunca é descartado
[TestFixture]
public class BuscarUsuarioPorIdTests
{
    private AplicativoPetDbContext _db = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new AplicativoPetDbContext(/* ... */);
    }
    // sem TearDown → leak de memória acumulado por toda a suíte
}

// CORRETO
[TearDown]
public void TearDown() => _db.Dispose();
```

---

## 7. Asserção fraca — verifica apenas que não é null

```csharp
// ERRADO — não verifica nenhum comportamento real
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    Assert.That(resultado, Is.Not.Null);  // sempre passa, não prova nada
}

// CORRETO — verifica o que importa
[Test]
public async Task Handle_DadosValidos_RetornaSucessoComTutorId()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);

    Assert.That(resultado.Sucesso, Is.True);
    Assert.That(resultado.Valor!.TutorId, Is.Not.EqualTo(Guid.Empty));

    var tutorSalvo = await _db.Tutores.FindAsync(resultado.Valor.TutorId);
    Assert.That(tutorSalvo!.Email, Is.EqualTo(command.Email));
}
```

---

## 8. Mocks desnecessários — testa os mocks, não o código

```csharp
// ERRADO — está testando que o mock retorna o que foi configurado
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    var mockDb = Substitute.For<AplicativoPetDbContext>();
    mockDb.Tutores.AnyAsync(Arg.Any<...>()).Returns(false);

    var handler = new RegistrarUsuarioHandler(mockDb);
    var resultado = await handler.Handle(command, CancellationToken.None);

    Assert.That(resultado.Sucesso, Is.True);
}

// CORRETO — UseInMemoryDatabase testa o comportamento real do Handler
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    Assert.That(resultado.Sucesso, Is.True);
}
```

---

## 9. `DateTime.Now` não determinístico em asserções

```csharp
// ERRADO — race condition: o tempo pode mudar entre a chamada e a asserção
[Test]
public async Task Handle_DadosValidos_SalvaDataCadastro()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    var tutor = await _db.Tutores.FindAsync(resultado.Valor!.TutorId);
    Assert.That(tutor!.DataCadastro, Is.EqualTo(DateTime.UtcNow)); // falha intermitente
}

// CORRETO — use tolerância ou capture o tempo antes e depois
[Test]
public async Task Handle_DadosValidos_SalvaDataCadastro()
{
    var antes = DateTime.UtcNow;
    var resultado = await _handler.Handle(command, CancellationToken.None);
    var depois = DateTime.UtcNow;

    var tutor = await _db.Tutores.FindAsync(resultado.Valor!.TutorId);
    Assert.That(tutor!.DataCadastro, Is.InRange(antes, depois));
}
```

---

## 10. Exceção silenciada no `catch`

```csharp
// ERRADO — o teste passa mesmo se o Handler lançar exceção
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    try
    {
        var resultado = await _handler.Handle(command, CancellationToken.None);
        Assert.That(resultado.Sucesso, Is.True);
    }
    catch
    {
        // silenciado — esconde o bug
    }
}

// CORRETO — deixe a exceção propagar, o runner a captura e repota o teste como falha
[Test]
public async Task Handle_DadosValidos_RetornaSucesso()
{
    var resultado = await _handler.Handle(command, CancellationToken.None);
    Assert.That(resultado.Sucesso, Is.True);
}
```
