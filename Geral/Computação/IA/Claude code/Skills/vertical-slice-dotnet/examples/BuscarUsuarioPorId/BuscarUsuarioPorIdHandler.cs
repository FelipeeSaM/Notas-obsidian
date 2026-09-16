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
