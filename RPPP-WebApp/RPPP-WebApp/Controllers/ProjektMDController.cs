using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Views.Projekt;

namespace RPPP_WebApp.Controllers;
public class ProjektMDController : Controller
{
    private readonly DBContext context;
    private readonly AppSettings appSettings;
    private readonly ILogger<ProjektMDController> logger;
    
    public ProjektMDController(DBContext context, IOptionsSnapshot<AppSettings> options, ILogger<ProjektMDController> logger)
    {
        this.context = context;
        this.logger = logger;
        appSettings = options.Value;
    }
    
    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appSettings.PageSize;
        
        var query = context.Projekti
            .Include(p => p.IdVrsteProjektaNavigation)
            .Include(p => p.OibnaruciteljNavigation)
            .Include(p => p.Dokuments)
            .Include(p => p.EvidencijaUlogas)
            .Include(p => p.ProjektnaKarticas)
            .Include(p => p.Zahtjevs)
            .AsNoTracking();//provjeri ovo
        
        if(query.Count() == 0){
            logger.LogInformation("Ne postoje projekti");
            TempData[Constants.Message] = "Ne postoje projekti";
            TempData[Constants.ErrorOccurred] = false;
        }

        /*var projekt = query
                      .OrderBy(p => p.IdProjekta)
                      .ToList();*/


        var pagingInfo = new PagingInfo{
            CurrentPage = page,
            Sort = sort,
            Ascending = ascending,
            ItemsPerPage = pagesize,
            TotalItems = query.Count()
        };

        if(page < 1 || page > pagingInfo.TotalPages){
            return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
        }

        query = query.ApplySort(sort, ascending);

        var projektici = query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();       
 
        var model = new ProjektMDViewModel
        {
            Projekti = projektici,
            PagingInfo = pagingInfo
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Show(int id)
    {
        var query = context.Dokumenti
            .Include(d => d.IdVrsteDokumentaNavigation)
            .Include(d => d.IdProjektaNavigation)
            .AsNoTracking();

        int num = context.Dokumenti.Select(q => q.IdProjekta == id).Count();

        var svi = query
            .Where(q => q.IdProjekta == id)
            .Take(num)
            .ToList();

        var model = new DokumentiViewModel()
        {
            Dokumenti = svi
        };

        var doki = await context.Dokumenti
            .Where(d => d.IdProjekta == id)
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
}