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

using RPPP_WebApp.Views.Projekt;
using RPPP_WebApp.ModelsValidation;

namespace RPPP_WebApp.Controllers;

public class ProjektController : Controller
{
    private readonly DBContext ctx;

    private readonly AppSettings appSettings;

    private readonly ILogger<ProjektController> logger;

    ProjektValidator ProjektValidator = new ProjektValidator();
    ProjektDTOValidator ProjektDTOValidator = new ProjektDTOValidator();
    ProjektDTOViewModelValidator ProjektDTOViewModelValidator = new ProjektDTOViewModelValidator();

    public ProjektController(DBContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjektController> logger){
        this.ctx = ctx;
        this.logger = logger;
        this.appSettings = options.Value;
    }
        
    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appSettings.PageSize;

        var query = ctx.Projekti
                    .Include(p => p.IdVrsteProjektaNavigation)
                    .Include(p => p.OibnaruciteljNavigation)
                    .AsNoTracking();

        if(query.Count() == 0){
            logger.LogInformation("Ne postoje projekti");
            TempData[Constants.Message] = "Ne postoje projekti";
            TempData[Constants.ErrorOccurred] = false;
        }

        var projekt = query
                      .OrderBy(p => p.IdProjekta)
                      .ToList();


        var pagingInfo = new PagingInfo{
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pagesize,
            TotalItems = query.Count()
        };

        if(page < 1 || page > pagingInfo.TotalPages){
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }

        query = query.ApplySort(sort, ascending);

        var projektici = query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();       
 
