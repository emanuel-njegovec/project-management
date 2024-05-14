using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Views.Zadatak;

namespace RPPP_WebApp.Controllers;

public class ZadatakMD2Controller : Controller
{
    private readonly DBContext context;
    private readonly AppSettings appSettings;
    
    public ZadatakMD2Controller(DBContext context, IOptionsSnapshot<AppSettings> options)
    {
        this.context = context;
        appSettings = options.Value;
    }

    public async Task<IActionResult> Index(string filter, int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appSettings.PageSize;
        
        var query = context.Zadatci
            .Include(z => z.IdZahtjevaNavigation)
            .Include(z => z.IdStatusNavigation)
            .Include(z => z.OibosobaNavigation)
            .Include(z => z.IdPrioritetaNavigation)
            .Include(z => z.EvidencijaRada)
            .AsNoTracking();
        
        
        #region Apply filter
        ZadatakFilter df = ZadatakFilter.FromString(filter);
        if (!df.IsEmpty())
        {
            if (df.IdZadatka.HasValue)
            {
                df.ImeZadatka = await context.Zadatci
                    .Where(p => p.IdZadatka == df.IdZadatka)
                    .Select(vp => vp.ImeZadatka)
                    .FirstOrDefaultAsync();
            }
            query = df.Apply(query);
        }
        #endregion
        
        
        var pagingInfo = new PagingInfo
        {
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pagesize,
            TotalItems = query.Count()
        };
        
        if (page < 1 || page > pagingInfo.TotalPages)
        {
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }
        
        query = query.ApplySort(sort, ascending);
        
        var svizad = query
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToList();
        
        var model = new ZadatakMDViewModel()
        {
            Zadaci = svizad,
            PagingInfo = pagingInfo,
            Filter = df
        };

        return View(model);
    }
    
    [HttpPost]
    public IActionResult Filter(ZadatakFilter filter)
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
        
        
        
        List<EvidencijaRadaDTO> radic = await context.EvidencijaRada                            
            .Where(m => m.IdZadatka == id)
            .Select(m => new EvidencijaRadaDTO()
            {
                IdEvidencijaRad = m.IdEvidencijaRad,
                Opis = m.Opis,
                DatumRada = m.DatumRada,
                VrijemeRada = m.VrijemeRada,
                Oibosoba = m.Oibosoba,
                IdZadatka = m.IdZadatka,
                IdVrstePosla = m.IdVrstePosla,
                Persona = m.OibosobaNavigation.Ime + " " + m.OibosobaNavigation.Prezime,
                vrstaposlica = m.IdVrstePoslaNavigation.Naziv,
                imezadatka = m.IdZadatkaNavigation.ImeZadatka
                
            })
            .ToListAsync();
        
