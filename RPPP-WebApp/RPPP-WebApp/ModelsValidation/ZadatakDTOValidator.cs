using FluentValidation;
using RPPP_WebApp.Models;
using RPPP_WebApp.Views.Zadatak;
using ZadatakDTO = RPPP_WebApp.Views.Zadatak.ZadatakDTO;

namespace RPPP_WebApp.ModelsValidation;

public class ZadatakDTOValidator : AbstractValidator<ZadatakDTO>
{
    public ZadatakDTOValidator()
    {
        RuleFor(zadatak => zadatak.ImeZadatka).NotEmpty().WithMessage("Ime zadatka je obavezno.");
        
        RuleFor(zadatak => zadatak.PlaniraniPocetak).NotEmpty().WithMessage("Planirani početak je obavezan.");
        RuleFor(zadatak => zadatak.PlaniraniZavrsetak).NotEmpty().WithMessage("Planirani završetak je obavezan.");
        RuleFor(zadatak => zadatak.PlaniraniZavrsetak).GreaterThanOrEqualTo(zadatak => zadatak.PlaniraniPocetak)
            .WithMessage("Planirani završetak mora biti nakon ili jednak planiranom početku.");

        RuleFor(zadatak => zadatak.StvarniPocetak).NotEmpty().WithMessage("Stvarni početak je obavezan.");
        RuleFor(zadatak => zadatak.StvarniZavrsetak).NotEmpty().WithMessage("Stvarni završetak je obavezan.");
        RuleFor(zadatak => zadatak.StvarniZavrsetak).GreaterThanOrEqualTo(zadatak => zadatak.StvarniPocetak)
            .WithMessage("Stvarni završetak mora biti nakon ili jednak stvarnom početku.");

        RuleFor(zadatak => zadatak.NazZahtjeva).NotEmpty().WithMessage("Naziv zahtjeva je obavezan.");
        RuleFor(zadatak => zadatak.NazStatus).NotEmpty().WithMessage("Naziv statusa je obavezan.");
        RuleFor(zadatak => zadatak.NazOsoba).NotEmpty().WithMessage("Naziv osobe je obavezan.");
        RuleFor(zadatak => zadatak.NazPrioriteta).NotEmpty().WithMessage("Naziv prioriteta je obavezan.");
    
    }
}