using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelValidation;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class VrstaZahtjevaController : Controller
    {
        private readonly DBContext ctx;
        private readonly ILogger<VrstaZahtjevaController> logger;
        private readonly AppSettings appSettings;
        private VrstaZahtjevaValidator VrstaZahtjevaValidator = new VrstaZahtjevaValidator();

        public VrstaZahtjevaController(DBContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaZahtjevaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.VrsteZahtjeva
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedna vrsta");
                TempData[Constants.Message] = "Ne postoji niti jedna vrsta.";
                TempData[Constants.ErrorOccurred] = false;
                //return RedirectToAction(nameof(Create));
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

            var vrste = query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();

            var model = new VrstaZahtjevaViewModel
            {
                Vrste = vrste,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VrstaZahtjeva vrsta)
        {
            VrstaZahtjeva fake = new VrstaZahtjeva();

            var rezultatValidacije = VrstaZahtjevaValidator.Validate(vrsta);

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

                    VrstaZahtjeva novi = new VrstaZahtjeva(vrsta.Naziv, vrsta.Opis);




                    ctx.Add(novi);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"VrstaZahtjeva {novi.IdVrsteZahtjeva} dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    return View(fake);
                }
            }
            else
            {
                return View(fake);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                var vrsta = await ctx.VrsteZahtjeva.FindAsync(id);

                if (vrsta == null)
                {
                    TempData[Constants.Message] = "VrstaZahtjeva nije pronađen.";
                    TempData[Constants.ErrorOccurred] = true;
                }
                else
                {
                    ctx.VrsteZahtjeva.Remove(vrsta);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"VrstaZahtjeva {id} obrisan.";
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
            var vrsta = await ctx.VrsteZahtjeva
                .Where(m => m.IdVrsteZahtjeva == id)
                .Select(m => new VrstaZahtjeva()
                {
                    IdVrsteZahtjeva = m.IdVrsteZahtjeva,
                    Naziv = m.Naziv
                })
                .SingleOrDefaultAsync();


            if (vrsta != null)
            {
                return PartialView(vrsta);
            }
            else
            {
                return NotFound($"Neispravan ID zahtjeva: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VrstaZahtjeva vrsta)
        {
            if (vrsta == null)
            {

                return BadRequest("Invalid input: vrsta je null");
            }


            var rezultatValidacije = VrstaZahtjevaValidator.Validate(vrsta);

            if (!rezultatValidacije.IsValid)
            {
                foreach (var error in rezultatValidacije.Errors)
                {
                    Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                }

            }



            bool checkId = await ctx.VrsteZahtjeva.AnyAsync(m => m.IdVrsteZahtjeva == vrsta.IdVrsteZahtjeva);
            if (!checkId)
            {
                return NotFound($"Neispravan ID zahtjeva: {vrsta?.IdVrsteZahtjeva}");
            }

            if (ModelState.IsValid)
            {
                try
                {

                    VrstaZahtjeva vrsta1 = new VrstaZahtjeva(vrsta.IdVrsteZahtjeva, vrsta.Naziv, vrsta.Opis);

                    ctx.Update(vrsta1);
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    return PartialView(vrsta);
                }
            }
            else
            {
                return PartialView(vrsta);
            }
        }
    }
}
