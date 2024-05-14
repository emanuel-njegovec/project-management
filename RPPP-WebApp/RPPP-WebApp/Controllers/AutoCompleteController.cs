using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers;

public class AutoCompleteController : Controller
{
    private readonly DBContext ctx;
    private readonly AppSettings appData;
    
    public AutoCompleteController(DBContext ctx, IOptionsSnapshot<AppSettings> options)
    {
        this.ctx = ctx;
        appData = options.Value;
    }
    
    public async Task<IEnumerable<IdLabel>> Zahtjev(string term)
    {
        var query = ctx.Zahtjevi
            .Select(m => new IdLabel
            {
                Id = m.IdZahtjeva,
                Label = m.Naslov
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
    
    public async Task<IEnumerable<IdLabel>> Status(string term)
    {
        var query = ctx.Statusi
            .Select(m => new IdLabel
            {
                Id = m.IdStatus,
                Label = m.Naziv
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
  
    public async Task<IEnumerable<IdLabel>> Osoba(string term)
    {
        var query = ctx.Osobe
            .Select(m => new IdLabel
            {
                Id = m.Oibosoba,
                Label = m.Ime + " " + m.Prezime
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
    
    public async Task<IEnumerable<IdLabel>> Osoba2(string term)
    {
        var query = ctx.Osobe
            .Select(m => new IdLabel
            {
                Id = m.Oibosoba,
                Label = m.Ime + " " + m.Prezime
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
    
    public async Task<IEnumerable<IdLabel>> Osoba3(string term)
    {
        var query = ctx.Osobe
            .Select(m => new IdLabel
            {
                Id = m.Oibosoba,
                Label = m.Ime + " " + m.Prezime
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
    
    public async Task<IEnumerable<IdLabel>> Prioritet(string term)
    {
        var query = ctx.Prioriteti
            .Select(m => new IdLabel
            {
                Id = m.IdPrioriteta,
                Label = m.Naziv
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
    
    public async Task<IEnumerable<IdLabel>> Zadatak(string term)
    {
        var query = ctx.Zadatci
            .Select(m => new IdLabel
            {
                Id = m.IdZadatka,
                Label = m.ImeZadatka
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }


    public async Task<IEnumerable<IdLabel>> VrstaPosla(string term)
    {
        var query = ctx.VrstePosla
            .Select(m => new IdLabel
            {
                Id = m.IdVrstePosla,
                Label = m.Naziv
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
    
    public async Task<IEnumerable<IdLabel>> OpisEvRada(string term)
    {
        var query = ctx.EvidencijaRada
            .Select(m => new IdLabel
            {
                Id = m.IdEvidencijaRad,
                Label = m.Opis
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }

    public async Task<IEnumerable<IdLabel>> VrstaZahtjeva(string term)
    {
        var query = ctx.VrsteZahtjeva
            .Select(m => new IdLabel
            {
                Id = m.IdVrsteZahtjeva,
                Label = m.Naziv
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }

    public async Task<IEnumerable<IdLabel>> Projekt(string term)
    {
        var query = ctx.Projekti
            .Select(m => new IdLabel
            {
                Id = m.IdProjekta,
                Label = m.Naziv
            })
            .Where(l => l.Label.Contains(term));

        var list = await query.OrderBy(l => l.Label)
            .ThenBy(l => l.Id)
            .Take(appData.AutoCompleteCount)
            .ToListAsync();
        return list;
    }
    
    

}