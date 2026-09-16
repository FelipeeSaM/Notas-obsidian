using AplicativoPet.Api.Comum;

namespace AplicativoPet.Api.Features.Usuarios.Query.BuscarUsuarioPorId;

public record BuscarUsuarioPorIdQuery(Guid TutorId) : IQuery<Result<BuscarUsuarioPorIdResponse>>;

public record BuscarUsuarioPorIdResponse(Guid TutorId, string Nome, string Email, DateTime DataCadastro);
