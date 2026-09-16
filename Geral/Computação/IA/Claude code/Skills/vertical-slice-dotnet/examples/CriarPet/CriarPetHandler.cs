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
