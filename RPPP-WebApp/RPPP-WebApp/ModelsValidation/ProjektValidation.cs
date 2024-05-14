using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation;

public class ProjektValidator : AbstractValidator<Projekt>
{
    public ProjektValidator()
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
        
        RuleFor(p => p.IdVrsteProjekta)
            .NotEmpty()
            .WithMessage("Vrsta projekta je obavezna.");
        
    }
    
}