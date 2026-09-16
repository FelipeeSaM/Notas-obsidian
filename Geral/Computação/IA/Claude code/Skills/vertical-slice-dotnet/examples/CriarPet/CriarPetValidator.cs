namespace AplicativoPet.Api.Features.Pet.Command.CriarPet;

public class CriarPetValidator : AbstractValidator<CriarPetCommand>
{
    public CriarPetValidator()
    {
        // Apenas validação estrutural. Nada aqui consulta o banco —
        // a existência do tutor é responsabilidade do Handler.
        RuleFor(x => x.Nome)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Raca)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Idade)
            .InclusiveBetween(0, 40);

        RuleFor(x => x.TutorId)
            .NotEmpty();
    }
}
