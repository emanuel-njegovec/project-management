using System.Drawing;
using System.Text;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Models;


namespace RPPP_WebApp.Controllers;

public class ProjektMD2ReportController : Controller
{
    private readonly DBContext ctx;
    private readonly IWebHostEnvironment environment;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ProjektMD2ReportController(DBContext ctx, IWebHostEnvironment environment)
    {
        this.ctx = ctx;
        this.environment = environment;
    }

    public async Task<IActionResult> ProjektiMD2()
    {
        var proj = await ctx.Projekti
            .AsNoTracking()
            .OrderBy(d => d.IdProjekta).Include(p => p.IdVrsteProjektaNavigation)
            .Include(p => p.OibnaruciteljNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis Projekata i Dokumenata";
            excel.Workbook.Properties.Author = "Viktorija Sturlic-Tupek";
            var worksheet = excel.Workbook.Worksheets.Add("Projekti");

            worksheet.Cells[1, 1].Value = "Naziv";
            worksheet.Cells[1, 2].Value = "DatumPocetka";
            worksheet.Cells[1, 3].Value = "DatumZavrsetka";
            worksheet.Cells[1, 4].Value = "Opis";
            worksheet.Cells[1, 5].Value = "Narucitelj";
            worksheet.Cells[1, 6].Value = "Vrsta";

            for (int i = 0; i < proj.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = proj[i].Naziv;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = proj[i].DatumPocetka.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 3].Value = proj[i].DatumZavrsetka.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 6].Value = proj[i].Opis;
                worksheet.Cells[i + 2, 7].Value = proj[i].OibnaruciteljNavigation.Naziv;
                worksheet.Cells[i + 2, 8].Value = proj[i].IdVrsteProjektaNavigation.Naziv;

                List<Dokument> projektici =
                    ctx.Dokumenti.Where(d => d.IdProjekta == proj[i].IdProjekta).AsNoTracking().ToList();
                StringBuilder builderBob = new StringBuilder();
                foreach (var VARIABLE in projektici)
                {
                    builderBob.Append(VARIABLE.DatumNastanka + ", ");
                }

                if (builderBob.ToString().Contains(", "))
                {
                    builderBob.Remove(builderBob.Length - 2, 2);
                }

                worksheet.Cells[i + 2, 8].Value = builderBob;
            }

