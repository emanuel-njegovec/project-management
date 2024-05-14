using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation;


public class EvidencijaRadaValidator : AbstractValidator<EvidencijaRada>
{
    public EvidencijaRadaValidator()
    {
        RuleFor(rad => rad.Opis).NotEmpty().WithMessage("Opis rada je obavezan");
        RuleFor(rad => rad.DatumRada).NotEmpty().WithMessage("Datum rada je obavezan");
        RuleFor(rad => rad.VrijemeRada).NotEmpty().WithMessage("Vrijeme rada je obavezan");
        //RuleFor(rad => rad.IdZadatka).NotEmpty().WithMessage("Zadatak je obavezan");
        //RuleFor(rad => rad.IdVrstePosla).NotEmpty().WithMessage("Vrsta posla je obavezna");
        //RuleFor(rad => rad.Oibosoba).NotEmpty().WithMessage("Osoba je obavezna");
        
        RuleFor(rad => rad.OibosobaNavigation).NotEmpty().WithMessage("Osoba je obavezna");
        RuleFor(rad => rad.IdVrstePoslaNavigation).NotEmpty().WithMessage("Vrsta posla je obavezna");
        RuleFor(rad => rad.IdZadatkaNavigation).NotEmpty().WithMessage("Zadatak je obavezan");
        //RuleFor(rad => rad.OibosobaNavigation.Ime).NotEmpty().WithMessage("Osoba je obavezna");


    }
}