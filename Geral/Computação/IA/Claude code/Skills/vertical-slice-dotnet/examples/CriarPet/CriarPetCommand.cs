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
