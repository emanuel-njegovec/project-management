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
using RPPP_WebApp.ModelValidation;
using RPPP_WebApp.Views.Zadatak;

namespace RPPP_WebApp.Controllers
{
    public class ZahtjevController : Controller
    {
        private readonly DBContext ctx;
        private readonly ILogger<ZahtjevController> logger;
        private readonly AppSettings appSettings;
        private ZahtjevValidator ZahtjevValidator = new ZahtjevValidator();

        public ZahtjevController(DBContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZahtjevController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Zahtjevi.Include(d => d.IdVrsteZahtjevaNavigation).Include(d => d.IdPrioritetaNavigation).Include(d => d.IdProjektaNavigation)
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan zahtjev");
                TempData[Constants.Message] = "Ne postoji niti jedan zahtjev.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var zahtjevi = query
                           .Skip((page - 1) * pagesize)
                           .Take(pagesize)
                           .ToList();

            var model = new ZahtjevViewModel
            {
                Zahtjevi = zahtjevi,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Zahtjev zahtjev)
        {
            Zahtjev fake = new Zahtjev();

            var rezultatValidacije = ZahtjevValidator.Validate(zahtjev);

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

                    int idVrstaZahtjeva = ctx.VrsteZahtjeva
                        .Where(z => z.Naziv == zahtjev.IdVrsteZahtjevaNavigation.Naziv)
                        .Select(z => z.IdVrsteZahtjeva)
                        .FirstOrDefault();

                    int idPrioriteta = ctx.Prioriteti
                        .Where(z => z.Naziv == zahtjev.IdPrioritetaNavigation.Naziv)
                        .Select(z => z.IdPrioriteta)
                        .FirstOrDefault();

                    int idProjekta = ctx.Projekti
                        .Where(z => z.Naziv == zahtjev.IdProjektaNavigation.Naziv)
                        .Select(z => z.IdProjekta)
                        .FirstOrDefault();


                    Zahtjev novi = new Zahtjev(zahtjev.Naslov, zahtjev.Opis, idVrstaZahtjeva, idPrioriteta, idProjekta);




                    ctx.Add(novi);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Zahtjev {novi.IdZahtjeva} dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
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


        private async Task PrepareDropDownLists()
        {
            ViewBag.Vrste = new SelectList(await ctx.VrsteZahtjeva
                                  .OrderBy(v => v.IdVrsteZahtjeva)
                                  .Select(v => new SelectListItem
                                  {
                                      Value = v.Naziv,
                                      Text = v.Naziv
                                  })
                                  .ToListAsync(), "Value", "Text");


            ViewBag.Prioriteti = new SelectList(await ctx.Prioriteti
                                  .OrderBy(v => v.IdPrioriteta)
                                  .Select(v => new SelectListItem
                                  {
                                      Value = v.Naziv,
                                      Text = v.Naziv
                                  })
                                  .ToListAsync(), "Value", "Text");

            ViewBag.Projekti = new SelectList(await ctx.Projekti
                                  .OrderBy(v => v.IdProjekta)
                                  .Select(v => new SelectListItem
                                  {
                                      Value = v.Naziv,
                                      Text = v.Naziv
                                  })
                                  .ToListAsync(), "Value", "Text");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                var zahtjev = await ctx.Zahtjevi.FindAsync(id);

                if (zahtjev == null)
                {
                    TempData[Constants.Message] = "Zahtjev nije pronađen.";
                    TempData[Constants.ErrorOccurred] = true;
                }
                else
                {
                    ctx.Zahtjevi.Remove(zahtjev);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Zahtjev {id} obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "Error deleting rada");
                TempData[Constants.Message] = "Greška prilikom brisanja rada";
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });


        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var zahtjev = await ctx.Zahtjevi
                .Where(m => m.IdZahtjeva == id)
                .Select(m => new Zahtjev()
                {
                    IdZahtjeva = m.IdZahtjeva,
                    Naslov = m.Naslov,
                    Opis = m.Opis,
                    IdVrsteZahtjeva = m.IdVrsteZahtjeva,
                    IdPrioriteta = m.IdPrioriteta,
                    IdProjekta = m.IdProjekta,

                })
                .SingleOrDefaultAsync();


            if (zahtjev != null)
            {
                await PrepareDropDownLists();
                return PartialView(zahtjev);
            }
            else
            {
                return NotFound($"Neispravan ID zahtjeva: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Zahtjev zahtjev)
        {
            if (zahtjev == null)
            {

                return BadRequest("Invalid input: zahtjev je null");
            }


            var rezultatValidacije = ZahtjevValidator.Validate(zahtjev);

            if (!rezultatValidacije.IsValid)
            {
                foreach (var error in rezultatValidacije.Errors)
                {
                    Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                }

            }



            bool checkId = await ctx.Zahtjevi.AnyAsync(m => m.IdZahtjeva == zahtjev.IdZahtjeva);
            if (!checkId)
            {
                return NotFound($"Neispravan ID zahtjeva: {zahtjev?.IdZahtjeva}");
            }

            if (ModelState.IsValid)
            {
                int idVrstaZahtjeva = ctx.VrsteZahtjeva
                       .Where(z => z.Naziv == zahtjev.IdVrsteZahtjevaNavigation.Naziv)
                       .Select(z => z.IdVrsteZahtjeva)
                       .FirstOrDefault();

                int idPrioriteta = ctx.Prioriteti
                    .Where(z => z.Naziv == zahtjev.IdPrioritetaNavigation.Naziv)
                    .Select(z => z.IdPrioriteta)
                    .FirstOrDefault();

                int idProjekta = ctx.Projekti
                    .Where(z => z.Naziv == zahtjev.IdProjektaNavigation.Naziv)
                    .Select(z => z.IdProjekta)
                    .FirstOrDefault();
                try
                {

                    Zahtjev zahtjev1 = new Zahtjev(zahtjev.IdZahtjeva, zahtjev.Naslov, zahtjev.Opis, idVrstaZahtjeva, idPrioriteta, idProjekta);

                    ctx.Update(zahtjev1);
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    await PrepareDropDownLists();
                    return PartialView(zahtjev);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return PartialView(zahtjev);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Show(int id)
        {

            var query = ctx.Zadatci
              .Include(z => z.IdZahtjevaNavigation)
              .Include(z => z.IdStatusNavigation)
              .Include(z => z.OibosobaNavigation)
              .Include(z => z.IdPrioritetaNavigation)
              .Include(z => z.EvidencijaRada)
              .AsNoTracking();

            int count = ctx.Zadatci.Select(q => q.IdZahtjeva == id).Count();

            var zahtjevi = query
                .Where(q => q.IdZahtjeva == id)
                .Take(count)
                .ToList();

            var model = new ZadatakViewModel()
            {
                Zadatci = zahtjevi
            };

            var zad = await ctx.Zadatci
                .Where(m => m.IdZadatka == id)
                .Select(m => new Zadatak()
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

            return View(model);
        }
    }
}
