using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.ModelValidation;

namespace RPPP_WebApp.Controllers
{
    public class PrioritetZahtjevaController : Controller
    {
        private readonly DBContext ctx;
        private readonly ILogger<PrioritetZahtjevaController> logger;
        private readonly AppSettings appSettings;
        private PrioritetValidator PrioritetValidator = new PrioritetValidator();

        public PrioritetZahtjevaController(DBContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<PrioritetZahtjevaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Prioriteti
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan prioritet");
                TempData[Constants.Message] = "Ne postoji niti jedan prioritet.";
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

            var prioriteti = query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();

            var model = new PrioritetZahtjevaViewModel
            {
                Prioriteti = prioriteti,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prioritet prioritet)
        {
            Prioritet fake = new Prioritet();

            var rezultatValidacije = PrioritetValidator.Validate(prioritet);

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
                   Prioritet novi = new Prioritet(prioritet.Naziv);




                    ctx.Add(novi);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Prioritet {novi.IdPrioriteta} dodan.";
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
                var prioritet = await ctx.Prioriteti.FindAsync(id);

                if (prioritet == null)
                {
                    TempData[Constants.Message] = "Prioritet nije pronađen.";
                    TempData[Constants.ErrorOccurred] = true;
                }
                else
                {
                    ctx.Prioriteti.Remove(prioritet);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Prioritet {id} obrisan.";
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
            var prioritet = await ctx.Prioriteti
                .Where(m => m.IdPrioriteta == id)
                .Select(m => new Prioritet()
                {
                    IdPrioriteta = m.IdPrioriteta,
                    Naziv = m.Naziv,

                })
                .SingleOrDefaultAsync();


            if (prioritet != null)
            {
                return PartialView(prioritet);
            }
            else
            {
                return NotFound($"Neispravan ID prioriteta: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Prioritet prioritet)
        {
            if (prioritet == null)
            {

                return BadRequest("Invalid input: prioritet je null");
            }


            var rezultatValidacije = PrioritetValidator.Validate(prioritet);

            if (!rezultatValidacije.IsValid)
            {
                foreach (var error in rezultatValidacije.Errors)
                {
                    Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                }

            }



            bool checkId = await ctx.Prioriteti.AnyAsync(m => m.IdPrioriteta == prioritet.IdPrioriteta);
            if (!checkId)
            {
                return NotFound($"Neispravan ID prioriteta: {prioritet?.IdPrioriteta}");
            }

            if (ModelState.IsValid)
            {
                
                try
                {

                    Prioritet prioritet1 = new Prioritet(prioritet.IdPrioriteta, prioritet.Naziv);

                    ctx.Update(prioritet1);
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    return PartialView(prioritet);
                }
            }
            else
            {
                return PartialView(prioritet);
            }
        }
    }
}