        var model = new ProjektViewModel
        {
            Projekt = projektici,
            PagingInfo = pagingInfo
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(){
        await PrepareDropDownLists();
        return View();
    }

    private async Task PrepareDropDownLists(){
        var PrvaVrsta = await ctx.Projekti
        .Select(p => new{ p.IdVrsteProjektaNavigation.Naziv })
        .Distinct()
        .FirstOrDefaultAsync();

        var SveVrste = await ctx.Projekti
        .OrderBy(p => p.IdVrsteProjektaNavigation.Naziv)
        .Select(p => new { p.IdVrsteProjektaNavigation.Naziv })
        .Distinct()
        .ToListAsync();

        var PrviNarucitelj = await ctx.Projekti
        .Select(p => new{ p.OibnaruciteljNavigation.Naziv })
        .Distinct()
        .FirstOrDefaultAsync();

        var SviNarucitelji = await ctx.Projekti
        .OrderBy(p => p.OibnaruciteljNavigation.Naziv)
        .Select(p => new { p.OibnaruciteljNavigation.Naziv })
        .Distinct()
        .ToListAsync();

        var vr = await ctx.VrsteProjekta
        .OrderBy(p => p.IdVrsteProjekta)
        .Select(p => new SelectListItem{
            Value = p.Naziv, 
            Text = $"{p.Naziv}"
        })
        .ToListAsync();

        var nar = await ctx.Narucitelji
        .OrderBy(p => p.Oibnarucitelj)
        .Select(p => new SelectListItem{
            Value = p.Naziv, 
            Text = $"{p.Naziv}"
        }).ToListAsync();

        ViewBag.VrstaProjekta = new SelectList(vr, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        ViewBag.Narucitelj = new SelectList(nar, nameof(SelectListItem.Value), nameof(SelectListItem.Text));

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjektDTO projektdto){
        Projekt projektPF = new Projekt();
        ProjektDTO projektdtoPF = new ProjektDTO();

        var val = ProjektDTOValidator.Validate(projektdto);

        if(!val.IsValid){
            foreach(var error in val.Errors){
                Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
            }
        }

        if(ModelState.IsValid){
            try{
                int idVrsteProjekta = ctx.VrsteProjekta
                    .Where(p => p.Naziv == projektdto.VrstaProjekta)
                    .Select(p => p.IdVrsteProjekta)
                    .FirstOrDefault();
                int idNarucitelj = ctx.Narucitelji
                    .Where(p => p.Oibnarucitelj == projektdto.Oibnarucitelj)
                    .Select(p => p.Oibnarucitelj)
                    .FirstOrDefault();
                Projekt prP = new Projekt(projektdto.Naziv, projektdto.Opis, projektdto.DatumPocetka, projektdto.DatumZavrsetka, idNarucitelj, idVrsteProjekta);
                ctx.Add(prP);
                await ctx.SaveChangesAsync();
                TempData[Constants.Message] = $"Projekt {prP.IdProjekta} dodan.";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Projekt {prP.IdProjekta} dodan";
                return RedirectToAction(nameof(Index));
            }catch(Exception exc){
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                TempData["errorMessage"] = $"Neuspijelo dodavanje projekta, provjerite unesene podatke";
                await PrepareDropDownLists();
                return View(projektdtoPF);
            }
        }else{
            await PrepareDropDownLists();
            return View(projektdtoPF);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true){
        try{
            var projekt = await ctx.Projekti.FindAsync(id);

            if(projekt == null){
                TempData[Constants.Message] = "Projekt ne postoji";
                TempData["errorMessage"] = "Projekt ne postoji";
                TempData[Constants.ErrorOccurred] = true;
            }else{
                ctx.Projekti.Remove(projekt);
                await ctx.SaveChangesAsync();
                TempData[Constants.Message] = $"Projekt {id} obrisan";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Projekt {projekt.Naziv} obrisan";
            }
        }catch(Exception exc){
            logger.LogError(exc, "Error brisanja projekt");
            TempData[Constants.Message] = "Greška prilikom brisanja projekta";
            TempData[Constants.ErrorOccurred] = true;
            TempData["errorMessage"] = "Greska pri brisanju projekta";
        }

        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProjektDTOViewModel prDto){

        if(prDto == null) return BadRequest("Nema poslanih podataka");

        var val = ProjektDTOViewModelValidator.Validate(prDto);

        if(!val.IsValid){
            foreach(var error in val.Errors){
                Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
            }
        }


        bool checkId = await ctx.Projekti.AnyAsync(p => p.IdProjekta == prDto.IdProjekta);

        if(!checkId) return NotFound($"Neispravan Id projekta: {prDto?.IdProjekta}");

        if(ModelState.IsValid){
            int idVrste = ctx.VrsteProjekta
                .Where(p => p.Naziv == prDto.IdVrsteProjektaNavigation.Naziv)
                .Select(p => p.IdVrsteProjekta)
                .FirstOrDefault();
            int idNarucitelja = ctx.Narucitelji
                .Where(p => p.Naziv == prDto.OibnaruciteljNavigation.Naziv)
                .Select(p => p.Oibnarucitelj)
                .FirstOrDefault();

            try{
                Projekt pp = new Projekt(prDto.IdProjekta, prDto.Naziv, prDto.Opis, prDto.DatumPocetka, prDto.DatumZavrsetka, idNarucitelja, idVrste);
                Projekt pd = new Projekt(prDto.Naziv, prDto.Opis, prDto.DatumPocetka, prDto.DatumZavrsetka, idNarucitelja, idVrste);
                ctx.Update(pp);
                await ctx.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Projekt {pp.Naziv} obrisan";
                return RedirectToAction(nameof(Index));
            }catch (Exception exc){
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                await PrepareDropDownLists();
                return PartialView(prDto);
            }
        }
        else{
            await PrepareDropDownLists();
            return PartialView(prDto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id){
            
        var pr = await ctx.Projekti
            .Where(p => p.IdProjekta == id)
            .Select(p => new ProjektDTOViewModel(){
                IdProjekta = p.IdProjekta,
                Naziv = p.Naziv,
                Opis = p.Opis,
                DatumPocetka = p.DatumPocetka,
                DatumZavrsetka = p.DatumZavrsetka,
                Oibnarucitelj = p.Oibnarucitelj,
                IdVrsteProjekta = p.IdVrsteProjekta,
            })
            .SingleOrDefaultAsync();

        ProjektDTO pp = new ProjektDTO();

        if(pr != null){
            await PrepareDropDownLists();
            return PartialView(pr);
        }else{
            return NotFound($"Neispravan ID projekta: {id}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Show(int id)
    {
        var query = ctx.Dokumenti
            .Include(d => d.IdVrsteDokumentaNavigation)
            .Include(d => d.IdVrsteDokumenta)
            .AsNoTracking();

        int br = ctx.Dokumenti.Select(q => q.IdProjekta == id).Count();

        var svi = query
            .Where(q => q.IdProjekta == id)
            .Take(br)
            .ToList();

        var model = new DokumentiViewModel()
        {
            Dokumenti = svi
        };

        var doki = await ctx.Dokumenti
            .Where(d => d.IdVrsteDokumenta == id)
            .Select(d => new Dokument()
            {
                DatumNastanka = d.DatumNastanka,
                Format = d.Format,
                IdDokumenta = d.IdDokumenta,
                IdProjekta = d.IdProjekta,
                IdVrsteDokumenta = d.IdVrsteDokumenta,
                Stavka = d.Stavka
            })
            .SingleOrDefaultAsync(); 

        return View(model);
    }
}
