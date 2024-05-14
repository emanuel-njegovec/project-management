using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Views.Zadatak;

namespace RPPP_WebApp.Controllers;

public class ZadatakMDController : Controller
{
    private readonly DBContext context;
    private readonly AppSettings appSettings;
    private readonly ILogger<ZadatakMDController> logger;
    
    public ZadatakMDController(DBContext context, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakMDController> logger)
    {
        this.context = context;
        this.logger = logger;
        appSettings = options.Value;
    }
    
    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appSettings.PageSize;
        
        var query = context.Zadatci
            .Include(z => z.IdZahtjevaNavigation)
            .Include(z => z.IdStatusNavigation)
            .Include(z => z.OibosobaNavigation)
            .Include(z => z.IdPrioritetaNavigation)
            .Include(z => z.EvidencijaRada)
            .AsNoTracking();
        
        if (query.Count() == 0)
        {
            logger.LogInformation("Ne postoji nijedan zadatak");
            TempData[Constants.Message] = "Ne postoji niti jedan zadatak.";
            TempData[Constants.ErrorOccurred] = false;
            //return RedirectToAction(nameof(Create));
        }
        
        
        //var zadci = query.OrderBy(g => g.IdZadatka).ToList();
        
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
            PagingInfo = pagingInfo
        };
        return View(model);
    }
    
    
    [HttpGet]
    public async Task<IActionResult> Show(int id)
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