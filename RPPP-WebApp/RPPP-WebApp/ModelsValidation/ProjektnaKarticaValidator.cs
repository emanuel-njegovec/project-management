using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class ProjektnaKarticaValidator : AbstractValidator<ProjektnaKartica>
    {
        public ProjektnaKarticaValidator() {
            RuleFor(k => k.Ibankartice).NotEmpty().WithMessage("Potrebno je unijeti IBAN kartice");
            RuleFor(k => k.Stanje).GreaterThanOrEqualTo(0).WithMessage("Stanje ne smije biti negativno");
            RuleFor(k => k.Stanje).NotEmpty().WithMessage("Potrebno je unijeti stanje kartice");
            RuleFor(k => k.IdProjekta).NotEmpty().WithMessage("Potrebno je odabrati projekt");
            RuleFor(k => k.Oibosoba).NotEmpty().WithMessage("Potrebno je odabrati zaduženu osobu");
        }
    }
}
