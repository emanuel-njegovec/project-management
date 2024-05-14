using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
    public class ProjektnaKarticaMDv2Controller : Controller
    {
        private readonly DBContext ctx;
        private readonly ILogger<ProjektnaKarticaMDv2Controller> logger;
        private readonly AppSettings appSettings;

        public ProjektnaKarticaMDv2Controller(DBContext context, IOptionsSnapshot<AppSettings> appSettings, ILogger<ProjektnaKarticaMDv2Controller> logger)
        {
            this.ctx = context;
            this.logger = logger;
            this.appSettings = appSettings.Value;
        }

        public async Task<IActionResult> Index(string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.ProjektneKartice.AsQueryable();

            #region Apply filter
            //DokumentFilter df = DokumentFilter.FromString(filter);
            //if (!df.IsEmpty())
            //{
              //  if (df.IdPartnera.HasValue)
                //{
                  //  df.NazPartnera = await ctx.vw_Partner
                    //                          .Where(p => p.IdPartnera == df.IdPartnera)
                      //                        .Select(vp => vp.Naziv)
                        //                      .FirstOrDefaultAsync();
                //}
                //query = df.Apply(query);
            //}
            #endregion

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (count > 0 && (page < 1 || page > pagingInfo.TotalPages))
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending, filter });
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

            //for (int i = 0; i < kartice.Count; i++)
            //{
            //    kartice[i].Position = (page - 1) * pagesize + i;
            //}
            var model = new ProjektneKarticeViewModel
            {
                ProjektneKartice = kartice,
                PagingInfo = pagingInfo,
                //Filter = df
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
                                        NazivOsobe = d.OibosobaNavigation.Ime + " " + d.OibosobaNavigation.Prezime
                                    })
                                    .FirstOrDefaultAsync();
            if (kartica == null)
            {
                return NotFound($"Dokument {id} ne postoji");
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
                    await SetPreviousAndNext(position.Value, filter, sort, ascending);
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;
                ViewBag.Position = position;

                return View(viewName, kartica);
            }
        }

        private async Task SetPreviousAndNext(int position, string filter, int sort, bool ascending)
        {
            var query = ctx.ProjektneKartice.AsQueryable();

            //DokumentFilter df = new DokumentFilter();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                //df = DokumentFilter.FromString(filter);
                //if (!df.IsEmpty())
                //{
                    //query = df.Apply(query);
                //}
            }

            query = query.ApplySort(sort, ascending);
            if (position > 0)
            {
                ViewBag.Previous = await query.Skip(position - 1).Select(d => d.IdKartice).FirstAsync();
            }
            if (position < await query.CountAsync() - 1)
            {
                ViewBag.Next = await query.Skip(position + 1).Select(d => d.IdKartice).FirstAsync();
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var kartica = new KarticaTransakcijeViewModel
            {
                Stanje = 0
            };
            await PrepareDropDownLists();
            return View(kartica);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KarticaTransakcijeViewModel model)
        {
            if (ModelState.IsValid)
            {
                ProjektnaKartica p = new ProjektnaKartica();
                p.IdKartice = model.IdKartice;
                p.Ibankartice = model.Ibankartice;
                p.Stanje = model.Stanje;
                foreach (var transakcija in model.Transakcije)
                {
                    Transakcija novaTransakcija = new Transakcija();
                    novaTransakcija.Iznos = transakcija.Iznos;
                    novaTransakcija.Iban2zaTransakciju = transakcija.Iban2zaTransakciju;
                    novaTransakcija.Datum = transakcija.Datum;
                    novaTransakcija.Vrijeme = transakcija.Vrijeme;
                    p.Transakcijas.Add(novaTransakcija);
                }

                //p.IznosDokumenta = (1 + d.PostoPorez) * d.Stavka.Sum(s => s.CijenaStavke);
                //eventualno umanji iznos za dodatni popust za kupca i sl... nešto što bi bilo poslovno pravilo
                try
                {
                    ctx.Add(p);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Kartica uspješno dodana. Id={p.IdKartice}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new { id = p.IdKartice });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(model);
            }
        }

        [HttpGet]
        public Task<IActionResult> Edit(int id, int? position, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            return Show(id, position, filter, page, sort, ascending, viewName: nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KarticaTransakcijeViewModel model, int? position, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Filter = filter;
            ViewBag.Position = position;
            if (ModelState.IsValid)
            {
                var kartica = await ctx.ProjektneKartice
                                        .Include(d => d.Transakcijas)
                                        .Where(d => d.IdKartice == model.IdKartice)
                                        .FirstOrDefaultAsync();
                if (kartica == null)
                {
                    return NotFound("Ne postoji kartica s id-om: " + model.IdKartice);
                }

                if (position.HasValue)
                {
                    page = 1 + position.Value / appSettings.PageSize;
                    await SetPreviousAndNext(position.Value, filter, sort, ascending);
                }

                kartica.IdKartice = model.IdKartice;
                kartica.Ibankartice = model.Ibankartice;
                kartica.Stanje = model.Stanje;

                List<int> idTransakcija = model.Transakcije
                                          .Select(s => s.IdTransakcije)
                                          .ToList();
                //izbaci sve koje su nisu više u modelu
                ctx.RemoveRange(kartica.Transakcijas.Where(s => !idTransakcija.Contains(s.IdTransakcije)));

                foreach (var transakcija in model.Transakcije)
                {
                    //ažuriraj postojeće i dodaj nove
                    Transakcija novaTransakcija; // potpuno nova ili dohvaćena ona koju treba izmijeniti
                    if (transakcija.IdTransakcije > 0)
                    {
                        novaTransakcija = kartica.Transakcijas.First(s => s.IdTransakcije == transakcija.IdTransakcije);
                    }
                    else
                    {
                        novaTransakcija = new Transakcija();
                        kartica.Transakcijas.Add(novaTransakcija);
                    }
                    novaTransakcija.Iznos = transakcija.Iznos;
                    novaTransakcija.Iban2zaTransakciju = transakcija.Iban2zaTransakciju;
                    novaTransakcija.Datum = transakcija.Datum;
                    novaTransakcija.Vrijeme = transakcija.Vrijeme;
                }

                //kartica.IznosDokumenta = (1 + dokument.PostoPorez) * model.Stavke.Sum(s => s.IznosArtikla);
                //eventualno umanji iznos za dodatni popust za kupca i sl... nešto što bi bilo poslovno pravilo
                try
                {

                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Dokument {kartica.IdKartice} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = kartica.IdKartice,
                        position,
                        filter,
                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int IdKartice, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            var kartica = await ctx.ProjektneKartice
                                    .Where(d => d.IdKartice == IdKartice)
                                    .SingleOrDefaultAsync();
            if (kartica != null)
            {
                try
                {
                    ctx.Remove(kartica);
                    await ctx.SaveChangesAsync();
                    TempData[Constants.Message] = $"Dokument {kartica.IdKartice} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja dokumenta: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
            }
            else
            {
                TempData[Constants.Message] = "Ne postoji dokument s id-om: " + IdKartice;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { filter, page, sort, ascending });
        }
    }


}
