using AplicativoPet.Api.Banco;
using AplicativoPet.Api.Comum;
using AplicativoPet.Api.Features.Usuarios.Query.BuscarUsuarioPorId;
using AplicativoPet.Api.Modelos;
using Blocos.Nucleo.Enums;
using Microsoft.EntityFrameworkCore;

namespace AplicativoPet.Tests.Features.Usuarios.Query.BuscarUsuarioPorId;

[TestFixture]
public class BuscarUsuarioPorIdTests
{
    private AplicativoPetDbContext _db = null!;
    private BuscarUsuarioPorIdHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AplicativoPetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AplicativoPetDbContext(options);
        _handler = new BuscarUsuarioPorIdHandler(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Handle_UsuarioExistente_RetornaSucessoComDadosCorretos()
    {
        var tutor = new Tutor
        {
            TutorId = Guid.NewGuid(),
            Nome = "Felipe",
            Email = "felipe@teste.com",
            Senha = "hash",
            TipoUsuarioEnum = TipoUsuarioEnum.Basico,
            DataNascimento = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _db.Tutores.Add(tutor);
        await _db.SaveChangesAsync();

        var resultado = await _handler.Handle(new BuscarUsuarioPorIdQuery(tutor.TutorId), CancellationToken.None);

        Assert.That(resultado.Sucesso, Is.True);
        Assert.That(resultado.Valor!.Nome, Is.EqualTo("Felipe"));
        Assert.That(resultado.Valor.Email, Is.EqualTo("felipe@teste.com"));
    }

    [Test]
    public async Task Handle_UsuarioInexistente_RetornaFalhaComTipoNaoEncontrado()
    {
        var resultado = await _handler.Handle(new BuscarUsuarioPorIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(resultado.Sucesso, Is.False);
        Assert.That(resultado.TipoErro, Is.EqualTo(ResultadoTipoErro.NaoEncontrado));
    }
}
