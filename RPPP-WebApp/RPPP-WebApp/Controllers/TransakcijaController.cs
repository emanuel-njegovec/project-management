using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using Microsoft.VisualBasic;
using RPPP_WebApp.ModelsValidation;

namespace RPPP_WebApp.Controllers
{
    public class TransakcijaController : Controller
    {
        private readonly DBContext ctx;
        private readonly AppSettings appSettings;
        TransakcijaValidator TransakcijaValidator = new TransakcijaValidator();

        public TransakcijaController(DBContext ctx, IOptionsSnapshot<AppSettings> options)
        {
            this.ctx = ctx;
            appSettings = options.Value;

        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.Transakcije.AsNoTracking();

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

            var transakcije = query
                          .Select(m => new TransakcijaViewModel
                          {
                              IdTransakcije = m.IdTransakcije,
                              Iznos = m.Iznos,
                              Iban2zaTransakciju = m.Iban2zaTransakciju,
                              Datum = m.Datum,
                              Vrijeme = m.Vrijeme,
                              IbanKartice = m.IdKarticeNavigation.Ibankartice,
                              Opis = m.IdVrsteTransakcijeNavigation.Opis
                          })
                          .Skip((page - 1) * pagesize)
                          .Take(pagesize)
                          .ToList();

            var model = new TransakcijeViewModel
            {
                Transakcije = transakcije,
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
        public async Task<IActionResult> Create(Transakcija transakcija)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(transakcija);
                    await ctx.SaveChangesAsync();

                    //TempData[Constants.Message] = $"Kartica s IBAN-om {kartica.Ibankartice} dodano. Id kartice = {kartica.IdKartice}";
                    //TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(transakcija);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(transakcija);
            }
        }


        private async Task PrepareDropDownLists()
        {
            var kartice = await ctx.ProjektneKartice
                                  .OrderBy(d => d.IdKartice)
                                  .Select(d => new SelectListItem
                                  {
                                      Value = d.IdKartice.ToString(),
                                      Text = d.Ibankartice.ToString()
                                  })
                                  .ToListAsync();

            var vrsteTransakcija = await ctx.VrsteTransakcije
                                  .OrderBy(d => d.IdVrsteTransakcije)
                                  .Select(d => new SelectListItem
                                  {
                                      Value = d.IdVrsteTransakcije.ToString(),
                                      Text = d.Opis
                                  })
                                  .ToListAsync();

            ViewBag.Kartice = new SelectList(kartice, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
            ViewBag.VrsteTransakcija = new SelectList(vrsteTransakcija, nameof(SelectListItem.Value), nameof(SelectListItem.Text));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            /*ActionResponseMessage responseMessage;
            var transakcija = await ctx.Transakcije.FindAsync(id);
            if (transakcija != null)
            {
                try
                {
                    //int iban = transakcija.Ibankartice;
                    ctx.Remove(transakcija);
                    await ctx.SaveChangesAsync();
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Kartica sa šifrom {id} uspješno obrisana.");
                }
                catch (Exception exc)
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja kartice: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Kartica sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return responseMessage.MessageType == MessageType.Success ?
              new EmptyResult() : await Get(id);
              */
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var transakcija = await ctx.Transakcije
                                  .AsNoTracking()
                                  .Where(m => m.IdTransakcije == id)
                                  .SingleOrDefaultAsync();
            if (transakcija != null)
            {
                await PrepareDropDownLists();
                return PartialView(transakcija);
            }
            else
            {
                return NotFound($"Neispravan id mjesta: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Transakcija transakcija)
        {
            if (transakcija == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await ctx.Transakcije.AnyAsync(m => m.IdTransakcije == transakcija.IdTransakcije);
            if (!checkId)
            {
                return NotFound($"Neispravan id mjesta: {transakcija?.IdTransakcije}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(transakcija);
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Get), new { id = transakcija.IdTransakcije });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return PartialView(transakcija);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return PartialView(transakcija);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var transakcija = await ctx.Transakcije
                                  .Where(m => m.IdTransakcije == id)
                                  .Select(m => new TransakcijaViewModel
                                  {
                                      IdTransakcije = m.IdTransakcije,
                                      Iznos = m.Iznos,
                                      Iban2zaTransakciju = m.Iban2zaTransakciju,
                                      Datum = m.Datum,
                                      Vrijeme = m.Vrijeme,
                                      IbanKartice = m.IdKarticeNavigation.Ibankartice,
                                      Opis = m.IdVrsteTransakcijeNavigation.Opis
                                  })
                                  .SingleOrDefaultAsync();
            if (transakcija != null)
            {
                return PartialView(transakcija);
            }
            else
            {
                return NotFound($"Neispravan id mjesta: {id}");
            }
        }
    }
}