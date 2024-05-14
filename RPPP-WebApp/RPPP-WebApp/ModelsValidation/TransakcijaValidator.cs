using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class TransakcijaValidator : AbstractValidator<Transakcija>
    {
        public TransakcijaValidator()
        {
            RuleFor(k => k.Iznos).NotEmpty().WithMessage("Potrebno je unijeti iznos transakcije");
            RuleFor(k => k.Iban2zaTransakciju).NotEmpty().WithMessage("Potrebno je unijeti IBAN");
            RuleFor(k => k.Datum).NotEmpty().WithMessage("Potrebno je odabrati datum transakcije");
        }
    }
}
