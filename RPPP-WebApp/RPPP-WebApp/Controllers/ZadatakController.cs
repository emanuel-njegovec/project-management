using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.ModelsValidation;
using RPPP_WebApp.Views.Zadatak;


namespace RPPP_WebApp.Controllers;

public class ZadatakController : Controller
{
    
    private readonly DBContext context;
    private readonly AppSettings appSettings;
    private readonly ILogger<ZadatakController> logger;
    ZadatakValidator ZadatakValidator = new ZadatakValidator();
    ZadatakDTOValidator ZadatakDTOValidator = new ZadatakDTOValidator();
    ZadatakDTOViewModelValidator ZadatakDTOViewModelValidator = new ZadatakDTOViewModelValidator();


    public ZadatakController(DBContext context, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakController> logger)
    {
        this.context = context;
        this.logger = logger;
        appSettings = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appSettings.PageSize;
        
        var query = context.Zadatci
            .Include(z => z.IdZahtjevaNavigation)
            .Include(z => z.IdStatusNavigation)
            .Include(z => z.OibosobaNavigation)
            .Include(z => z.IdPrioritetaNavigation)
            .AsNoTracking();
        
        if (query.Count() == 0)
        {
            logger.LogInformation("Ne postoji nijedan zadatak");
            TempData[Constants.Message] = "Ne postoji niti jedan zadatak.";
            TempData[Constants.ErrorOccurred] = false;
            //return RedirectToAction(nameof(Create));
        }
        
        
        var zadci = query.OrderBy(g => g.IdZadatka).ToList();
        
        var pagingInfo = new PagingInfo
        {
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pagesize,
            TotalItems = query.Count()
        };
        
        if (page < 1 || page > pagingInfo.TotalPages)
        {
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }
        
        query = query.ApplySort(sort, ascending);
        
        
        var svizad = query
            
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToList();

        var model = new ZadatakViewModel()
        {
            Zadatci = svizad,
            PagingInfo = pagingInfo
        };
        return View(model);
    }
    
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PrepareDropDownLists();
        return View();
    }

    private async Task PrepareDropDownLists()
    {
        var PrviStatus = await context.Zadatci                  
            .Select(d => new { d.IdStatusNavigation.Naziv })
            .Distinct()
            .FirstOrDefaultAsync();
        
        var SviStatusi = await context.Zadatci                      
            .OrderBy(d => d.IdStatusNavigation.Naziv)
            .Select(d => new { d.IdStatusNavigation.Naziv,})
            .Distinct()
            .ToListAsync();
        
        
        
        var PrviZahtjev = await context.Zadatci                  
            .Select(d => new { d.IdZahtjevaNavigation.Naslov })
            .Distinct()
            .FirstOrDefaultAsync();
        
        var SviZahtjevi = await context.Zadatci                      
            .OrderBy(d => d.IdZahtjevaNavigation.Naslov)
            .Select(d => new { d.IdZahtjevaNavigation.Naslov,})
            .Distinct()
            .ToListAsync();
        
        
        
        var PrviPrioritet = await context.Zadatci                  
            .Select(d => new { d.IdPrioritetaNavigation.Naziv })
            .Distinct()
            .FirstOrDefaultAsync();
        
        var SviPrioriteti = await context.Zadatci                      
            .OrderBy(d => d.IdPrioritetaNavigation.Naziv)
            .Select(d => new { d.IdPrioritetaNavigation.Naziv,})
            .Distinct()
            .ToListAsync();

        
        //ViewBag.Statusi = new SelectList(SviStatusi, nameof(Zadatak.IdStatusNavigation.Naziv), nameof(Zadatak.IdStatusNavigation.Naziv), PrviStatus.Naziv);
        //ViewBag.Zahtjevi = new SelectList(SviZahtjevi, nameof(Zadatak.IdZahtjevaNavigation.Naslov), nameof(Zadatak.IdZahtjevaNavigation.Naslov), PrviZahtjev.Naslov);
        //ViewBag.Prioriteti = new SelectList(SviPrioriteti, nameof(Zadatak.IdPrioritetaNavigation.Naziv), nameof(Zadatak.IdPrioritetaNavigation.Naziv), PrviPrioritet.Naziv);


        var uahtjevi = await context.Zahtjevi
            .OrderBy(d => d.IdZahtjeva)
            .Select(d => new SelectListItem
            {
                Value = d.Naslov,  
                Text = $"{d.Naslov}" 
            })
            .ToListAsync();
        
        var statusici = await context.Statusi
            .OrderBy(d => d.IdStatus)
            .Select(d => new SelectListItem
            {
                Value = d.Naziv,  
                Text = $"{d.Naziv}"  
            })
            .ToListAsync();
        
        var prior = await context.Prioriteti
            .OrderBy(d => d.IdPrioriteta)
            .Select(d => new SelectListItem
            {
                Value = d.Naziv,  
                Text = $"{d.Naziv}" 
            })
            .ToListAsync();
        
        
        var ljudovi = await context.Osobe
            .OrderBy(d => d.Oibosoba)
            .Select(d => new SelectListItem
            {
                Value = d.Ime + " " + d.Prezime, 
                Text = $"{d.Ime + " " + d.Prezime}"  
            })
            .ToListAsync();


        ViewBag.Zahtjevi = new SelectList(uahtjevi, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        ViewBag.Statusi = new SelectList(statusici, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        ViewBag.Prioriteti = new SelectList(prior, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        ViewBag.Ljudi = new SelectList(ljudovi, nameof(SelectListItem.Value), nameof(SelectListItem.Text));


    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Views.Zadatak.ZadatakDTO zadatakDTO)
    {
        Zadatak zadatakPraviFake = new Zadatak();
        Views.Zadatak.ZadatakDTO zadatakDTOPraviFake = new Views.Zadatak.ZadatakDTO();
        
        var rezultatValidacije = ZadatakDTOValidator.Validate(zadatakDTO);

        if (!rezultatValidacije.IsValid)
        {
            foreach (var error in rezultatValidacije.Errors)
            {
                Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
            }

        }
        
        if (ModelState.IsValid)
        {
            try
            {
                
                int idPrioriteta = context.Prioriteti
                    .Where(z => z.Naziv == zadatakDTO.NazPrioriteta)
                    .Select(z => z.IdPrioriteta)
                    .FirstOrDefault();
                
                int idStatusa = context.Statusi
                    .Where(z => z.Naziv == zadatakDTO.NazStatus)
                    .Select(z => z.IdStatus)
                    .FirstOrDefault();
                
                int idZahtjeva = context.Zahtjevi
                    .Where(z => z.Naslov == zadatakDTO.NazZahtjeva)
                    .Select(z => z.IdPrioriteta)
                    .FirstOrDefault();
                
                string[] dijelovi = zadatakDTO.NazOsoba.Split(' ');

                int oibic = context.Osobe
                    .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                    .Select(z => z.Oibosoba)
                    .FirstOrDefault();

                Zadatak zadatakPravi = new Zadatak(zadatakDTO.ImeZadatka, zadatakDTO.PlaniraniPocetak, zadatakDTO.PlaniraniZavrsetak, zadatakDTO.StvarniPocetak, zadatakDTO.StvarniZavrsetak, idZahtjeva, idStatusa, oibic, idPrioriteta);




                

                context.Add(zadatakPravi);
                await context.SaveChangesAsync();

                TempData[Constants.Message] = $"Zadatak {zadatakPravi.IdZadatka} dodan.";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Zadatak {zadatakPravi.ImeZadatka} dodan.";
                return RedirectToAction(nameof(Index));

            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                TempData["errorMessage"] = $"Neuspjelo dodavanje zadatka. Provjerite jesu li svi podaci uneseni";
                await PrepareDropDownLists();
                return View(zadatakDTOPraviFake);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return View(zadatakDTOPraviFake);
        }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            var zadatak = await context.Zadatci.FindAsync(id);

            if (zadatak == null)
            {
                TempData[Constants.Message] = "Zadatak nije pronađen.";
                TempData["errorMessage"] = "Zadatak nije pronađen.";
                TempData[Constants.ErrorOccurred] = true;
            }
            else
            {
                context.Zadatci.Remove(zadatak);
                await context.SaveChangesAsync();

                TempData[Constants.Message] = $"Zadatak {id} obrisan.";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Zadatak {zadatak.ImeZadatka} obrisan.";
            }
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Error deleting Zadatak");
            TempData[Constants.Message] = "Greška prilikom brisanja zadatka";
            TempData[Constants.ErrorOccurred] = true;
            TempData["errorMessage"] = "Greška prilikom brisanja zadatka. Provjerite brišete li Zadatak koji je strani ključ nekom drugom podatku";
        }

        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

    }

    [HttpPost]
    public async Task<IActionResult> Edit(ZadatakDTOViewModel zaddto)
    {
        if (zaddto == null)
        {
            
            return BadRequest("Invalid input: zaddto je null");
        }
        var rezultatValidacije = ZadatakDTOViewModelValidator.Validate(zaddto);

        if (!rezultatValidacije.IsValid)
        {
            foreach (var error in rezultatValidacije.Errors)
            {
                Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
            }

        }
        
        /*
        
        if (zaddto == null)
        {
            return NotFound("Nema poslanih podataka");
        }
        
        if (zaddto.IdPrioritetaNavigation == null)
        {
            return NotFound($"Zaboravili ste označiti Prioritet");
        }
        
        if (zaddto.IdStatusNavigation == null)
        {
            return NotFound($"Zaboravili ste označiti Status");
        }
        
        if (zaddto.IdZahtjevaNavigation == null)
        {
            return NotFound($"Zaboravili ste označiti Zahtjev");
        }
        
        if (zaddto.OibosobaNavigation == null)
        {
            return NotFound($"Zaboravili ste označiti Osobu");
        }
        
        
        */
            
        
        
        
        bool checkId = await context.Zadatci.AnyAsync(m => m.IdZadatka == zaddto.IdZadatka);
        
        if (!checkId)
        {
            return NotFound($"Neispravan ID zadatka: {zaddto?.IdZadatka}");
        }
        
       
        
        
        if (ModelState.IsValid)
        {
            int idPrioriteta = context.Prioriteti
                .Where(z => z.Naziv == zaddto.IdPrioritetaNavigation.Naziv)
                .Select(z => z.IdPrioriteta)
                .FirstOrDefault();
                
            int idStatusa = context.Statusi
                .Where(z => z.Naziv == zaddto.IdStatusNavigation.Naziv)
                .Select(z => z.IdStatus)
                .FirstOrDefault();
                
            int idZahtjeva = context.Zahtjevi
                .Where(z => z.Naslov == zaddto.IdZahtjevaNavigation.Naslov)
                .Select(z => z.IdPrioriteta)
                .FirstOrDefault();
        
            string[] dijelovi = zaddto.OibosobaNavigation.Ime.Split(' ');

            int oibic = context.Osobe
                .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                .Select(z => z.Oibosoba)
                .FirstOrDefault();
            try
            {

                Zadatak doka = new Zadatak(zaddto.IdZadatka, zaddto.ImeZadatka, zaddto.PlaniraniPocetak,
                    zaddto.PlaniraniZavrsetak, zaddto.StvarniPocetak, zaddto.StvarniZavrsetak, idZahtjeva, idStatusa,
                    oibic, idPrioriteta);
                
                Zadatak dokabez = new Zadatak(zaddto.ImeZadatka, zaddto.PlaniraniPocetak,
                    zaddto.PlaniraniZavrsetak, zaddto.StvarniPocetak, zaddto.StvarniZavrsetak, idZahtjeva, idStatusa,
                    zaddto.Oibosoba, idPrioriteta);
                context.Update(doka);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Zadatak {doka.ImeZadatka} obrisan.";
                return RedirectToAction(nameof(Index));
                //return RedirectToAction(nameof(Get), new { id = mjesto.IdMjesta });
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                await PrepareDropDownLists();
                return PartialView(zaddto);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return PartialView(zaddto);
        }

     
    }



    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      
        
        var zad = await context.Zadatci                            
            .Where(m => m.IdZadatka == id)
            .Select(m => new ZadatakDTOViewModel()
            {
                IdZadatka = m.IdZadatka,
                ImeZadatka = m.ImeZadatka,
                PlaniraniPocetak = m.PlaniraniPocetak,
                PlaniraniZavrsetak = m.PlaniraniZavrsetak,
                StvarniPocetak = m.StvarniPocetak,
                StvarniZavrsetak = m.StvarniZavrsetak,
                IdZahtjeva = m.IdZahtjeva,
                IdStatus = m.IdStatus,
                Oibosoba = m.Oibosoba,
                IdPrioriteta = m.IdPrioriteta,
                
            })
            .SingleOrDefaultAsync();

        Views.Zadatak.ZadatakDTO d = new Views.Zadatak.ZadatakDTO();
        
        if (zad != null)
        {        
            await PrepareDropDownLists();
            return PartialView(zad);
        }
        else
        {
            return NotFound($"Neispravan ID zadatka: {id}");
        }
    }


    [HttpGet]
    public async Task<IActionResult> Show(int id)
    {
        
        var query = context.EvidencijaRada
            .Include(z => z.OibosobaNavigation)
            .Include(z => z.IdZadatkaNavigation)
            .Include(z => z.IdVrstePoslaNavigation)
            .AsNoTracking();

        int brojsvih = context.EvidencijaRada.Select(q => q.IdZadatka == id).Count();

        var svizad = query
            .Where(q => q.IdZadatka == id)
            .Take(brojsvih)
            .ToList();
        
        var model = new EvidencijaRadaViewModel()
        {
            EviRadovi = svizad
        };
        
        var radic = await context.EvidencijaRada                            
            .Where(m => m.IdEvidencijaRad == id)
            .Select(m => new EvidencijaRada()
            {
                IdEvidencijaRad = m.IdEvidencijaRad,
                Opis = m.Opis,
                DatumRada = m.DatumRada,
                VrijemeRada = m.VrijemeRada,
                Oibosoba = m.Oibosoba,
                IdZadatka = m.IdZadatka,
                IdVrstePosla = m.IdVrstePosla,
                
            })
            .SingleOrDefaultAsync();

        //ZadatakDTO d = new ZadatakDTO();
        
        return View(model);
    }
}