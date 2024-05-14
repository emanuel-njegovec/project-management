using FluentValidation;
using RPPP_WebApp.Models;
using RPPP_WebApp.Views.Projekt;

namespace RPPP_WebApp.ModelsValidation;

public class ProjektDTOValidator : AbstractValidator<ProjektDTO>
{
    public ProjektDTOValidator()
    {
        RuleFor(p => p.Naziv)
            .NotEmpty()
            .WithMessage("Naziv projekta je obavezno.");
        
        RuleFor(p => p.DatumPocetka)
            .NotEmpty()
            .WithMessage("Datum početka je obavezan.");
        
        RuleFor(p => p.DatumZavrsetka)
            .GreaterThanOrEqualTo(p => p.DatumPocetka)
            .WithMessage("Datum završetka mora biti nakon ili jednak datumu početka.");

        RuleFor(p => p.Oibnarucitelj)
            .NotEmpty()
            .WithMessage("Narucitelj je obavezan.");
        
        RuleFor(p => p.VrstaProjekta)
            .NotEmpty()
            .WithMessage("Vrsta projekta je obavezna.");
        
    }
}