using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation;

public class DokumentiValidator : AbstractValidator<Dokument>
{
    public DokumentiValidator()
    {
        RuleFor(d => d.IdProjektaNavigation)
            .NotEmpty()
            .WithMessage("Obaveazan projekt");

        RuleFor(d => d.IdVrsteDokumenta)
            .NotEmpty()
            .WithMessage("Obavezna vrsta dokumenta");

        RuleFor(d => d.DatumNastanka)
            .NotEmpty()
            .WithMessage("Obavezan datum nastanka");

        RuleFor(d => d.Format)
            .NotEmpty()
            .WithMessage("Obavezan format");
    }
}
