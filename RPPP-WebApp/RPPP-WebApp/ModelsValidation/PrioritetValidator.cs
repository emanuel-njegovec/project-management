using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelValidation;
public class PrioritetValidator : AbstractValidator<Prioritet>
{
    public PrioritetValidator()
    {
        RuleFor(d => d.Naziv)
          .NotEmpty().WithMessage("Prioritet je obvezno polje");
    }
}