using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelValidation;
public class StatusValidator : AbstractValidator<Status>
{
    public StatusValidator()
    {
        RuleFor(d => d.Naziv)
          .NotEmpty().WithMessage("Status je obvezno polje");
    }
}