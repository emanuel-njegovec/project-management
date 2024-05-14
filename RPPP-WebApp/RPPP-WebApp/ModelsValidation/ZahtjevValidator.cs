using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelValidation;
  public class ZahtjevValidator : AbstractValidator<Zahtjev>
  {
    public ZahtjevValidator()
    {
        RuleFor(d => d.Naslov)
          .NotEmpty().WithMessage("Naziv zahtjeva je obvezno polje");        

      RuleFor(d => d.Opis)        
        .NotEmpty().WithMessage("Opis je obvezno polje");

        RuleFor(d => d.IdVrsteZahtjevaNavigation.Naziv)
        .NotEmpty().WithMessage("Vrsta zahtjeva je obvezno polje");

        RuleFor(d => d.IdPrioritetaNavigation.Naziv)
        .NotEmpty().WithMessage("Prioritet je obvezno polje");

        RuleFor(d => d.IdProjektaNavigation.Naziv)
	    .NotEmpty().WithMessage("Projekt je obvezno polje");
    }
  }