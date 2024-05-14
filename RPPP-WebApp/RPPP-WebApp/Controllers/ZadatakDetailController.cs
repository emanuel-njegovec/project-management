using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Views.Zadatak;
using ZadatakDTO = RPPP_WebApp.Views.Zadatak.ZadatakDTO;


namespace RPPP_WebApp.Controllers;

public class ZadatakDetailController : Controller
{

    private readonly DBContext ctx;
    private readonly AppSettings appSettings;
    private readonly ILogger<ZadatakDetailController> logger;


    public ZadatakDetailController(DBContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakDetailController> logger)
    {
        this.ctx = ctx;
        this.logger = logger;
        appSettings = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
    {
        int pagesize = appSettings.PageSize;

        var query = ctx.Zadatci
            .Include(z => z.IdZahtjevaNavigation)
            .Include(z => z.IdStatusNavigation)
            .Include(z => z.OibosobaNavigation)
            .Include(z => z.IdPrioritetaNavigation)
            .AsNoTracking();

        if (query.Count() == 0)
        {
            logger.LogInformation("Ne postoji nijedan zadatak");
            TempData[Constants.Message] = "Ne postoji niti jedan zadatak.";
            TempData[Constants.ErrorOccurred] = false;
        }


        var zadci = query.OrderBy(g => g.IdZadatka).ToList();

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

        var model = new ZadatakDetailViewModel()
        {
            Zadaci = svizad,
            PagingInfo = pagingInfo
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PrepareDropDownLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ZadatakDTO zadatak)
    {
        ZadatakDTO fake = new ZadatakDTO();
        if (ModelState.IsValid)
        {
            try
            {

                int idPrioriteta = ctx.Prioriteti
                    .Where(z => z.Naziv == zadatak.NazPrioriteta)
                    .Select(z => z.IdPrioriteta)
                    .FirstOrDefault();

                int idStatusa = ctx.Statusi
                    .Where(z => z.Naziv == zadatak.NazStatus)
                    .Select(z => z.IdStatus)
                    .FirstOrDefault();

                int idZahtjeva = ctx.Zahtjevi
                    .Where(z => z.Naslov == zadatak.NazZahtjeva)
                    .Select(z => z.IdPrioriteta)
                    .FirstOrDefault();

                string[] dijelovi = zadatak.NazOsoba.Split(' ');

                int oibic = ctx.Osobe
                    .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                    .Select(z => z.Oibosoba)
                    .FirstOrDefault();

                Zadatak novi = new Zadatak(zadatak.ImeZadatka, zadatak.PlaniraniPocetak, zadatak.PlaniraniZavrsetak, zadatak.StvarniPocetak, zadatak.StvarniZavrsetak, idZahtjeva, idStatusa, oibic, idPrioriteta);




                ctx.Add(novi);
                await ctx.SaveChangesAsync();

                TempData[Constants.Message] = $"Zadatak {novi.IdZadatka} dodan.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception exc)
            {
                await PrepareDropDownLists();
                return View(fake);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return View(fake);
        }
    }

	private async Task PrepareDropDownLists()
	{
		ViewBag.Zahtjevi = new SelectList(await ctx.Zahtjevi
							  .OrderBy(v => v.IdZahtjeva)
							  .Select(v => new SelectListItem
							  {
								  Value = v.Naslov,
								  Text = v.Naslov
							  })
							  .ToListAsync(), "Value", "Text");

		ViewBag.Statusi = new SelectList(await ctx.Statusi
							  .OrderBy(v => v.IdStatus)
							  .Select(v => new SelectListItem
							  {
								  Value = v.Naziv,
								  Text = v.Naziv
							  })
							  .ToListAsync(), "Value", "Text");

		ViewBag.Osobe = new SelectList(await ctx.Osobe
							  .OrderBy(v => v.Oibosoba)
							  .Select(v => new SelectListItem
							  {
								  Value = v.Ime + " " + v.Prezime,
								  Text = v.Ime + " " + v.Prezime
							  })
							  .ToListAsync(), "Value", "Text");

		ViewBag.Prioriteti = new SelectList(await ctx.Prioriteti
							  .OrderBy(v => v.IdPrioriteta)
							  .Select(v => new SelectListItem
							  {
								  Value = v.Naziv,
								  Text = v.Naziv
							  })
							  .ToListAsync(), "Value", "Text");
	}

	[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
    {
        try
        {
            var zadatak = await ctx.Zadatci.FindAsync(id);

            if (zadatak == null)
            {
                TempData[Constants.Message] = "Zadatak nije pronađen.";
                TempData[Constants.ErrorOccurred] = true;
            }
            else
            {
                ctx.Zadatci.Remove(zadatak);
                await ctx.SaveChangesAsync();

                TempData[Constants.Message] = $"Zadatak {id} obrisan.";
                TempData[Constants.ErrorOccurred] = false;
            }
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Error deleting Zadatak");
            TempData[Constants.Message] = "Greška prilikom brisanja zadatka";
            TempData[Constants.ErrorOccurred] = true;
        }

        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

    }

	[HttpGet]
	public async Task<IActionResult> Edit(int id)
	{


		var zad = await ctx.Zadatci
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
			.SingleOrDefaultAsync();

		ZadatakDTO d = new ZadatakDTO();

		if (zad != null)
		{
			await PrepareDropDownLists();
			return PartialView(zad);
		}
		else
		{
			return NotFound($"Neispravan ID zadatka: {id}");
		}
	}

	[HttpPost]
    public async Task<IActionResult> Edit(ZadatakDTOViewModel zadatak)
    {
        Console.WriteLine(zadatak);

        int idPrioriteta = ctx.Prioriteti
            .Where(z => z.Naziv == zadatak.IdPrioritetaNavigation.Naziv)
            .Select(z => z.IdPrioriteta)
            .FirstOrDefault();

        int idStatusa = ctx.Statusi
            .Where(z => z.Naziv == zadatak.IdStatusNavigation.Naziv)
            .Select(z => z.IdStatus)
            .FirstOrDefault();

        int idZahtjeva = ctx.Zahtjevi
            .Where(z => z.Naslov == zadatak.IdZahtjevaNavigation.Naslov)
            .Select(z => z.IdPrioriteta)
            .FirstOrDefault();

        string[] dijelovi = zadatak.OibosobaNavigation.Ime.Split(' ');

        int oibic = ctx.Osobe
            .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
            .Select(z => z.Oibosoba)
            .FirstOrDefault();

        bool checkId = await ctx.Zadatci.AnyAsync(m => m.IdZadatka == zadatak.IdZadatka);

        if (!checkId)
        {
            return NotFound($"Neispravan ID zadatka: {zadatak?.IdZadatka}");
        }

        if (ModelState.IsValid)
        {
            try
            {

                Zadatak doka = new Zadatak(zadatak.IdZadatka, zadatak.ImeZadatka, zadatak.PlaniraniPocetak,
                    zadatak.PlaniraniZavrsetak, zadatak.StvarniPocetak, zadatak.StvarniZavrsetak, idZahtjeva, idStatusa,
                    oibic, idPrioriteta);

                ctx.Update(doka);
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                await PrepareDropDownLists();
                return PartialView(zadatak);
            }
        }
        else
        {
            await PrepareDropDownLists();
            return PartialView(zadatak);
        }
    }
}