        var zad = await context.Zadatci                            
            .Where(m => m.IdZadatka == id)
            .Select(m => new ZadatakDTOViewModel()
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
            .FirstOrDefaultAsync();

        zad.RadoviPravi = radic;
        
        if (zad == null)
        {
            return NotFound($"Zadatak {id} ne postoji");
        }
        else
        {
            zad.ImeZadatka = await context.Zadatci
                .Where(p => p.IdZadatka == zad.IdZadatka)
                .Select(p => p.ImeZadatka)
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
        var query = context.Zadatci.AsQueryable();  
        
        ZadatakFilter df = new ZadatakFilter();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            df = ZadatakFilter.FromString(filter);
            if (!df.IsEmpty())
            {
                query = df.Apply(query);
            }
        }
        
        query = query.ApplySort(sort, ascending);      
        if (position > 0)
        {
            ViewBag.Previous = await query.Skip(position - 1).Select(d => d.IdZadatka).FirstAsync();
        }
        if (position < await query.CountAsync() - 1) 
        {
            ViewBag.Next = await query.Skip(position + 1).Select(d => d.IdZadatka).FirstAsync();
        }


    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] IFormCollection formData, int? position, string filter, int page = 1,
        int sort = 1, bool ascending = true)
    {
        int IdZadatka = 0;
        string ImeZadatka = null;
        DateTime PlaniraniPocetak = new DateTime(0);
        DateTime PlaniraniZavrsetak = new DateTime(0);
        DateTime StvarniPocetak = new DateTime(0);
        DateTime StvarniZavrsetak = new DateTime(0);
        int IdZahtjeva = 0;
        int IdStatus = 0;
        int Oibosoba = 0;
        int IdPrioriteta = 0;
        string zahtjev = null;
        string status = null;
        string osoba = null;
        string prioritet = null;
        
        List<PosaoStavka> listaPoslova = new List<PosaoStavka>();

        foreach (var kljuc in formData.Keys)
        {
            IdZadatka = int.Parse(formData["IdZadatka"]);
            ImeZadatka = (formData["ImeZadatka"]);
            PlaniraniPocetak = DateTime.Parse(formData["PlaniraniPocetak"]);
            PlaniraniZavrsetak = DateTime.Parse(formData["PlaniraniZavrsetak"]);
            StvarniPocetak = DateTime.Parse(formData["StvarniPocetak"]);
            StvarniZavrsetak = DateTime.Parse(formData["StvarniZavrsetak"]);
            IdZahtjeva = int.Parse(formData["IdZahtjeva"]);
            IdStatus = int.Parse(formData["IdStatus"]);
            Oibosoba = int.Parse(formData["Oibosoba"]);
            IdPrioriteta = int.Parse(formData["IdPrioriteta"]);
            zahtjev = (formData["IdZahtjevaNavigation.Naslov"]);
            status = (formData["IdStatusNavigation.Naziv"]);
            osoba = (formData["OibosobaNavigation.Ime"]);
            prioritet = (formData["IdPrioritetaNavigation.Naziv"]);
            
            
            
            
            if (kljuc.StartsWith("Posao[") && kljuc.EndsWith("].IdStavke"))
            {
                int idStavke = int.Parse(formData[kljuc]);

                PosaoStavka posao = new PosaoStavka
                {
                    IdStavke = idStavke,
                    Opis = formData[$"Posao[{idStavke}].Opis"],
                    DatumRada = DateTime.Parse(formData[$"Posao[{idStavke}].DatumRada"]),
                    VrijemeRada = int.Parse(formData[$"Posao[{idStavke}].VrijemeRada"]),
                    Persona = formData[$"Posao[{idStavke}].Persona"],
                    VrstaPoslica = formData[$"Posao[{idStavke}].vrstaposlica"]
                };

                listaPoslova.Add(posao);
            }
        }



        List<EvidencijaRada> radoviZad =
            context.EvidencijaRada.Where(d => d.IdZadatka == IdZadatka).AsNoTracking().ToList();
        List<EvidencijaRada> radoviposao = new List<EvidencijaRada>();
        List<EvidencijaRada> listabrisanja = new List<EvidencijaRada>();
        List<EvidencijaRada> listaupdate = new List<EvidencijaRada>();
        
        
        

        if (listaPoslova.Count == 0)
        {
            foreach (var VARIABLE in radoviZad)
            {
                listabrisanja.Add(VARIABLE);
            }
        }
        else
        {

            foreach (PosaoStavka poslic in listaPoslova)
            {
                if (poslic.IdStavke < 1000)
                {


                    string[] dijelovi = poslic.Persona.Split(' ');
                    int oibic = context.Osobe
                        .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                        .Select(z => z.Oibosoba)
                        .FirstOrDefault();

                    int idVrsta = context.VrstePosla
                        .Where(z => z.Naziv == poslic.VrstaPoslica)
                        .Select(z => z.IdVrstePosla)
                        .FirstOrDefault();

                    EvidencijaRada radpostojeci = new EvidencijaRada(poslic.IdStavke, poslic.Opis, poslic.DatumRada,
                        poslic.VrijemeRada, oibic, IdZadatka, idVrsta);

                    listaupdate.Add(radpostojeci);
                }
                else
                {
                        string[] dijelovi = poslic.Persona.Split(' ');

                        int oibic = context.Osobe
                            .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                            .Select(z => z.Oibosoba)
                            .FirstOrDefault();

                        int idVrsta = context.VrstePosla
                            .Where(z => z.Naziv == poslic.VrstaPoslica)
                            .Select(z => z.IdVrstePosla)
                            .FirstOrDefault();

                        EvidencijaRada posvenovi = new EvidencijaRada(poslic.Opis, poslic.DatumRada, poslic.VrijemeRada,
                            oibic, IdZadatka, idVrsta);
                        radoviposao.Add(posvenovi);
                    
                }
            }
        }
        List<int> radoviZadID = radoviZad.Select(e => e.IdEvidencijaRad).ToList();
        List<int> radoviposaoID = radoviposao.Select(e => e.IdEvidencijaRad).ToList();
        List<int> listabrisanjaID = listabrisanja.Select(e => e.IdEvidencijaRad).ToList();
        List<int> listaupdateID = listaupdate.Select(e => e.IdEvidencijaRad).ToList();
        
        foreach (int rad in radoviZadID)
        {
            if (!radoviposaoID.Contains(rad) && !listaupdateID.Contains(rad))
            {
                EvidencijaRada raddodaj = radoviZad.FirstOrDefault(d => d.IdEvidencijaRad == rad);
                
                listabrisanja.Add(raddodaj);
            }
        }
        
        
        
        
        try
        {
            Zadatak doka = new Zadatak(IdZadatka, ImeZadatka, PlaniraniPocetak,
                PlaniraniZavrsetak, StvarniPocetak, StvarniZavrsetak, IdZahtjeva, IdStatus,
                Oibosoba, IdPrioriteta);
            context.Update(doka);
            await context.SaveChangesAsync();
            
            foreach (var variajblica in listabrisanja)
            {
                context.Remove(variajblica);
            }
            await context.SaveChangesAsync();
            
            foreach (var variajblica in listaupdate)
            {
                context.Update(variajblica);
            }
            await context.SaveChangesAsync();
            
            foreach (var variajblica in radoviposao)
            {
                context.Add(variajblica);
            }
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exc)
        {
            return null;
        }
        
        
        

        
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] IFormCollection formData)
    {
        int IdZadatka = 0;
        string ImeZadatka = null;
        DateTime PlaniraniPocetak = new DateTime(0);
        DateTime PlaniraniZavrsetak = new DateTime(0);
        DateTime StvarniPocetak = new DateTime(0);
        DateTime StvarniZavrsetak = new DateTime(0);
        int IdZahtjeva = 0;
        int IdStatus = 0;
        int Oibosoba = 0;
        int IdPrioriteta = 0;
        string zahtjev = null;
        string status = null;
        string osoba = null;
        string prioritet = null;
        
        List<PosaoStavka> listaPoslova = new List<PosaoStavka>();

        foreach (var kljuc in formData.Keys)
        {
            IdZadatka = int.Parse(formData["IdZadatka"]);
            ImeZadatka = (formData["ImeZadatka"]);
            PlaniraniPocetak = DateTime.Parse(formData["PlaniraniPocetak"]);
            PlaniraniZavrsetak = DateTime.Parse(formData["PlaniraniZavrsetak"]);
            StvarniPocetak = DateTime.Parse(formData["StvarniPocetak"]);
            StvarniZavrsetak = DateTime.Parse(formData["StvarniZavrsetak"]);
            IdZahtjeva = int.Parse(formData["IdZahtjeva"]);
            IdStatus = int.Parse(formData["IdStatus"]);
            Oibosoba = int.Parse(formData["Oibosoba"]);
            IdPrioriteta = int.Parse(formData["IdPrioriteta"]);
            zahtjev = (formData["IdZahtjevaNavigation.Naslov"]);
            status = (formData["IdStatusNavigation.Naziv"]);
            osoba = (formData["OibosobaNavigation.Ime"]);
            prioritet = (formData["IdPrioritetaNavigation.Naziv"]);
            
            
            
            
            if (kljuc.StartsWith("Posao[") && kljuc.EndsWith("].IdStavke"))
            {
                int idStavke = int.Parse(formData[kljuc]);

                PosaoStavka posao = new PosaoStavka
                {
                    IdStavke = idStavke,
                    Opis = formData[$"Posao[{idStavke}].Opis"],
                    DatumRada = DateTime.Parse(formData[$"Posao[{idStavke}].DatumRada"]),
                    VrijemeRada = int.Parse(formData[$"Posao[{idStavke}].VrijemeRada"]),
                    Persona = formData[$"Posao[{idStavke}].Persona"],
                    VrstaPoslica = formData[$"Posao[{idStavke}].vrstaposlica"]
                };

                listaPoslova.Add(posao);
            }
        }



        List<EvidencijaRada> radoviZad =
            context.EvidencijaRada.Where(d => d.IdZadatka == IdZadatka).AsNoTracking().ToList();
        List<EvidencijaRada> radoviposao = new List<EvidencijaRada>();
        List<EvidencijaRada> listabrisanja = new List<EvidencijaRada>();
        List<EvidencijaRada> listaupdate = new List<EvidencijaRada>();
        
        foreach (PosaoStavka poslic in listaPoslova)
            {
                        string[] dijelovi = poslic.Persona.Split(' ');

                        int oibic = context.Osobe
                            .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                            .Select(z => z.Oibosoba)
                            .FirstOrDefault();

                        int idVrsta = context.VrstePosla
                            .Where(z => z.Naziv == poslic.VrstaPoslica)
                            .Select(z => z.IdVrstePosla)
                            .FirstOrDefault();

                        EvidencijaRada posvenovi = new EvidencijaRada(poslic.Opis, poslic.DatumRada, poslic.VrijemeRada,
                            oibic, IdZadatka, idVrsta);
                        radoviposao.Add(posvenovi);
                    
                
            
        }
        List<int> radoviZadID = radoviZad.Select(e => e.IdEvidencijaRad).ToList();
        List<int> radoviposaoID = radoviposao.Select(e => e.IdEvidencijaRad).ToList();
        List<int> listabrisanjaID = listabrisanja.Select(e => e.IdEvidencijaRad).ToList();
        List<int> listaupdateID = listaupdate.Select(e => e.IdEvidencijaRad).ToList();
        

        try
        {
            Zadatak doka = new Zadatak(ImeZadatka, PlaniraniPocetak,
                PlaniraniZavrsetak, StvarniPocetak, StvarniZavrsetak, IdZahtjeva, IdStatus,
                Oibosoba, IdPrioriteta);
            context.Add(doka);
            await context.SaveChangesAsync();
            
            
            foreach (var variajblica in listaupdate)
            {
                context.Update(variajblica);
            }
            await context.SaveChangesAsync();
            
            foreach (var variajblica in radoviposao)
            {
                EvidencijaRada upload = new EvidencijaRada(variajblica.Opis, variajblica.DatumRada,
                    variajblica.VrijemeRada, variajblica.Oibosoba, doka.IdZadatka, variajblica.IdVrstePosla);
                
                context.Add(upload);
            }
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exc)
        {
            return null;
        }
        
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var zad = new ZadatakDTOViewModel()
        {
            
        };
        return View(zad);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int IdZadatka, string filter, int page = 1, int sort = 1, bool ascending = true)
    {
        var zadat = await context.Zadatci                             
            .Where(d => d.IdZadatka == IdZadatka)
            .SingleOrDefaultAsync();
        List<EvidencijaRada> radoviZad =
            context.EvidencijaRada.Where(d => d.IdZadatka == IdZadatka).AsNoTracking().ToList();
        
        if (zadat != null)
        {
            try
            {
                foreach (var VARIABLE in radoviZad)
                {
                    context.Remove(VARIABLE);
                }
                await context.SaveChangesAsync();
                
                
                context.Remove(zadat);
                await context.SaveChangesAsync();
                TempData[Constants.Message] = $"Zadatak {zadat.IdZadatka} uspješno obrisan.";
                TempData[Constants.ErrorOccurred] = false;
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = "Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
            }
        }
        else
        {
            TempData[Constants.Message] = "Ne postoji Zadatak s id-om: " + IdZadatka;
            TempData[Constants.ErrorOccurred] = true;
        }
        return RedirectToAction(nameof(Index), new { filter, page, sort, ascending });
    }

    public async Task<IActionResult> Show2(int id)
    {
        var query = context.EvidencijaRada
            .Include(z => z.OibosobaNavigation)
            .Include(z => z.IdZadatkaNavigation)
            .Include(z => z.IdVrstePoslaNavigation)
            .AsNoTracking();

        int brojsvih = context.EvidencijaRada.Select(q => q.IdZadatka == id).Count();

        var svizad = query
            .Where(q => q.IdZadatka == id)
            .Take(brojsvih)
            .ToList();
        
        var model = new EvidencijaRadaViewModel()
        {
            EviRadovi = svizad
        };
        
        var radic = await context.EvidencijaRada                            
            .Where(m => m.IdEvidencijaRad == id)
            .Select(m => new EvidencijaRada()
            {
                IdEvidencijaRad = m.IdEvidencijaRad,
                Opis = m.Opis,
                DatumRada = m.DatumRada,
                VrijemeRada = m.VrijemeRada,
                Oibosoba = m.Oibosoba,
                IdZadatka = m.IdZadatka,
                IdVrstePosla = m.IdVrstePosla,
                
            })
            .SingleOrDefaultAsync();

        //ZadatakDTO d = new ZadatakDTO();
        
        return View(model);
    }
}
