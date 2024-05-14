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

public class EvidencijaRadaController : Controller
{
    private readonly DBContext context;
    private readonly AppSettings appSettings;
    private readonly ILogger<EvidencijaRadaController> logger;
    
    //ZadatakValidator ZadatakValidator = new ZadatakValidator();
    //ZadatakDTOValidator ZadatakDTOValidator = new ZadatakDTOValidator();
    //ZadatakDTOViewModelValidator

    private EvidencijaRadaValidator EvidencijaRadaValidator = new EvidencijaRadaValidator();

    
    public EvidencijaRadaController(DBContext context, IOptionsSnapshot<AppSettings> options, ILogger<EvidencijaRadaController> logger)
    {
        this.context = context;
        this.logger = logger;
        appSettings = options.Value;
    }
    
    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appSettings.PageSize;
        
        var query = context.EvidencijaRada
            .Include(z => z.OibosobaNavigation)
            .Include(z => z.IdZadatkaNavigation)
            .Include(z => z.IdVrstePoslaNavigation)
            .Include(z => z.IdZadatkaNavigation)
            .ThenInclude(z => z.IdZahtjevaNavigation)
            .ThenInclude(z => z.IdProjektaNavigation)
            .AsNoTracking();
        
        if (query.Count() == 0)
        {
            logger.LogInformation("Ne postoji niti jedna evidencija rada.");
            TempData[Constants.Message] = "Ne postoji niti jedna evidencija rada.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Create));
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
        
        
        var sviradovi = query
            
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToList();

        var model = new EvidencijaRadaViewModel()
        {
            EviRadovi = sviradovi,
            PagingInfo = pagingInfo
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            var rad = await context.EvidencijaRada.FindAsync(id);

            if (rad == null)
            {
                
                TempData[Constants.Message] = "Rad nije pronađen.";
                TempData[Constants.ErrorOccurred] = true;
            }
            else
            {
                context.EvidencijaRada.Remove(rad);
                await context.SaveChangesAsync();
                logger.LogInformation($"Rad {rad.Opis} uspješno obrisan");

                TempData[Constants.Message] = $"Rad {id} obrisan.";
                TempData["SuccessMessage"] = $"Rad {rad.Opis} uspješno obrisan";

                TempData[Constants.ErrorOccurred] = false;
            }
        }
        catch (Exception exc)
        {
            TempData[Constants.Message] = "Greška prilikom brisanja rada";
            TempData[Constants.ErrorOccurred] = true;
            logger.LogError(exc, "Error deleting rada");

        }

        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

        
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


       
        
        var zadatkovi = await context.Zadatci
            .OrderBy(d => d.IdZadatka)
            .Select(d => new SelectListItem
            {
                Value = d.ImeZadatka,  
                Text = $"{d.ImeZadatka}"  
            })
            .ToListAsync();
        
        var vrstePoslova = await context.VrstePosla
            .OrderBy(d => d.IdVrstePosla)
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


        ViewBag.Zadatci = new SelectList(zadatkovi, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        ViewBag.VrstePosla = new SelectList(vrstePoslova, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        ViewBag.Ljudi = new SelectList(ljudovi, nameof(SelectListItem.Value), nameof(SelectListItem.Text));


    }
    
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PrepareDropDownLists();
        return View();
    }
        
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EvidencijaRada rad)
    {
        EvidencijaRada fake = new EvidencijaRada();
        var rezultatValidacije = EvidencijaRadaValidator.Validate(rad);

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
                
                int idVrstaPosla = context.VrstePosla
                    .Where(z => z.Naziv == rad.IdVrstePoslaNavigation.Naziv)
                    .Select(z => z.IdVrstePosla)
                    .FirstOrDefault();
                
                int idZad = context.Zadatci
                    .Where(z => z.ImeZadatka == rad.IdZadatkaNavigation.ImeZadatka)
                    .Select(z => z.IdZadatka)
                    .FirstOrDefault();
                
                
                string[] dijelovi = rad.OibosobaNavigation.Ime.Split(' ');

                int oibic = context.Osobe
                    .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                    .Select(z => z.Oibosoba)
                    .FirstOrDefault();

                //Zadatak zadatakPravi = new Zadatak(zadatakDTO.ImeZadatka, zadatakDTO.PlaniraniPocetak, zadatakDTO.PlaniraniZavrsetak, zadatakDTO.StvarniPocetak, zadatakDTO.StvarniZavrsetak, idZahtjeva, idStatusa, oibic, idPrioriteta);

                EvidencijaRada nova = new EvidencijaRada(rad.Opis, rad.DatumRada, rad.VrijemeRada, oibic, idZad, idVrstaPosla);

                

                
                 context.Add(nova);
                await context.SaveChangesAsync();
                logger.LogInformation(new EventId(1000), $"Evidencija rada {nova.Opis} dodana.");

                TempData[Constants.Message] = $"Evidencija rada {nova.IdEvidencijaRad} dodana.";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Evidencija rada {nova.Opis} dodana.";

                
                return RedirectToAction(nameof(Index));

            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dodavanje novog rada: {0}", exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                await PrepareDropDownLists();
                return View(fake);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return View(fake);
        }
    }
    
    
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      
        
        var rad = await context.EvidencijaRada                            
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
        
        if (rad != null)
        {        
            await PrepareDropDownLists();
            //var routeValues = new { rad, page = page, sort = sort, ascending = ascending };
            //return RedirectToAction(nameof(Edit), routeValues);
            

            //return RedirectToAction(nameof(Edit), rad, page, sort, ascending);
            return View(rad);
        }
        else
        {
            return NotFound($"Neispravan ID rada: {id}");
        }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EvidencijaRada rad)
    {
        if (rad == null)
        {
            
            return NotFound("Invalid input: rad je null");
        }
        
        
        var rezultatValidacije = EvidencijaRadaValidator.Validate(rad);

        if (!rezultatValidacije.IsValid)
        {
            foreach (var error in rezultatValidacije.Errors)
            {
                Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
            }

        }
        
        
        
        bool checkId = await context.EvidencijaRada.AnyAsync(m => m.IdEvidencijaRad == rad.IdEvidencijaRad);
        if (!checkId)
        {
            return NotFound($"Neispravan ID zadatka: {rad?.IdEvidencijaRad}");
        }
        
        if (ModelState.IsValid)
        {
            int idZad = context.Zadatci
                .Where(z => z.ImeZadatka == rad.IdZadatkaNavigation.ImeZadatka)
                .Select(z => z.IdZadatka)
                .FirstOrDefault();
                
            int idVrsta = context.VrstePosla
                .Where(z => z.Naziv == rad.IdVrstePoslaNavigation.Naziv)
                .Select(z => z.IdVrstePosla)
                .FirstOrDefault();
        
            string[] dijelovi = rad.OibosobaNavigation.Ime.Split(' ');

            int oibic = context.Osobe
                .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                .Select(z => z.Oibosoba)
                .FirstOrDefault();
            try
            {

                EvidencijaRada evirad = new EvidencijaRada(rad.IdEvidencijaRad, rad.Opis, rad.DatumRada,
                    rad.VrijemeRada, oibic, idZad, idVrsta);
                    
                context.Update(evirad);
                await context.SaveChangesAsync();
                TempData[Constants.Message] = "Rad ažurirana.";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Evidencija rada {evirad.Opis} dodana.";

/*

                int page = 0;
                var urlPath = HttpContext.Request.Path;
                var pageSegmentIndex = urlPath.Value.IndexOf("page=", StringComparison.OrdinalIgnoreCase);
                if (pageSegmentIndex != -1)
                {
                    // Izdvoji dio staze koji počinje od segmenta "page="
                    var pageSegment = urlPath.Value.Substring(pageSegmentIndex);

                    // Razdvoji segment kako biste dobili vrijednost "page"
                    var pageValue = pageSegment.Split('&')[0].Split('=')[1];

                    // Sada imate vrijednost "page" u varijabli "page"
                    page = Convert.ToInt32(pageValue);
                }

                */
                //return RedirectToAction(nameof(Index), routeValues);

                //return RedirectToAction(nameof(Index));
                //return RedirectToAction(nameof(Index), new { page = page});
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                await PrepareDropDownLists();
                return View(rad);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return View(rad);
        }
    }
    
}