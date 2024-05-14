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
    public class StatusController : Controller
    {
        private readonly DBContext ctx;
        private readonly ILogger<StatusController> logger;
        private readonly AppSettings appSettings;
        private StatusValidator StatusValidator = new StatusValidator();

        public StatusController(DBContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<StatusController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Statusi
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan status");
                TempData[Constants.Message] = "Ne postoji niti jedan status.";
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

            var statusi = query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();

            var model = new StatusViewModel
            {
                Statusi = statusi,
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
        public async Task<IActionResult> Create(Status status)
        {
            Status fake = new Status();

            var rezultatValidacije = StatusValidator.Validate(status);

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

                    Status novi = new Status(status.Naziv);




                    ctx.Add(novi);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Status {novi.IdStatus} dodan.";
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
                var status = await ctx.Statusi.FindAsync(id);

                if (status == null)
                {
                    TempData[Constants.Message] = "Status nije pronađen.";
                    TempData[Constants.ErrorOccurred] = true;
                }
                else
                {
                    ctx.Statusi.Remove(status);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Status {id} obrisan.";
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
            var status = await ctx.Statusi
                .Where(m => m.IdStatus == id)
                .Select(m => new Status()
                {
                    IdStatus = m.IdStatus,
                    Naziv = m.Naziv,
                })
                .SingleOrDefaultAsync();


            if (status != null)
            {
                return PartialView(status);
            }
            else
            {
                return NotFound($"Neispravan ID statusa: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Status status)
        {
            if (status == null)
            {

                return BadRequest("Invalid input: status je null");
            }


            var rezultatValidacije = StatusValidator.Validate(status);

            if (!rezultatValidacije.IsValid)
            {
                foreach (var error in rezultatValidacije.Errors)
                {
                    Console.WriteLine($"Property: {error.PropertyName}, Error: {error.ErrorMessage}");
                }

            }



            bool checkId = await ctx.Statusi.AnyAsync(m => m.IdStatus == status.IdStatus);
            if (!checkId)
            {
                return NotFound($"Neispravan ID zahtjeva: {status?.IdStatus}");
            }

            if (ModelState.IsValid)
            {
                try
                {

                    Status status1 = new Status(status.IdStatus, status.Naziv);
                    ctx.Update(status1);
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    return PartialView(status);
                }
            }
            else
            {
                return PartialView(status);
            }
        }
    }
}
