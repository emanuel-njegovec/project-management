using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Views.Zadatak;

namespace RPPP_WebApp.Controllers
{
    public class ZahtjevMDController : Controller
    {
        private readonly DBContext ctx;
        private readonly AppSettings appSettings;

        public ZahtjevMDController(DBContext ctx, IOptionsSnapshot<AppSettings> options)
        {
            this.ctx = ctx;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Zahtjevi
                .Include(z => z.IdVrsteZahtjevaNavigation)
                .Include(z => z.IdPrioritetaNavigation)
                .Include(z => z.IdProjektaNavigation)
                .AsNoTracking();

            #region Apply filter
            ZahtjevFilter zf = ZahtjevFilter.FromString(filter);
            if (!zf.IsEmpty())
            {
                if (zf.IdZahtjeva.HasValue)
                {
                    zf.Naslov = await ctx.Zahtjevi
                        .Where(p => p.IdZahtjeva == zf.IdZahtjeva)
                        .Select(vp => vp.Naslov)
                        .FirstOrDefaultAsync();
                }
                query = zf.Apply(query);
            }
            #endregion

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

            var zahtjevi = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            var model = new ZahtjevSTPViewModel
            {
                Zahtjevii = zahtjevi,
                PagingInfo = pagingInfo,
                Filter = zf
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Filter(ZahtjevFilter filter)
        {
            return RedirectToAction(nameof(Index), new { filter = filter.ToString() });
        }

        [HttpGet]
        public Task<IActionResult> Edit(int id, int? position, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            return Show(id, position, filter, page, sort, ascending, viewName: nameof(Edit));
        }

        public async Task<IActionResult> Show(int id, int? position, string filter, int page = 1, int sort = 1,
        bool ascending = true, string viewName = nameof(Show))
        {



            List<Models.ZadatakDTO> zadataak = await ctx.Zadatci
                .Where(m => m.IdZahtjeva == id)
                .Select(m => new Models.ZadatakDTO()
                {
                    IdZadatka = m.IdZadatka,
                    ImeZadatka = m.ImeZadatka,
                    PlaniraniPocetak = m.PlaniraniPocetak,
                    PlaniraniZavrsetak = m.PlaniraniZavrsetak,
                    StvarniPocetak = m.StvarniPocetak,
                    StvarniZavrsetak = m.StvarniZavrsetak,
                    Zahtjev = m.IdZahtjevaNavigation.Naslov,
                    Status = m.IdStatusNavigation.Naziv,
                    Osoba = m.OibosobaNavigation.Ime + " " + m.OibosobaNavigation.Prezime,
                    Prioritet = m.IdPrioritetaNavigation.Naziv,

                })
                .ToListAsync();

            var zad = await ctx.Zahtjevi
                .Where(m => m.IdZahtjeva == id)
                .Select(m => new ZahtjevDTOViewModel()
                {
                    IdZahtjeva = m.IdZahtjeva,
                    Naslov = m.Naslov,
                    Opis = m.Opis,
                    IdVrsteZahtjeva = m.IdVrsteZahtjeva,
                    IdPrioriteta = m.IdPrioriteta,
                    IdProjekta = m.IdProjekta,

                })
                .FirstOrDefaultAsync();

            zad.ZadatciPravi = zadataak;

            if (zad == null)
            {
                return NotFound($"Zahtjev {id} ne postoji");
            }
            else
            {
                zad.Naslov = await ctx.Zahtjevi
                    .Where(p => p.IdZahtjeva == zad.IdZahtjeva)
                    .Select(p => p.Naslov)
                    .FirstOrDefaultAsync();



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

                return View(viewName, zad);


            }

        }

        private async Task SetPreviousAndNext(int position, string filter, int sort, bool ascending)
        {
            var query = ctx.Zahtjevi.AsQueryable();

            ZahtjevFilter zf = new ZahtjevFilter();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                zf = ZahtjevFilter.FromString(filter);
                if (!zf.IsEmpty())
                {
                    query = zf.Apply(query);
                }
            }

            query = query.ApplySort(sort, ascending);
            if (position > 0)
            {
                ViewBag.Previous = await query.Skip(position - 1).Select(d => d.IdZahtjeva).FirstAsync();
            }
            if (position < await query.CountAsync() - 1)
            {
                ViewBag.Next = await query.Skip(position + 1).Select(d => d.IdZahtjeva).FirstAsync();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] IFormCollection formData, int? position, string filter, int page = 1,
        int sort = 1, bool ascending = true)
        {
            int IdZahtjeva = 0;
            string Naslov = null;
            string Opis = null;
            int IdVrsteZahtjeva = 0;
            int IdPrioriteta = 0;
            int IdProjekta = 0;
            string vrstaZahtjeva = null;
            string prioritet = null;
            string projekt = null;

            List<ZadatakStavka> listaZadataka = new List<ZadatakStavka>();

            foreach (var kljuc in formData.Keys)
            {
                IdZahtjeva = int.Parse(formData["IdZahtjeva"]);
                Naslov = (formData["Naslov"]);
                Opis = (formData["Opis"]);
                IdVrsteZahtjeva = int.Parse(formData["IdVrsteZahtjeva"]);
                IdPrioriteta = int.Parse(formData["IdPrioriteta"]);
                IdProjekta = int.Parse(formData["IdProjekta"]);
                vrstaZahtjeva = (formData["IdVrsteZahtjevaNavigation.Naziv"]);
                prioritet = (formData["IdPrioritetaNavigation.Naziv"]);
                projekt = (formData["IdProjektaNavigation.Naziv"]);




                if (kljuc.StartsWith("Zadatak[") && kljuc.EndsWith("].IdStavke"))
                {
                    int idStavke = int.Parse(formData[kljuc]);

                    ZadatakStavka zadatak = new ZadatakStavka
                    {
                        IdStavke = idStavke,
                        ImeZadatka = formData[$"Zadatak[{idStavke}].ImeZadatka"],
                        PlaniraniPocetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].PlaniraniPocetak"]),
                        PlaniraniZavrsetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].PlaniraniZavrsetak"]),
                        StvarniPocetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].StvarniPocetak"]),
                        StvarniZavrsetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].StvarniZavrsetak"]),
                        Zahtjev = formData[$"Zadatak[{idStavke}].Zahtjev"],
                        Status = formData[$"Zadatak[{idStavke}].Status"],
                        Osoba = formData[$"Zadatak[{idStavke}].Osoba"],
                        Prioritet = formData[$"Zadatak[{idStavke}].Prioritet"]
                    };

