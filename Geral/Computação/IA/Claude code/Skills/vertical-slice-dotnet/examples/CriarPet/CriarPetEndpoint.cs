using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Pet.Command.CriarPet;

public class CriarPetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/pet/criar", async (CriarPetRequest request, ISender sender) =>
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
