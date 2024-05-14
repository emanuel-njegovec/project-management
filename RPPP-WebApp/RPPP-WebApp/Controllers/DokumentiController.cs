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
using RPPP_WebApp.Views.Projekt;
namespace RPPP_WebApp.Controllers;

public class DokumentiController : Controller
{
    private readonly DBContext ctx;
    private readonly AppSettings appSettings;
    private readonly ILogger<DokumentiController> logger;

    private DokumentiValidator _dokumentiValidator = new DokumentiValidator();

    public DokumentiController(DBContext ctx, IOptionsSnapshot<AppSettings> options,
        ILogger<DokumentiController> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
        appSettings = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pageSize = appSettings.PageSize;

        var query = ctx.Dokumenti
            .Include(d => d.IdProjektaNavigation)
            .Include(d => d.IdVrsteDokumentaNavigation)
            .AsNoTracking();

        if (query.Count() == 0)
        {
            logger.LogInformation("Ne postoje dokumenti");
            TempData[Constants.Message] = "Ne postoje dokumenti";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Create));
        }

        var dokumenti = query.OrderBy(d => d.IdDokumenta).ToList();

        var pagingInfo = new PagingInfo
        {
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pageSize,
            TotalItems = query.Count()
        };

        if (page < 1 || page > pagingInfo.TotalPages)
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });

        query = query.ApplySort(sort, ascending);

        var svi = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var model = new DokumentiViewModel()
        {
            Dokumenti = svi,
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
            var dok = await ctx.Dokumenti.FindAsync(id);

            if (dok == null)
            {
                TempData[Constants.Message] = "Dokument nije pronaden";
                TempData[Constants.ErrorOccurred] = true;
            }
            else
            {
                ctx.Dokumenti.Remove(dok);
                await ctx.SaveChangesAsync();
                logger.LogInformation($"Dokument {dok.IdDokumenta} uspjesno obrisan");
                TempData[Constants.Message] = $"Dokument {dok.IdDokumenta} je obrisan";
                TempData["SuccessMessage"] = $"Dokument {dok.IdDokumenta} uspjesno obrisan";
                TempData[Constants.ErrorOccurred] = false;
            }
        }
        catch (Exception exc)
        {
            TempData[Constants.Message] = "Greska pri brisanju dokumenta";
            TempData[Constants.ErrorOccurred] = true;
            logger.LogError(exc, "Greska pri brisanju");
        }

        return RedirectToAction(nameof(Index), new { page , sort, ascending });
    }

    private async Task PrepareDropDownLists()
    {
        var sviProjekti = await ctx.Projekti
            .OrderBy(d => d.IdProjekta)
            .Select(d => new SelectListItem
            {
                Value = d.Naziv,
                Text = $"{d.Naziv}"
            })
            .ToListAsync();

        var sveVrste = await ctx.VrsteDokumenta
            .OrderBy(d => d.IdVrsteDokumenta)
            .Select(d => new SelectListItem
            {
                Value = d.Naziv,
                Text = $"{d.Naziv}"
            })
            .ToListAsync();

        ViewBag.Projekti = new SelectList(sviProjekti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        ViewBag.VrsteDokumenata = new SelectList(sveVrste, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PrepareDropDownLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Dokument dok)
    {
        Dokument fk = new Dokument();

        var val = _dokumentiValidator.Validate(dok);

        if (!val.IsValid)
        {
            foreach (var er in val.Errors)
            {
                Console.WriteLine($"Property: {er.PropertyName}, Error: {er.ErrorMessage}");
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                int idProj = ctx.Projekti
                    .Where(d => d.Naziv == dok.IdProjektaNavigation.Naziv)
                    .Select(d => d.IdProjekta)
                    .FirstOrDefault();

                int idVrsD = ctx.VrsteDokumenta
                    .Where(d => d.Naziv == dok.IdVrsteDokumentaNavigation.Naziv)
                    .Select(d => d.IdVrsteDokumenta)
                    .FirstOrDefault();

                Dokument nov = new Dokument(dok.DatumNastanka, dok.Stavka, dok.Format, idVrsD, idProj);

                ctx.Add(nov);
                await ctx.SaveChangesAsync();
                logger.LogInformation(new EventId(1000), $"Dokument {nov.IdDokumenta} dodan");

                TempData[Constants.Message] = $"Dokument {nov.IdDokumenta} dodan";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Dokument {nov.IdDokumenta} dodan";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogInformation("Pogreska pri dodavanju novog dokumenta: {0}", exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                await PrepareDropDownLists();
                return View(fk);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return View(fk);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dok = await ctx.Dokumenti
            .Where(d => d.IdDokumenta == id)
            .Select(d => new Dokument()
            {
                IdDokumenta = id,
                DatumNastanka = d.DatumNastanka,
                Format = d.Format,
                IdProjekta = d.IdProjekta,
                IdVrsteDokumenta = d.IdVrsteDokumenta,
                Stavka = d.Stavka
            })
            .SingleOrDefaultAsync();

        if (dok != null)
        {
            await PrepareDropDownLists();
            return PartialView(dok);
        }
        else
        {
            return NotFound("Neispravan id");
        }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Dokument dok)
    {
        if (dok == null) return NotFound("Invalid input");

        var val = _dokumentiValidator.Validate(dok);

        if (!val.IsValid)
        {
            foreach (var er in val.Errors)
            {
                Console.WriteLine($"Property: {er.PropertyName}, Error: {er.ErrorMessage}");
            }
        }

        bool cId = await ctx.Dokumenti.AnyAsync(d => d.IdDokumenta == dok.IdDokumenta);

        if (!cId) return NotFound($"Neispravan id {dok.IdDokumenta}");

        if (ModelState.IsValid)
        {
            int idProj = ctx.Projekti
                .Where(d => d.Naziv == dok.IdProjektaNavigation.Naziv)
                .Select(d => d.IdProjekta)
                .FirstOrDefault();

            int idVrsD = ctx.VrsteDokumenta
                .Where(d => d.Naziv == dok.IdVrsteDokumentaNavigation.Naziv)
                .Select(d => d.IdVrsteDokumenta)
                .FirstOrDefault();
            try
            {
                Dokument doki = new Dokument(dok.IdDokumenta, dok.DatumNastanka, dok.Stavka, dok.Format, idVrsD,
                    idProj);
                ctx.Update(doki);
                await ctx.SaveChangesAsync();
                TempData[Constants.Message] = "Dokument azuriran";
                TempData[Constants.ErrorOccurred] = false;
                TempData["SuccessMessage"] = $"Dokument {doki.IdDokumenta} dodan";
                return RedirectToAction(nameof(Index));

            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                await PrepareDropDownLists();
                return PartialView(dok);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return PartialView(dok);
        }
    }
}

