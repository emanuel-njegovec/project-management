using FluentValidation;
using RPPP_WebApp.ViewModels;

public class ProjektDTOViewModelValidator : AbstractValidator<ProjektDTOViewModel>
{
    public ProjektDTOViewModelValidator()
    {

        RuleFor(p => p.Naziv)
            .NotEmpty()
            .WithMessage("Naziv projekta je obavezno.");

        RuleFor(p => p.IdVrsteProjektaNavigation)
            .NotNull()
            .WithMessage("Vrsta projekta je obavezna.");

        RuleFor(p => p.OibnaruciteljNavigation)
            .NotNull()
            .WithMessage("Narucitelj je obavezan.");
        
        RuleFor(p => p.DatumPocetka)
            .NotEmpty()
            .WithMessage("Datum početka je obavezan.");
        
        RuleFor(p => p.DatumZavrsetka)
            .GreaterThanOrEqualTo(p => p.DatumPocetka)
            .WithMessage("Datum završetka mora biti nakon ili jednak datumu početka.");

    }
}