using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Usuarios.Query.BuscarUsuarioPorId;

public class BuscarUsuarioPorIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/usuario/buscar/{id:guid}", async (Guid id, ISender sender) =>
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
