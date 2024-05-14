using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using RPPP_WebApp.ModelsValidation;

namespace RPPP_WebApp.Controllers
{
    public class ProjektnaKarticaController : Controller
    {
        private readonly DBContext ctx;
        private readonly AppSettings appSettings;
        ProjektnaKarticaValidator ProjektnaKarticaValidator = new ProjektnaKarticaValidator();

        public ProjektnaKarticaController(DBContext ctx, IOptionsSnapshot<AppSettings> options)
        {
            this.ctx = ctx;
            appSettings = options.Value;

        }
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;
            var query = ctx.ProjektneKartice.AsNoTracking();

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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjektnaKartica kartica)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(kartica);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Kartica s IBAN-om {kartica.Ibankartice} dodano. Id kartice = {kartica.IdKartice}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return View(kartica);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return View(kartica);
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

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var kartica = await ctx.ProjektneKartice.FindAsync(id);
            if (kartica != null)
            {
                try
                {
                    int iban = kartica.Ibankartice;
                    ctx.Remove(kartica);
                    await ctx.SaveChangesAsync();
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Kartica sa šifrom {id} i IBAN-om {iban.ToString()} uspješno obrisana.");
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
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kartica = await ctx.ProjektneKartice
                                  .AsNoTracking()
                                  .Where(m => m.IdKartice == id)
                                  .SingleOrDefaultAsync();
            if (kartica != null)
            {
                await PrepareDropDownLists();
                return PartialView(kartica);
            }
            else
            {
                return NotFound($"Neispravan id mjesta: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProjektnaKartica kartica)
        {
            if (kartica == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await ctx.ProjektneKartice.AnyAsync(m => m.IdKartice == kartica.IdKartice);
            if (!checkId)
            {
                return NotFound($"Neispravan id kartice: {kartica?.IdKartice}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(kartica);
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Get), new { id = kartica.IdKartice });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownLists();
                    return PartialView(kartica);
                }
            }
            else
            {
                await PrepareDropDownLists();
                return PartialView(kartica);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var kartica = await ctx.ProjektneKartice
                                  .Where(m => m.IdKartice == id)
                                  .Select(m => new ProjektnaKarticaViewModel
                                  {
                                      IdKartice = m.IdKartice,
                                      Ibankartice = m.Ibankartice,
                                      Stanje = m.Stanje,
                                      NazivProjekta = m.IdProjektaNavigation.Naziv,
                                      NazivOsobe = m.OibosobaNavigation.Ime + " " + m.OibosobaNavigation.Prezime
                                  })
                                  .SingleOrDefaultAsync();
            if (kartica != null)
            {
                return PartialView(kartica);
            }
            else
            {
                return NotFound($"Neispravan id mjesta: {id}");
            }
        }
    }
}