                    listaZadataka.Add(zadatak);
                }
            }



            List<Zadatak> zadaciZahtjeva = ctx.Zadatci.Where(d => d.IdZahtjeva == IdZahtjeva).AsNoTracking().ToList();
            List<Zadatak> zadacii = new List<Zadatak>();
            List<Zadatak> listabrisanja = new List<Zadatak>();
            List<Zadatak> listaupdate = new List<Zadatak>();



            if (listaZadataka.Count == 0)
            {
                foreach (var var in zadaciZahtjeva)
                {
                    listabrisanja.Add(var);
                }
            }
            else
            {

                foreach (ZadatakStavka zad in listaZadataka)
                {
                    if (zad.IdStavke < 1000)
                    {
                        int idPrioriteta = ctx.Prioriteti
                                            .Where(z => z.Naziv == zad.Prioritet)
                                            .Select(z => z.IdPrioriteta)
                                            .FirstOrDefault();
                        int idStatusa = ctx.Statusi
                                        .Where(z => z.Naziv == zad.Status)
                                        .Select(z => z.IdStatus)
                                        .FirstOrDefault();
                        int idZahtjeva = ctx.Zahtjevi
                                        .Where(z => z.Naslov == zad.Zahtjev)
                                        .Select(z => z.IdPrioriteta)
                                        .FirstOrDefault();

                        string[] dijelovi = zad.Osoba.Split(' ');

                        int oib = ctx.Osobe
                            .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                            .Select(z => z.Oibosoba)
                            .FirstOrDefault();

                        Zadatak novi = new Zadatak(zad.ImeZadatka, zad.PlaniraniPocetak, zad.PlaniraniZavrsetak, zad.StvarniPocetak, zad.StvarniZavrsetak, idZahtjeva, idStatusa, oib, idPrioriteta);
                        listaupdate.Add(novi);
                    }
                    else
                    {
                        int idPrioriteta = ctx.Prioriteti
                                            .Where(z => z.Naziv == zad.Prioritet)
                                            .Select(z => z.IdPrioriteta)
                                            .FirstOrDefault();
                        int idStatusa = ctx.Statusi
                                        .Where(z => z.Naziv == zad.Status)
                                        .Select(z => z.IdStatus)
                                        .FirstOrDefault();
                        int idZahtjeva = ctx.Zahtjevi
                                        .Where(z => z.Naslov == zad.Zahtjev)
                                        .Select(z => z.IdPrioriteta)
                                        .FirstOrDefault();

                        string[] dijelovi = zad.Osoba.Split(' ');

                        int oib = ctx.Osobe
                            .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                            .Select(z => z.Oibosoba)
                            .FirstOrDefault();

                        Zadatak novii = new Zadatak(zad.ImeZadatka, zad.PlaniraniPocetak, zad.PlaniraniZavrsetak, zad.StvarniPocetak, zad.StvarniZavrsetak, idZahtjeva, idStatusa, oib, idPrioriteta);
                        zadacii.Add(novii);

                    }
                }
            }
            List<int> zadaciZahId = zadaciZahtjeva.Select(e => e.IdZadatka).ToList();
            List<int> radoviposaoID = zadacii.Select(e => e.IdZadatka).ToList();
            List<int> listabrisanjaID = listabrisanja.Select(e => e.IdZadatka).ToList();
            List<int> listaupdateID = listaupdate.Select(e => e.IdZadatka).ToList();

            foreach (int zad in zadaciZahId)
            {
                if (!radoviposaoID.Contains(zad) && !listaupdateID.Contains(zad))
                {
                    Zadatak zadDodaj = zadaciZahtjeva.FirstOrDefault(d => d.IdZadatka == zad);

                    listabrisanja.Add(zadDodaj);
                }
            }




            try
            {
                Zahtjev z = new Zahtjev(IdZahtjeva, Naslov, Opis, IdVrsteZahtjeva, IdPrioriteta, IdProjekta);
                ctx.Update(z);
                await ctx.SaveChangesAsync();

                foreach (var var in listabrisanja)
                {
                    ctx.Remove(var);
                }
                await ctx.SaveChangesAsync();

                foreach (var var in listaupdate)
                {
                    ctx.Update(var);
                }
                await ctx.SaveChangesAsync();

                foreach (var var in zadacii)
                {
                    ctx.Add(var);
                }
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                TempData["ErrorMessage"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] IFormCollection formData)
        {
            int IdZahtjeva = 0;
            string Naslov = null;
            string Opis = null;
            int IdVrsteZahtjeva = 0;
            int IdPrioriteta = 0;
            int IdProjekta = 0;
            string vrstaZahtjeva = null;
            string prioritet = null;
            string projekt = null;

            List<ZadatakStavka> listaZadataka = new List<ZadatakStavka>();

            foreach (var kljuc in formData.Keys)
            {
                IdZahtjeva = int.Parse(formData["IdZahtjeva"]);
                Naslov = (formData["Naslov"]);
                Opis = (formData["Opis"]);
                IdVrsteZahtjeva = int.Parse(formData["IdVrsteZahtjeva"]);
                IdPrioriteta = int.Parse(formData["IdPrioriteta"]);
                IdProjekta = int.Parse(formData["IdProjekta"]);
                vrstaZahtjeva = (formData["IdVrsteZahtjevaNavigation.Naziv"]);
                prioritet = (formData["IdPrioritetaNavigation.Naziv"]);
                projekt = (formData["IdProjektaNavigation.Naziv"]);




                if (kljuc.StartsWith("Zadatak[") && kljuc.EndsWith("].IdStavke"))
                {
                    int idStavke = int.Parse(formData[kljuc]);

                    ZadatakStavka zadatak = new ZadatakStavka
                    {
                        IdStavke = idStavke,
                        ImeZadatka = formData[$"Zadatak[{idStavke}].ImeZadatka"],
                        PlaniraniPocetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].PlaniraniPocetak"]),
                        PlaniraniZavrsetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].PlaniraniZavrsetak"]),
                        StvarniPocetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].StvarniPocetak"]),
                        StvarniZavrsetak = DateTime.Parse(formData[$"Zadatak[{idStavke}].StvarniZavrsetak"]),
                        Zahtjev = formData[$"Zadatak[{idStavke}].Zahtjev"],
                        Status = formData[$"Zadatak[{idStavke}].Status"],
                        Osoba = formData[$"Zadatak[{idStavke}].Osoba"],
                        Prioritet = formData[$"Zadatak[{idStavke}].Prioritet"]
                    };

                    listaZadataka.Add(zadatak);
                }
            }

            List<Zadatak> zadaciZahtjeva =
                ctx.Zadatci.Where(d => d.IdZahtjeva == IdZahtjeva).AsNoTracking().ToList();
            List<Zadatak> zadacii = new List<Zadatak>();
            List<Zadatak> listabrisanja = new List<Zadatak>();
            List<Zadatak> listaupdate = new List<Zadatak>();

            foreach (ZadatakStavka zad in listaZadataka)
            {
                int idPrioriteta = ctx.Prioriteti
                                            .Where(z => z.Naziv == zad.Prioritet)
                                            .Select(z => z.IdPrioriteta)
                                            .FirstOrDefault();
                int idStatusa = ctx.Statusi
                                .Where(z => z.Naziv == zad.Status)
                                .Select(z => z.IdStatus)
                                .FirstOrDefault();
                int idZahtjeva = ctx.Zahtjevi
                                .Where(z => z.Naslov == zad.Zahtjev)
                                .Select(z => z.IdPrioriteta)
                                .FirstOrDefault();

                string[] dijelovi = zad.Osoba.Split(' ');

                int oib = ctx.Osobe
                    .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                    .Select(z => z.Oibosoba)
                    .FirstOrDefault();

                Zadatak novi = new Zadatak(zad.ImeZadatka, zad.PlaniraniPocetak, zad.PlaniraniZavrsetak, zad.StvarniPocetak, zad.StvarniZavrsetak, idZahtjeva, idStatusa, oib, idPrioriteta);
                zadacii.Add(novi);



            }
            List<int> zadaciZahId = zadaciZahtjeva.Select(e => e.IdZadatka).ToList();
            List<int> radoviposaoID = zadacii.Select(e => e.IdZadatka).ToList();
            List<int> listabrisanjaID = listabrisanja.Select(e => e.IdZadatka).ToList();
            List<int> listaupdateID = listaupdate.Select(e => e.IdZadatka).ToList();


            try
            {
                Zahtjev z = new Zahtjev(Naslov, Opis, IdVrsteZahtjeva, IdPrioriteta, IdProjekta);

                ctx.Add(z);
                await ctx.SaveChangesAsync();


                foreach (var var in listaupdate)
                {
                    ctx.Update(var);
                }
                await ctx.SaveChangesAsync();

                foreach (var var in zadacii)
                {
                    Zadatak upload = new Zadatak(var.ImeZadatka, var.PlaniraniPocetak,
                    var.PlaniraniZavrsetak, var.StvarniPocetak, var.StvarniZavrsetak, z.IdZahtjeva, var.IdStatus,
                    var.Oibosoba, var.IdPrioriteta);

                    ctx.Add(upload);
                }
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                TempData["ErrorMessage"] = "An error occurred.";
                return RedirectToAction(nameof(Index));
            }

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var zah = new ZahtjevDTOViewModel()
            {

            };
            return View(zah);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int IdZahtjeva, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            var zahtjev = await ctx.Zahtjevi
                .Where(d => d.IdZahtjeva == IdZahtjeva)
                .SingleOrDefaultAsync();
            List<Zahtjev> zadaciZahtjeva =
                ctx.Zahtjevi.Where(d => d.IdZahtjeva == IdZahtjeva).AsNoTracking().ToList();

            if (zahtjev != null)
            {
                try
                {
                    foreach (var var in zadaciZahtjeva)
                    {
                        ctx.Remove(var);
                    }
                    await ctx.SaveChangesAsync();


                    ctx.Remove(zahtjev);
                    await ctx.SaveChangesAsync();
                    TempData[Constants.Message] = $"Zahtjev {zahtjev.IdZahtjeva} uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja zahtjeva: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
            }
            else
            {
                TempData[Constants.Message] = "Ne postoji Zahtjev s id-om: " + IdZahtjeva;
                TempData[Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { filter, page, sort, ascending });
        }

        public async Task<IActionResult> Show2(int id)
        {
            var query = ctx.Zadatci
                  .Include(z => z.IdZahtjevaNavigation)
                  .Include(z => z.IdStatusNavigation)
                  .Include(z => z.OibosobaNavigation)
                  .Include(z => z.IdPrioritetaNavigation)
                  .Include(z => z.EvidencijaRada)
                  .AsNoTracking();

            int count = ctx.Zadatci.Select(q => q.IdZahtjeva == id).Count();

            var zadaci = query
                .Where(q => q.IdZahtjeva == id)
                .Take(count)
                .ToList();

            var model = new ZadatakViewModel()
            {
                Zadatci = zadaci
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