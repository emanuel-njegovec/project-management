using FluentValidation;
using RPPP_WebApp.ViewModels;

public class ZadatakDTOViewModelValidator : AbstractValidator<ZadatakDTOViewModel>
{
    public ZadatakDTOViewModelValidator()
    {
        RuleFor(zadatak => zadatak.IdZahtjevaNavigation).NotNull().WithMessage("Naziv zahtjeva je obavezan.");
        RuleFor(zadatak => zadatak.IdPrioritetaNavigation).NotNull().WithMessage("Naziv prioriteta je obavezan.");
        RuleFor(zadatak => zadatak.OibosobaNavigation).NotNull().WithMessage("Naziv osobe je obavezan.");
        RuleFor(zadatak => zadatak.IdStatusNavigation).NotNull().WithMessage("Naziv statusa je obavezan.");
        
        RuleFor(zadatak => zadatak.PlaniraniPocetak).NotEmpty().WithMessage("Planirani početak je obavezan.");
        RuleFor(zadatak => zadatak.PlaniraniZavrsetak).NotEmpty().WithMessage("Planirani završetak je obavezan.");
        RuleFor(zadatak => zadatak.PlaniraniZavrsetak).GreaterThanOrEqualTo(zadatak => zadatak.PlaniraniPocetak)
            .WithMessage("Planirani završetak mora biti nakon ili jednak planiranom početku.");
        
        RuleFor(zadatak => zadatak.StvarniPocetak).NotEmpty().WithMessage("Stvarni početak je obavezan.");
        RuleFor(zadatak => zadatak.StvarniZavrsetak).NotEmpty().WithMessage("Stvarni završetak je obavezan.");
        RuleFor(zadatak => zadatak.StvarniZavrsetak).GreaterThanOrEqualTo(zadatak => zadatak.StvarniPocetak)
            .WithMessage("Stvarni završetak mora biti nakon ili jednak stvarnom početku.");
        
        RuleFor(zadatak => zadatak.ImeZadatka).NotEmpty().WithMessage("Ime zadatka je obavezno.");


    }
}