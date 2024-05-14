using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelValidation;
public class VrstaZahtjevaValidator : AbstractValidator<VrstaZahtjeva>
{
    public VrstaZahtjevaValidator()
    {
        RuleFor(d => d.Naziv)
          .NotEmpty().WithMessage("Naziv vrste zahtjeva je obvezno polje");

        RuleFor(d => d.Opis)
          .NotEmpty().WithMessage("Opis je obvezno polje");
    }
}