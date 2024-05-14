using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation;

public class ZadatakValidator : AbstractValidator<Zadatak>
{
    public ZadatakValidator()
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

        RuleFor(zadatak => zadatak.IdZahtjeva).NotEmpty().WithMessage("Naziv zahtjeva je obavezan.");
        RuleFor(zadatak => zadatak.IdStatus).NotEmpty().WithMessage("Naziv statusa je obavezan.");
        RuleFor(zadatak => zadatak.Oibosoba).NotEmpty().WithMessage("Naziv osobe je obavezan.");
        RuleFor(zadatak => zadatak.IdPrioriteta).NotEmpty().WithMessage("Naziv prioriteta je obavezan.");
     
        
    }
    
}