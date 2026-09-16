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

        var command = new CriarPetCommand("Rex", "Vira-lata", 3, tutor.TutorId);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.That(resultado.Sucesso, Is.True);
        Assert.That(resultado.Valor!.PetId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Handle_TutorInexistente_RetornaFalhaComTipoNaoEncontrado()
    {
        var command = new CriarPetCommand("Rex", "Vira-lata", 3, Guid.NewGuid());

        var resultado = await _handler.Handle(command, CancellationToken.None);

        Assert.That(resultado.Sucesso, Is.False);
        Assert.That(resultado.TipoErro, Is.EqualTo(ResultadoTipoErro.NaoEncontrado));
    }

    [Test]
    public void Validator_NomeVazio_FalhaNaValidacao()
    {
        var validator = new CriarPetValidator();
        var command = new CriarPetCommand("", "Vira-lata", 3, Guid.NewGuid());

        var resultado = validator.Validate(command);

        Assert.That(resultado.IsValid, Is.False);
        Assert.That(resultado.Errors, Has.Some.Property("PropertyName").EqualTo("Nome"));
    }
}