            worksheet.Cells[1, 1, proj.Count + 1, 8].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "projektiMD.xlsx");
    }

    public async Task<IActionResult> Projekti()
    {
        var proj = await ctx.Projekti
            .AsNoTracking()
            .OrderBy(d => d.IdProjekta).Include(p => p.IdVrsteProjektaNavigation)
            .Include(p => p.OibnaruciteljNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis Projekata";
            excel.Workbook.Properties.Author = "Viktorija Sturlic-Tupek";
            var worksheet = excel.Workbook.Worksheets.Add("Projekti");

            worksheet.Cells[1, 1].Value = "Naziv";
            worksheet.Cells[1, 2].Value = "DatumPocetka";
            worksheet.Cells[1, 3].Value = "DatumZavrsetka";
            worksheet.Cells[1, 4].Value = "Opis";
            worksheet.Cells[1, 5].Value = "Narucitelj";
            worksheet.Cells[1, 6].Value = "Vrsta";


            for (int i = 0; i < proj.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = proj[i].Naziv;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = proj[i].DatumPocetka.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 3].Value = proj[i].DatumZavrsetka.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 6].Value = proj[i].Opis;
                worksheet.Cells[i + 2, 7].Value = proj[i].OibnaruciteljNavigation.Naziv;
                worksheet.Cells[i + 2, 8].Value = proj[i].IdVrsteProjektaNavigation.Naziv;

            }

            worksheet.Cells[1, 1, proj.Count + 1, 8].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "Projekti.xlsx");
    }

    public async Task<IActionResult> Dokumenti()
    {
        var doki = await ctx.Dokumenti
            .AsNoTracking()
            .OrderBy(d => d.DatumNastanka)
            .Include(d => d.IdProjektaNavigation)
            .Include(d => d.IdVrsteDokumentaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis Dokumenata";
            excel.Workbook.Properties.Author = "Viktorija Sturlic-Tupek";
            var worksheet = excel.Workbook.Worksheets.Add("Dokumenti");

            worksheet.Cells[1, 1].Value = "DatumNastanka";
            worksheet.Cells[1, 2].Value = "Format";
            worksheet.Cells[1, 3].Value = "Vrsta";
            worksheet.Cells[1, 4].Value = "Projekt";
            worksheet.Cells[1, 5].Value = "Stavka";

            for (int i = 0; i < doki.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = doki[i].DatumNastanka.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = doki[i].Format;
                worksheet.Cells[i + 2, 3].Value = doki[i].IdVrsteDokumentaNavigation.Naziv;
                worksheet.Cells[i + 2, 4].Value =
                    doki[i].IdProjektaNavigation.Naziv;
                worksheet.Cells[i + 2, 5].Value = doki[i].Stavka;
            }

            worksheet.Cells[1, 1, doki.Count + 1, 6].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "dokumenti.xlsx");
    }

    public async Task<IActionResult> ProjektiPDF()
    {
        string naslov = "Popis Projekata";

        var proj = await ctx.Projekti
            .AsNoTracking()
            .OrderBy(d => d.IdProjekta).Include(p => p.IdVrsteProjektaNavigation)
            .Include(p => p.OibnaruciteljNavigation)
            .ToListAsync();


        PdfReport report = CreateReport(naslov);

        #region Podnožje i zaglavlje

        report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });

        #endregion

        #region Postavljanje izvora podataka i stupaca

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(proj));

        report.MainTableColumns(columns =>
        {
            columns.AddColumn(column =>
            {
                column.IsRowNumber(true);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Order(0);
                column.Width(1);
                column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(x => x.IdProjekta);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("Oznaka Projekta");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(x => x.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Naziv Projekta", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(x => x.DatumPocetka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("DatumPocetka", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(x => x.DatumZavrsetka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("DatumZavrsetka", horizontalAlignment: HorizontalAlignment.Center);
            });
            
            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(x => x.Opis);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(x => x.OibnaruciteljNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Narucitelj", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Projekt>(x => x.IdVrsteProjektaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Vrsta", horizontalAlignment: HorizontalAlignment.Center);
            });
            
        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=projekti.pdf");
            return File(pdf, "application/pdf");
            //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
        }
        else
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> DokumentiPDF()
    {
        string naslov = "Popis Dokumenata";

        var doki = await ctx.Dokumenti
            .AsNoTracking()
            .OrderBy(d => d.DatumNastanka)
            .Include(d => d.IdProjektaNavigation)
            .Include(d => d.IdVrsteDokumentaNavigation)
            .ToListAsync();

        PdfReport report = CreateReport(naslov);

        #region Podnožje i zaglavlje

        report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });

        #endregion

        #region Postavljanje izvora podataka i stupaca

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(doki));

        report.MainTableColumns(columns =>
        {
            columns.AddColumn(column =>
            {
                column.IsRowNumber(true);
                column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                column.IsVisible(true);
                column.Order(0);
                column.Width(1);
                column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokument>(x => x.IdDokumenta);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("Oznaka Dokumenta");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokument>(x => x.DatumNastanka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("DatumNastanka", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokument>(x => x.Stavka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Stavka", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokument>(x => x.Format);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Format", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokument>(x => x.IdVrsteDokumentaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Vrsta", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Dokument>(x => x.IdProjektaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
            });
            
        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=dokumenti.pdf");
            return File(pdf, "application/pdf");
            //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
        }
        else
        {
            return NotFound();
        }
    }


    [HttpPost]
    public async Task<IActionResult> UveziProjekt(IFormFile excelFile)
    {
        List<Projekt> projektinovi = new List<Projekt>();
        List<Projekt> uspjeh = new List<Projekt>();
        List<Projekt> neuspjeh = new List<Projekt>();
        List<String> broj = new List<String>();
        int counter = 0;
        if (excelFile == null || excelFile.Length == 0)
        {
            // Prikazati poruku o grešci ako datoteka nije odabrana
            ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
            return View("Index");
        }

        try
        {
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    // Pretpostavljamo da su podaci u određenim redosledima, prilagodite prema stvarnom rasporedu u vašoj Excel datoteci
                    int rowStart = 2; // Preskačemo zaglavlje (prvi red)
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = rowStart; row <= rowCount; row++)
                    {
                        
                        string naziv = worksheet.Cells[row, 1].Value?.ToString();
                        DateTime datumPocetka = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                        DateTime datumZavrsetka = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString());
                        string opis = worksheet.Cells[row, 6].Value?.ToString();
                        string narucitelj = worksheet.Cells[row, 7].Value?.ToString();
                        string vrsta = worksheet.Cells[row, 8].Value?.ToString();
                       
                        
                        int idVrste = ctx.VrsteProjekta
                            .Where(z => z.Naziv == vrsta)
                            .Select(z => z.IdVrsteProjekta)
                            .FirstOrDefault();
                
                        int idNarucitelja = ctx.Narucitelji
                            .Where(z => z.Naziv == narucitelj)
                            .Select(z => z.Oibnarucitelj)
                            .FirstOrDefault();

                        if (idVrste != 0 && idNarucitelja != 0 && naziv != null)
                        {
                            Projekt novi = new Projekt(naziv, opis, datumPocetka, datumZavrsetka,
                                 idNarucitelja, idVrste);
                            projektinovi.Add(novi);
                            broj.Add("DODAN");
                        }
                        else
                        {
                            broj.Add("NIJE DODAN");
                        }
                        

                        
                    }
                }
            }

            foreach (var VARIABLE in projektinovi)
            {
                
                ctx.Add(VARIABLE);
                uspjeh.Add(VARIABLE);
                
            }
            await ctx.SaveChangesAsync();

            if (uspjeh.Count == broj.Count)
            {
                var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                var package = new ExcelPackage(stream);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                for (int i = 2; i <= worksheet.Dimension.Rows; i++) 
                {
                    worksheet.Cells[i, worksheet.Dimension.Columns + 1].Value = "DODAN";   
                }
                byte[] updatedContent = package.GetAsByteArray();
                Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                return File(updatedContent, ExcelContentType);
            }
            else
            {
                int counte2r = 2;
                var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                var package = new ExcelPackage(stream);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int brojkol = worksheet.Dimension.Columns + 1;
                foreach (var VARIABLE in broj)
                {
                    
                    worksheet.Cells[counte2r, brojkol].Value = VARIABLE;
                    counte2r = counte2r + 1;
                }
                
                byte[] updatedContent = package.GetAsByteArray();
                Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                return File(updatedContent, ExcelContentType);
            }
            
            ViewBag.Message = "Podaci uspešno uvezeni.";
            return View("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("excelFile", "Došlo je do greške prilikom uvoza podataka iz Excel datoteke.");
            return View("Index");
        }
    }



    private PdfReport CreateReport(string naslov)
    {
        var pdf = new PdfReport();

        pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "Viktorija Sturlic-Tupek",
                    Application = "RPPP",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            //fix za linux https://github.com/VahidN/PdfReport.Core/issues/40
            .DefaultFonts(fonts =>
            {
                fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                    Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
                fonts.Size(9);
                fonts.Color(System.Drawing.Color.Black);
            })
            //
            .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ProfessionalTemplate); })
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                //table.NumberOfDataRowsPerPage(20);
                table.GroupsPreferences(new GroupsPreferences
                {
                    GroupType = GroupType.HideGroupingColumns,
                    RepeatHeaderRowPerGroup = true,
                    ShowOneGroupPerPage = true,
                    SpacingBeforeAllGroupsSummary = 5f,
                    NewGroupAvailableSpacingThreshold = 150,
                    SpacingAfterAllGroupsSummary = 5f
                });
                table.SpacingAfter(4f);
            });

        return pdf;
    }

    public IActionResult Index()
    {
        return View();
    }

    
    [HttpPost]
    public async Task<IActionResult> UveziDokument(IFormFile excelFile2)
    {
        List<Dokument> dokinovi = new List<Dokument>();
        List<Dokument> uspjeh = new List<Dokument>();
        List<Dokument> neuspjeh = new List<Dokument>();
        List<String> broj = new List<String>();
        int counter = 0;
        if (excelFile2 == null || excelFile2.Length == 0)
        {
            
            ModelState.AddModelError("excelFile", "Odaberite Excel datoteku.");
            return View("Index");
        }

        try
        {
            using (var stream = new MemoryStream())
            {
                await excelFile2.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    
                    int rowStart = 2; 
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = rowStart; row <= rowCount; row++)
                    {
                        
                        DateTime datumNastanka = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                        string format = worksheet.Cells[row, 4].Value?.ToString();
                        string vrsta = worksheet.Cells[row, 5].Value?.ToString();
                        string projekt = worksheet.Cells[row, 6].Value?.ToString();
                        string stavka = worksheet.Cells[row, 7].Value?.ToString();
                        
                        int idVrste = ctx.VrsteDokumenta
                            .Where(z => z.Naziv == vrsta)
                            .Select(z => z.IdVrsteDokumenta)
                            .FirstOrDefault();
                
                        int idProjekt = ctx.Projekti
                            .Where(z => z.Naziv == projekt)
                            .Select(z => z.IdProjekta)
                            .FirstOrDefault();
                        

                        if (idVrste != 0 && idProjekt != 0 && datumNastanka != null)
                        {
                            Dokument nova = new Dokument(datumNastanka, stavka, format, idVrste, idProjekt);
                            dokinovi.Add(nova);
                            broj.Add("DODAN");
                        }
                        else
                        {
                            broj.Add("NIJE DODAN");
                        }
                        

                        
                    }
                }
            }

            foreach (var VARIABLE in dokinovi)
            {
                
                ctx.Add(VARIABLE);
                uspjeh.Add(VARIABLE);
                
            }
            await ctx.SaveChangesAsync();

            if (uspjeh.Count == broj.Count)
            {
                var stream = new MemoryStream();
                await excelFile2.CopyToAsync(stream);
                var package = new ExcelPackage(stream);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                for (int i = 2; i <= worksheet.Dimension.Rows; i++) 
                {
                    worksheet.Cells[i, worksheet.Dimension.Columns + 1].Value = "DODAN";   
                }
                byte[] updatedContent = package.GetAsByteArray();
                Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                return File(updatedContent, ExcelContentType);
            }
            else
            {
                int counte2r = 2;
                var stream = new MemoryStream();
                await excelFile2.CopyToAsync(stream);
                var package = new ExcelPackage(stream);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int brojkol = worksheet.Dimension.Columns + 1;
                foreach (var VARIABLE in broj)
                {
                    
                    worksheet.Cells[counte2r, brojkol].Value = VARIABLE;
                    counte2r = counte2r + 1;
                }
                
                byte[] updatedContent = package.GetAsByteArray();
                Response.Headers.Add("content-disposition", "inline; filename=updatedFile.xlsx");
                return File(updatedContent, ExcelContentType);
            }
            
            
            
            ViewBag.Message = "Podaci uspešno uvezeni.";
            return View("Index");
        }
        catch (Exception ex)
        {
           
            ModelState.AddModelError("excelFile", "Došlo je do greške prilikom uvoza podataka iz Excel datoteke.");
            return View("Index");
        }
    }
}