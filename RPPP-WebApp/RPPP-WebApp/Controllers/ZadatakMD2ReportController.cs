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

public class ZadatakMD2ReportController : Controller
{
    private readonly DBContext ctx;
    private readonly IWebHostEnvironment environment;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ZadatakMD2ReportController(DBContext ctx, IWebHostEnvironment environment)
    {
        this.ctx = ctx;
        this.environment = environment;
    }

    public async Task<IActionResult> ZadaciMD2()
    {
        var zadaci = await ctx.Zadatci
            .AsNoTracking()
            .OrderBy(d => d.ImeZadatka).Include(zadatak => zadatak.IdZahtjevaNavigation)
            .Include(zadatak => zadatak.IdStatusNavigation).Include(zadatak => zadatak.OibosobaNavigation)
            .Include(zadatak => zadatak.IdPrioritetaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis Zadataka i Evidencije Radova";
            excel.Workbook.Properties.Author = "Luka Lulić";
            var worksheet = excel.Workbook.Worksheets.Add("Zadataci");

            worksheet.Cells[1, 1].Value = "Naziv Zadatka";
            worksheet.Cells[1, 2].Value = "Planirani Početak";
            worksheet.Cells[1, 3].Value = "Planirani Završetak";
            worksheet.Cells[1, 4].Value = "Stvarni Početak";
            worksheet.Cells[1, 5].Value = "Stvarni Završetak";
            worksheet.Cells[1, 6].Value = "Zahtjev";
            worksheet.Cells[1, 7].Value = "Status";
            worksheet.Cells[1, 8].Value = "Osoba";
            worksheet.Cells[1, 9].Value = "Prioritet";
            worksheet.Cells[1, 10].Value = "Evidencija Radova";

            for (int i = 0; i < zadaci.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = zadaci[i].ImeZadatka;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = zadaci[i].PlaniraniPocetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 3].Value = zadaci[i].PlaniraniZavrsetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 4].Value = zadaci[i].StvarniPocetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 5].Value = zadaci[i].StvarniZavrsetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 6].Value = zadaci[i].IdZahtjevaNavigation.Naslov;
                worksheet.Cells[i + 2, 7].Value = zadaci[i].IdStatusNavigation.Naziv;
                worksheet.Cells[i + 2, 8].Value =
                    zadaci[i].OibosobaNavigation.Ime + " " + zadaci[i].OibosobaNavigation.Prezime;
                worksheet.Cells[i + 2, 9].Value = zadaci[i].IdPrioritetaNavigation.Naziv;

                List<EvidencijaRada> radoviZad =
                    ctx.EvidencijaRada.Where(d => d.IdZadatka == zadaci[i].IdZadatka).AsNoTracking().ToList();
                StringBuilder builderic = new StringBuilder();
                foreach (var VARIABLE in radoviZad)
                {
                    builderic.Append(VARIABLE.Opis + ", ");
                }

                if (builderic.ToString().Contains(", "))
                {
                    builderic.Remove(builderic.Length - 2, 2);
                }

                worksheet.Cells[i + 2, 10].Value = builderic;
            }

            worksheet.Cells[1, 1, zadaci.Count + 1, 9].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "zadaciMD.xlsx");
    }

    public async Task<IActionResult> Zadaci()
    {
        var zadaci = await ctx.Zadatci
            .AsNoTracking()
            .OrderBy(d => d.ImeZadatka).Include(zadatak => zadatak.IdZahtjevaNavigation)
            .Include(zadatak => zadatak.IdStatusNavigation).Include(zadatak => zadatak.OibosobaNavigation)
            .Include(zadatak => zadatak.IdPrioritetaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis Zadataka";
            excel.Workbook.Properties.Author = "Luka Lulić";
            var worksheet = excel.Workbook.Worksheets.Add("Zadataci");

            worksheet.Cells[1, 1].Value = "Naziv Zadatka";
            worksheet.Cells[1, 2].Value = "Planirani Početak";
            worksheet.Cells[1, 3].Value = "Planirani Završetak";
            worksheet.Cells[1, 4].Value = "Stvarni Početak";
            worksheet.Cells[1, 5].Value = "Stvarni Završetak";
            worksheet.Cells[1, 6].Value = "Zahtjev";
            worksheet.Cells[1, 7].Value = "Status";
            worksheet.Cells[1, 8].Value = "Osoba";
            worksheet.Cells[1, 9].Value = "Prioritet";

            for (int i = 0; i < zadaci.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = zadaci[i].ImeZadatka;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = zadaci[i].PlaniraniPocetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 3].Value = zadaci[i].PlaniraniZavrsetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 4].Value = zadaci[i].StvarniPocetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 5].Value = zadaci[i].StvarniZavrsetak.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 6].Value = zadaci[i].IdZahtjevaNavigation.Naslov;
                worksheet.Cells[i + 2, 7].Value = zadaci[i].IdStatusNavigation.Naziv;
                worksheet.Cells[i + 2, 8].Value =
                    zadaci[i].OibosobaNavigation.Ime + " " + zadaci[i].OibosobaNavigation.Prezime;
                worksheet.Cells[i + 2, 9].Value = zadaci[i].IdPrioritetaNavigation.Naziv;
            }

            worksheet.Cells[1, 1, zadaci.Count + 1, 9].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "Zadaci.xlsx");
    }

    public async Task<IActionResult> EvidencijaRadova()
    {
        var radovi = await ctx.EvidencijaRada
            .AsNoTracking()
            .OrderBy(d => d.Opis).Include(evidencijaRada => evidencijaRada.OibosobaNavigation)
            .Include(evidencijaRada => evidencijaRada.IdZadatkaNavigation)
            .Include(evidencijaRada => evidencijaRada.IdVrstePoslaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis Zadataka";
            excel.Workbook.Properties.Author = "Luka Lulić";
            var worksheet = excel.Workbook.Worksheets.Add("EvidenciajRadova");

            worksheet.Cells[1, 1].Value = "Opis Rada";
            worksheet.Cells[1, 2].Value = "Datum Rada";
            worksheet.Cells[1, 3].Value = "Vrijeme Rada";
            worksheet.Cells[1, 4].Value = "Osoba";
            worksheet.Cells[1, 5].Value = "Zadatak";
            worksheet.Cells[1, 6].Value = "Vrsta Posla";

            for (int i = 0; i < radovi.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = radovi[i].Opis;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = radovi[i].DatumRada.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 3].Value = radovi[i].VrijemeRada;
                worksheet.Cells[i + 2, 4].Value =
                    radovi[i].OibosobaNavigation.Ime + " " + radovi[i].OibosobaNavigation.Prezime;
                worksheet.Cells[i + 2, 5].Value = radovi[i].IdZadatkaNavigation.ImeZadatka;
                worksheet.Cells[i + 2, 6].Value = radovi[i].IdVrstePoslaNavigation.Naziv;
            }

            worksheet.Cells[1, 1, radovi.Count + 1, 6].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "evidencijaradova.xlsx");
    }

    public async Task<IActionResult> ZadaciPDF()
    {
        string naslov = "Popis Zadataka";

        var zadaci = await ctx.Zadatci
            .AsNoTracking()
            .OrderBy(d => d.ImeZadatka).Include(zadatak => zadatak.IdZahtjevaNavigation)
            .Include(zadatak => zadatak.IdStatusNavigation).Include(zadatak => zadatak.OibosobaNavigation)
            .Include(zadatak => zadatak.IdPrioritetaNavigation)
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

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zadaci));

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
                column.PropertyName<Zadatak>(x => x.IdZadatka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("Oznaka Zadatka");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.ImeZadatka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Naziv Zadatka", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.PlaniraniPocetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Planirani Početak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.PlaniraniZavrsetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Planirani Završetak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.StvarniPocetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Stvarni Početak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.StvarniZavrsetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Stvarni Završetak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.IdZahtjevaNavigation.Naslov);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Zahtjev", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.IdStatusNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Status", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.OibosobaNavigation.Ime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Ime Osobe", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.OibosobaNavigation.Prezime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Prezime Osobe", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.IdPrioritetaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Prioritet", horizontalAlignment: HorizontalAlignment.Center);
            });
        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=zadaci.pdf");
            return File(pdf, "application/pdf");
            //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
        }
        else
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> EvidencijaRadaPDF()
    {
        string naslov = "Popis Evidencije Rada";

        var radovi = await ctx.EvidencijaRada
            .AsNoTracking()
            .OrderBy(d => d.Opis).Include(evidencijaRada => evidencijaRada.OibosobaNavigation)
            .Include(evidencijaRada => evidencijaRada.IdZadatkaNavigation)
            .Include(evidencijaRada => evidencijaRada.IdVrstePoslaNavigation)
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

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(radovi));

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
                column.PropertyName<EvidencijaRada>(x => x.IdEvidencijaRad);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("Oznaka Rada");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<EvidencijaRada>(x => x.Opis);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Opis Rada", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<EvidencijaRada>(x => x.DatumRada);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Datum Rada", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<EvidencijaRada>(x => x.VrijemeRada);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Vrijeme Rada", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<EvidencijaRada>(x => x.OibosobaNavigation.Ime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Ime Osobe", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<EvidencijaRada>(x => x.OibosobaNavigation.Prezime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Prezime Osobe", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<EvidencijaRada>(x => x.IdZadatkaNavigation.ImeZadatka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Zadatak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<EvidencijaRada>(x => x.IdVrstePoslaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Vrsta Posla", horizontalAlignment: HorizontalAlignment.Center);
            });
        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=evidencijarada.pdf");
            return File(pdf, "application/pdf");
            //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
        }
        else
        {
            return NotFound();
        }
    }


    [HttpPost]
    public async Task<IActionResult> UveziZadatak(IFormFile excelFile)
    {
        List<Zadatak> zadacinovi = new List<Zadatak>();
        List<Zadatak> uspjeh = new List<Zadatak>();
        List<Zadatak> neuspjeh = new List<Zadatak>();
        List<String> broj = new List<String>();
        
        int counter = 0;
        if (excelFile == null || excelFile.Length == 0)
        {
           
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
                    
                    int rowStart = 2;
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = rowStart; row <= rowCount; row++)
                    {
                        
                        string naziv = worksheet.Cells[row, 1].Value?.ToString();
                        
                        
                        DateTime? planiraniPocetak;
                        try
                        {
                            planiraniPocetak = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                        }
                        catch (Exception e)
                        {
                             planiraniPocetak = null;
                        }
                        
                        DateTime? planiraniZavrsetak;
                        try
                        {
                            planiraniZavrsetak = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString());
                        }
                        catch (Exception e)
                        {
                            planiraniZavrsetak = null;
                        }
                        
                        DateTime? stvarniPocetak;
                        try
                        {
                            stvarniPocetak = DateTime.Parse(worksheet.Cells[row, 4].Value?.ToString());
                        }
                        catch (Exception e)
                        {
                            stvarniPocetak = null;
                        }
                        
                        DateTime? stvarniZavrsetak;
                        try
                        {
                            stvarniZavrsetak = DateTime.Parse(worksheet.Cells[row, 5].Value?.ToString());
                        }
                        catch (Exception e)
                        {
                            stvarniZavrsetak = null;
                        }
                        
                        string zahtjev = worksheet.Cells[row, 6].Value?.ToString();
                        string status = worksheet.Cells[row, 7].Value?.ToString();
                        string osoba = worksheet.Cells[row, 8].Value?.ToString();
                        string prioritet = worksheet.Cells[row, 9].Value?.ToString();
                        
                        int idPrioriteta = ctx.Prioriteti
                            .Where(z => z.Naziv == prioritet)
                            .Select(z => z.IdPrioriteta)
                            .FirstOrDefault();
                
                        int idStatusa = ctx.Statusi
                            .Where(z => z.Naziv == status)
                            .Select(z => z.IdStatus)
                            .FirstOrDefault();
                
                        int idZahtjeva = ctx.Zahtjevi
                            .Where(z => z.Naslov == zahtjev)
                            .Select(z => z.IdPrioriteta)
                            .FirstOrDefault();
                
                        int oibic = 0;
                        if (osoba != null)
                        {
                            string[] dijelovi = osoba.Split(' ');

                            oibic = ctx.Osobe
                                .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                                .Select(z => z.Oibosoba)
                                .FirstOrDefault();
                        }

                        if (idPrioriteta != 0 && idStatusa != 0 && idZahtjeva != 0 && oibic != 0 && naziv != null && planiraniPocetak != null && planiraniZavrsetak != null && stvarniPocetak != null && stvarniZavrsetak != null)
                        {
                            DateTime planiraniPocetaknovi = (DateTime)planiraniPocetak;
                            DateTime planiraniZavrsetaknovi = (DateTime)planiraniZavrsetak;
                            DateTime stvarniPocetaknovi = (DateTime)stvarniPocetak;
                            DateTime stvarniZavrsetaknovi = (DateTime)stvarniZavrsetak;
                            
                            Zadatak novi = new Zadatak(naziv, planiraniPocetaknovi, planiraniZavrsetaknovi, stvarniPocetaknovi,
                                stvarniZavrsetaknovi, idZahtjeva, idStatusa, oibic, idPrioriteta);
                            zadacinovi.Add(novi);
                            broj.Add("DODAN");
                        }
                        else
                        {
                            broj.Add("NIJE DODAN");
                        }
                        

                        
                    }
                }
            }

            foreach (var VARIABLE in zadacinovi)
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
                    Author = "Luka Lulić",
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
    public async Task<IActionResult> UveziEvidencijuRada(IFormFile excelFile2)
    {
        List<EvidencijaRada> zadacinovi = new List<EvidencijaRada>();
        List<EvidencijaRada> uspjeh = new List<EvidencijaRada>();
        List<EvidencijaRada> neuspjeh = new List<EvidencijaRada>();
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
                        
                        string opis = worksheet.Cells[row, 1].Value?.ToString();
                        
                        string osoba = worksheet.Cells[row, 4].Value?.ToString();
                        string zadatak = worksheet.Cells[row, 5].Value?.ToString();
                        string vrstaposla = worksheet.Cells[row, 6].Value?.ToString();
                        
                        int? vrijeme;
                        try
                        {
                            vrijeme = int.Parse(worksheet.Cells[row, 3].Value?.ToString());
                        } catch (Exception e)
                        {
                            vrijeme = null;
                        }
                        
                        DateTime? datumrada;
                        try
                        {
                            datumrada = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                        } catch (Exception e)
                        {
                            datumrada = null;
                        }
                        
                        int idZad = ctx.Zadatci
                            .Where(z => z.ImeZadatka == zadatak)
                            .Select(z => z.IdZadatka)
                            .FirstOrDefault();
                
                        int idVrstaPosla = ctx.VrstePosla
                            .Where(z => z.Naziv == vrstaposla)
                            .Select(z => z.IdVrstePosla)
                            .FirstOrDefault();


                        int oibic = 0;
                        if (osoba != null)
                        {
                            string[] dijelovi = osoba.Split(' ');

                            oibic = ctx.Osobe
                                .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                                .Select(z => z.Oibosoba)
                                .FirstOrDefault();
                        }
                        
                        

                        if (idZad != 0 && idVrstaPosla != 0 && oibic != 0 && opis != null && datumrada != null && vrijeme != null)
                        {
                            int vrijemenovi = (int)vrijeme;
                            DateTime datumradanovi = (DateTime)datumrada;
                            EvidencijaRada nova = new EvidencijaRada(opis, datumradanovi, vrijemenovi, oibic, idZad, idVrstaPosla);
                            zadacinovi.Add(nova);
                            broj.Add("DODAN");
                        }
                        else
                        {
                            broj.Add("NIJE DODAN");
                        }
                        

                        
                    }
                }
            }

            foreach (var VARIABLE in zadacinovi)
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