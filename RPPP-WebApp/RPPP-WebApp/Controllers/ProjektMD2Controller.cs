using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Views.Projekt;

namespace RPPP_WebApp.Controllers;

public class ProjektMD2Controller : Controller
{
    private readonly DBContext ctx;
    private readonly AppSettings appSettings;

    public ProjektMD2Controller(DBContext context, IOptionsSnapshot<AppSettings> options)
    {
        this.ctx = context;
        appSettings = options.Value;
    }

    public async Task<IActionResult> Index(string filter, int page = 1, int sort = 1, bool ascending = true)
    {
        int pgsize = appSettings.PageSize;

        var query = ctx.Projekti
            .Include(p => p.IdVrsteProjektaNavigation)
            .Include(p => p.OibnaruciteljNavigation)
            .Include(p => p.Dokuments)
            .AsNoTracking();

        #region Do filter

        ProjektFilter pf = ProjektFilter.FromString(filter);
        if (!pf.IsEmpty())
        {
            if (pf.IdProjekta.HasValue)
            {
                pf.Naziv = await ctx.Projekti
                    .Where(p => p.IdProjekta == pf.IdProjekta)
                    .Select(p => p.Naziv)
                    .FirstOrDefaultAsync();
            }

            query = pf.Apply(query);
        }
        #endregion

        var pagingInfo = new PagingInfo
        {
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pgsize,
            TotalItems = query.Count()
        };

        if (page < 1 || page > pagingInfo.TotalPages)
        {
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }

        query = query.ApplySort(sort, ascending);

        var svi = query
            .Skip((page - 1) * pgsize)
            .Take(pgsize)
            .ToList();

        var model = new ProjektMDViewModel()
        {
            Projekti = svi,
            PagingInfo = pagingInfo,
            Filter = pf
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Filter(ProjektFilter filter)
    {
        return RedirectToAction(nameof(Index), new { filter = filter.ToString() });
    }

    [HttpGet]
    public Task<IActionResult> Edit(int id, int? position, string filter, int page = 1, int sort = 1,
        bool ascending = true)
    {
        return Show(id, position, filter, page, sort, ascending, viewName: nameof(Edit));
    }

    public async Task<IActionResult> Show(int id, int? position, string filter, int page = 1, int sort = 1,
        bool ascending = true, string viewName = nameof(Show))
    {
        var query = ctx.Dokumenti
            .Include(d => d.IdVrsteDokumentaNavigation)
            .Include(d => d.IdProjektaNavigation)
            .AsNoTracking();

        int num = ctx.Dokumenti.Select(q => q.IdProjekta == id).Count();

        var svi = query
            .Where(q => q.IdProjekta == id)
            .Take(num)
            .ToList();

        var model = new DokumentiViewModel()
        {
            Dokumenti = svi
        };

        var doki = await ctx.Dokumenti
            .Where(d => d.IdDokumenta == id)
            .Select(d => new Dokument()
            {
                DatumNastanka = d.DatumNastanka,
                Format = d.Format,
                IdProjekta = d.IdProjekta,
                IdVrsteDokumenta = d.IdVrsteDokumenta,
                Stavka = d.Stavka
            })
            .SingleOrDefaultAsync();

        return View(model);
    }

    private async Task SetPreviousAndNext(int position, string filter, int sort, bool ascending)
    {
        var query = ctx.Projekti.AsQueryable();

        ProjektFilter pf = new ProjektFilter();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            pf = ProjektFilter.FromString(filter);
            if (!pf.IsEmpty())
            {
                query = pf.Apply(query);
            }
        }

        query = query.ApplySort(sort, ascending);

        if (position > 0)
        {
            ViewBag.Previous = await query.Skip(position - 1).Select(p => p.IdProjekta).FirstAsync();
        }

        if (position < await query.CountAsync() - 1)
        {
            ViewBag.Next = await query.Skip(position - 1).Select(p => p.IdProjekta).FirstAsync();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] IFormCollection fromData, int? position, string filter,
        int page = 1, int sort = 1, bool ascending = true)
    {
        int IdProjekt = 0;
        String NazivProjekta = null;
        String OpisProjekta = null;
        DateTime DatumPocetka = new DateTime(0);
        DateTime DatumZavrsetka = new DateTime(0);
        int OibNarucitelja = 0;
        int IdVrsteProjekta = 0;
        string narucitelj = null;
        string vrsta = null;

        List<Stavka> listaStavci = new List<Stavka>();

        foreach (var key in fromData.Keys)
        {
            IdProjekt = int.Parse(fromData["IdProjekta"]);
            NazivProjekta = fromData["NazivProjekta"];
            OpisProjekta = fromData["OpisProjekta"];
            DatumPocetka = DateTime.Parse(fromData["DatumPocetka"]);
            DatumZavrsetka = DateTime.Parse(fromData["DatumZavrsetka"]);
            OibNarucitelja = int.Parse(fromData["OibNarucitelja"]);
            IdVrsteProjekta = int.Parse(fromData["IdVrsteProjekta"]);
            narucitelj = fromData["narucitelj"];
            vrsta = fromData["vrsta"];

            if (key.StartsWith("Projekt[") && key.EndsWith("].IdStavke"))
            {
                int idStavke = int.Parse(fromData[key]);

                Stavka stavka = new Stavka
                {
                    IdStavke = idStavke,
                    Opis = fromData[$"Projekt[{idStavke}].Opis"],
                    DatumNastanka = DateTime.Parse(fromData[$"Projekt[{idStavke}].datumNastanka"]),
                    Format = fromData[$"Projekt[{idStavke}].format"],
                    IdProjekta = int.Parse(fromData[$"Projekt[{idStavke}].idProjekta"]),
                    IdVrsteDokumenta = int.Parse(fromData[$"Projekt[{idStavke}].idVrsteDokuments"])
                };
                
                listaStavci.Add(stavka);
            }
        }

        List<Dokument> doki = ctx.Dokumenti.Where(d => d.IdProjekta == IdProjekt).AsNoTracking().ToList();
        List<Dokument> brisi = new List<Dokument>();
        List<Dokument> radi = new List<Dokument>();
        List<Dokument> update = new List<Dokument>();
        
        if (listaStavci.Count == 0)
        {
            foreach (var st in doki)
            {
                brisi.Add(st);
            }
        }
        else
        {
            foreach (Stavka st in listaStavci)
            {
                if (st.IdStavke < 1000)
                {
                    int idVrsta = ctx.VrsteDokumenta
                        .Where(z => z.IdVrsteDokumenta != st.IdVrsteDokumenta)
                        .Select(z => z.IdVrsteDokumenta)
                        .FirstOrDefault();
                    int idProjekt = ctx.Projekti
                        .Where(z => z.IdProjekta != st.IdProjekta)
                        .Select(z => z.IdProjekta)
                        .FirstOrDefault();
                    
                    Dokument post = new Dokument(st.IdStavke, st.DatumNastanka, st.Opis,
                        st.Format, idVrsta, idProjekt);

                    update.Add(post);
                }
                else
                {
                    int idVrsta = ctx.VrsteDokumenta
                        .Where(z => z.IdVrsteDokumenta == st.IdVrsteDokumenta)
                        .Select(z => z.IdVrsteDokumenta)
                        .FirstOrDefault();
                    int idProjekt = ctx.Projekti
                        .Where(z => z.IdProjekta == st.IdProjekta)
                        .Select(z => z.IdProjekta)
                        .FirstOrDefault();
                    Dokument post = new Dokument(st.IdStavke, st.DatumNastanka, st.Opis,
                        st.Format, idVrsta, idProjekt);
                    
                    radi.Add(post);
                }
            }
        }

        List<int> dokiId = doki.Select(d => d.IdDokumenta).ToList();
        List<int> brId = brisi.Select(d => d.IdDokumenta).ToList();
        List<int> radId = radi.Select(d => d.IdDokumenta).ToList();
        List<int> upId = update.Select(d => d.IdDokumenta).ToList();

        foreach (int dok in dokiId)
        {
            if (!dokiId.Contains(dok) && !upId.Contains(dok))
            {
                Dokument dodaj = doki.FirstOrDefault(d => d.IdDokumenta == dok);
                brisi.Add(dodaj);
            }
        }

        try
        {

            Projekt pr = new Projekt(IdProjekt, NazivProjekta, OpisProjekta, DatumPocetka, DatumZavrsetka,
                OibNarucitelja, IdVrsteProjekta);
            ctx.Update(pr);
            await ctx.SaveChangesAsync();

            foreach (var vari in brisi)
            {
                ctx.Remove(vari);
            }
            await ctx.SaveChangesAsync();
            foreach (var vari in update)
            {
                ctx.Update(vari);
            }
            await ctx.SaveChangesAsync();
            foreach (var vari in radi)
            {
                ctx.Add(vari);
            }
            await ctx.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));

        }
        catch (Exception e)
        {
            return null;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] IFormCollection fromData)
    {
        int IdProjekt = 0;
        String NazivProjekta = null;
        String OpisProjekta = null;
        DateTime DatumPocetka = new DateTime(0);
        DateTime DatumZavrsetka = new DateTime(0);
        int OibNarucitelja = 0;
        int IdVrsteProjekta = 0;
        string narucitelj = null;
        string vrsta = null;

        List<Stavka> listaStavci = new List<Stavka>();

        foreach (var key in fromData.Keys)
        {
            IdProjekt = int.Parse(fromData["IdProjekta"]);
            NazivProjekta = fromData["NazivProjekta"];
            OpisProjekta = fromData["OpisProjekta"];
            DatumPocetka = DateTime.Parse(fromData["DatumPocetka"]);
            DatumZavrsetka = DateTime.Parse(fromData["DatumZavrsetka"]);
            OibNarucitelja = int.Parse(fromData["OibNarucitelja"]);
            IdVrsteProjekta = int.Parse(fromData["IdVrsteProjekta"]);
            narucitelj = fromData["narucitelj"];
            vrsta = fromData["vrsta"];

            if (key.StartsWith("Projekt[") && key.EndsWith("].IdStavke"))
            {
                int idStavke = int.Parse(fromData[key]);

                Stavka stavka = new Stavka
                {
                    IdStavke = idStavke,
                    Opis = fromData[$"Projekt[{idStavke}].Opis"],
                    DatumNastanka = DateTime.Parse(fromData[$"Projekt[{idStavke}].datumNastanka"]),
                    Format = fromData[$"Projekt[{idStavke}].format"],
                    IdProjekta = int.Parse(fromData[$"Projekt[{idStavke}].idProjekta"]),
                    IdVrsteDokumenta = int.Parse(fromData[$"Projekt[{idStavke}].idVrsteDokuments"])
                };
                
                listaStavci.Add(stavka);
            }
        }

        List<Dokument> doki = new List<Dokument>();
        List<Dokument> br = new List<Dokument>();
        List<Dokument> rad = new List<Dokument>();
        List<Dokument> up = new List<Dokument>();
        
        foreach(Stavka st in listaStavci)
        {
            int idVrsta = ctx.VrsteDokumenta
                .Where(z => z.IdVrsteDokumenta != st.IdVrsteDokumenta)
                .Select(z => z.IdVrsteDokumenta)
                .FirstOrDefault();
            int idProjekt = ctx.Projekti
                .Where(z => z.IdProjekta != st.IdProjekta)
                .Select(z => z.IdProjekta)
                .FirstOrDefault();

            Dokument post = new Dokument(st.IdStavke, st.DatumNastanka, st.Opis,
                st.Format, idVrsta, idProjekt);
            
            rad.Add(post);

        }
        List<int> dokiId = doki.Select(d => d.IdDokumenta).ToList();
        List<int> brId = br.Select(d => d.IdDokumenta).ToList();
        List<int> radId = rad.Select(d => d.IdDokumenta).ToList();
        List<int> upId = up.Select(d => d.IdDokumenta).ToList();
        
        try
        {

            Projekt pr = new Projekt(IdProjekt, NazivProjekta, OpisProjekta, DatumPocetka, DatumZavrsetka,
                OibNarucitelja, IdVrsteProjekta);
            ctx.Update(pr);
            await ctx.SaveChangesAsync();
            
            foreach (var vari in up)
            {
                ctx.Update(vari);
            }
            await ctx.SaveChangesAsync();
            foreach (var vari in rad)
            {
                ctx.Add(vari);
            }
            await ctx.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));

        }
        catch (Exception e)
        {
            return null;
        }
        
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var pr = new ProjektDTOViewModel()
        {

        };
        return View(pr);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int IdProjekt, string filter, int page = 1, int sort = 1,
        bool ascending = true)
    {
        var proj = await ctx.Projekti
            .Where(d => d.IdProjekta == IdProjekt)
            .SingleOrDefaultAsync();

        List<Dokument> doki = ctx.Dokumenti
            .Where(d => d.IdProjekta == IdProjekt)
            .AsNoTracking()
            .ToList();

        if (proj != null)
        {
            try
            {
                foreach (var vari in doki)
                {
                    ctx.Remove(vari);
                }
                await ctx.SaveChangesAsync();

                ctx.Remove(proj);
                await ctx.SaveChangesAsync();
                TempData[Constants.Message] = $"Projekt {proj.IdProjekta} uspjesno obrisan";
                TempData[Constants.ErrorOccurred] = false;
            }
            catch (Exception e)
            {
                TempData[Constants.Message] = "Pogreska brisanja";
                TempData[Constants.ErrorOccurred] = true;
            }
        }
        else
        {
            TempData[Constants.Message] = "Ne postoji takav projekt";
            TempData[Constants.ErrorOccurred] = true;
        }

        return RedirectToAction(nameof(Index), new { filter, page, sort, ascending });
    }
}