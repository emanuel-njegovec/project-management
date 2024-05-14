using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;


namespace RPPP_WebApp.Controllers
{
    public class ProjektnaKarticaMDController : Controller
    {
        private readonly DBContext ctx;
        private readonly AppSettings appSettings;

        public ProjektnaKarticaMDController(DBContext ctx, IOptionsSnapshot<AppSettings> options)
        {
            this.ctx = ctx;
            appSettings = options.Value;

        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.ProjektneKartice.AsNoTracking();
            var query2 = ctx.Transakcije.AsNoTracking();

            int count = query.Count();

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

            var kartice = query
                          .Select(m => new ProjektnaKarticaViewModel
                          {
                              IdKartice = m.IdKartice,
                              Ibankartice = m.Ibankartice,
                              Stanje = m.Stanje,
                              NazivProjekta = m.IdProjektaNavigation.Naziv,
                              NazivOsobe = m.OibosobaNavigation.Ime + " " + m.OibosobaNavigation.Prezime
                          })
                          .Skip((page - 1) * pagesize)
                          .Take(pagesize)
                          .ToList();

            var model = new ProjektneKarticeViewModel
            {
                ProjektneKartice = kartice,
                PagingInfo = pagingInfo
            };
            return View(model);
        }

        public async Task<IActionResult> Show(int id, int? position, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {
            var kartica = await ctx.ProjektneKartice
                                    .Where(d => d.IdKartice == id)
                                    .Select(d => new KarticaTransakcijeViewModel
                                    {
                                        IdKartice = d.IdKartice,
                                        Ibankartice = d.Ibankartice,
                                        Stanje = d.Stanje,
                                        NazivProjekta = d.IdProjektaNavigation.Naziv,
                                        NazivOsobe = d.OibosobaNavigation.Prezime,
                                    })
                                    .FirstOrDefaultAsync();
            if (kartica == null)
            {
                return NotFound($"Kartica {id} ne postoji");
            }
            else
            { 
                //učitavanje stavki
                var transakcije = await ctx.Transakcije
                                      .Where(s => s.IdKartice == kartica.IdKartice)
                                      .OrderBy(s => s.IdTransakcije)
                                      .Select(s => new TransakcijaViewModel
                                      {
                                          IdTransakcije = s.IdTransakcije,
                                          Iznos = s.Iznos,
                                          Iban2zaTransakciju = s.Iban2zaTransakciju,
                                          Datum = s.Datum,
                                          Vrijeme = s.Vrijeme,
                                          IbanKartice = s.IdKarticeNavigation.Ibankartice,
                                          Opis = s.IdVrsteTransakcijeNavigation.Opis
                                      })
                                      .ToListAsync();
                kartica.Transakcije = transakcije;

                if (position.HasValue)
                {
                    page = 1 + position.Value / appSettings.PageSize;
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;
                ViewBag.Position = position;

                return View(viewName, kartica);
            }
        }


        private async Task PrepareDropDownLists()
        {
            var projekti = await ctx.Projekti
                                  .OrderBy(d => d.IdProjekta)
                                  .Select(d => new SelectListItem
                                  {
                                      Value = d.IdProjekta.ToString(),
                                      Text = d.Naziv
                                  })
                                  .ToListAsync();

            var osobe = await ctx.Osobe
                                  .OrderBy(d => d.Oibosoba)
                                  .Select(d => new SelectListItem
                                  {
                                      Value = d.Oibosoba.ToString(),
                                      Text = d.Ime + " " + d.Prezime
                                  })
                                  .ToListAsync();

            ViewBag.Projekti = new SelectList(projekti, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            ViewBag.Osobe = new SelectList(osobe, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        public IActionResult Create()
        {
            throw new NotImplementedException();
        }
    }
}