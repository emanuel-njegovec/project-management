using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers
{
    public class ZahtjevSTPController : Controller
    {
        private readonly DBContext ctx;
        private readonly ILogger<ZahtjevSTPController> logger;
        private readonly AppSettings appSettings;

        public ZahtjevSTPController(DBContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZahtjevSTPController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Zahtjevi.Include(d => d.IdVrsteZahtjevaNavigation).Include(d => d.IdPrioritetaNavigation).Include(d => d.IdProjektaNavigation).Include(d => d.Zadataks)
                           .AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan zahtjev");
                TempData[Constants.Message] = "Ne postoji niti jedan zahtjev.";
                TempData[Constants.ErrorOccurred] = false;
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

            var model = new ZahtjevSTPViewModel
            {
                Zahtjevii = zahtjevi,
                PagingInfo = pagingInfo
            };

            return View(model);
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
