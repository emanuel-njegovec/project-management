using System.Drawing;
using System.Net.NetworkInformation;
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

public class ZahtjevSTPReportController : Controller
{
    private readonly DBContext ctx;
    private readonly IWebHostEnvironment environment;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ZahtjevSTPReportController(DBContext ctx, IWebHostEnvironment environment)
    {
        this.ctx = ctx;
        this.environment = environment;
    }

    public async Task<IActionResult> ZahtjeviSTP()
    {
        var zahtjevi = await ctx.Zahtjevi
            .AsNoTracking()
            .OrderBy(d => d.Naslov)
            .Include(z => z.IdVrsteZahtjevaNavigation)
            .Include(z => z.IdPrioritetaNavigation)
            .Include(z => z.IdProjektaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis zahtjeva";
            excel.Workbook.Properties.Author = "Ena Samaržija";
            var worksheet = excel.Workbook.Worksheets.Add("Zahtjevi");

            worksheet.Cells[1, 1].Value = "Naziv zahtjeva";
            worksheet.Cells[1, 2].Value = "Opis";
            worksheet.Cells[1, 3].Value = "Vrsta zahtjeva";
            worksheet.Cells[1, 4].Value = "Prioritet";
            worksheet.Cells[1, 5].Value = "Projekt";
            worksheet.Cells[1, 6].Value = "Zadaci";

            for (int i = 0; i < zahtjevi.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = zahtjevi[i].Naslov;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = zahtjevi[i].Opis;
                worksheet.Cells[i + 2, 3].Value = zahtjevi[i].IdVrsteZahtjevaNavigation.Naziv;
                worksheet.Cells[i + 2, 4].Value = zahtjevi[i].IdPrioritetaNavigation.Naziv;
                worksheet.Cells[i + 2, 5].Value = zahtjevi[i].IdProjektaNavigation.Naziv;

                List<Zadatak> zadaciZahtjeva =
                    ctx.Zadatci.Where(d => d.IdZahtjeva == zahtjevi[i].IdZahtjeva).AsNoTracking().ToList();
                StringBuilder sb = new StringBuilder();
                foreach (var var in zadaciZahtjeva)
                {
                    sb.Append(var.ImeZadatka + ", ");
                }

                if (sb.ToString().Contains(", "))
                {
                    sb.Remove(sb.Length - 2, 2);
                }

                worksheet.Cells[i + 2, 6].Value = sb;
            }

            worksheet.Cells[1, 1, zahtjevi.Count + 1, 5].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "zahtjevi.xlsx");
    }

    public async Task<IActionResult> Zahtjevi()
    {
        var zahtjevi = await ctx.Zahtjevi
            .AsNoTracking()
            .OrderBy(d => d.Naslov)
            .Include(z => z.IdVrsteZahtjevaNavigation)
            .Include(z => z.IdPrioritetaNavigation)
            .Include(z => z.IdProjektaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis zahtjeva";
            excel.Workbook.Properties.Author = "Ena Samaržija";
            var worksheet = excel.Workbook.Worksheets.Add("Zahtjevi");

            worksheet.Cells[1, 1].Value = "Naziv zahtjeva";
            worksheet.Cells[1, 2].Value = "Opis";
            worksheet.Cells[1, 3].Value = "Vrsta zahtjeva";
            worksheet.Cells[1, 4].Value = "Prioritet";
            worksheet.Cells[1, 5].Value = "Projekt";

            for (int i = 0; i < zahtjevi.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = zahtjevi[i].Naslov;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = zahtjevi[i].Opis;
                worksheet.Cells[i + 2, 3].Value = zahtjevi[i].IdVrsteZahtjevaNavigation.Naziv;
                worksheet.Cells[i + 2, 4].Value = zahtjevi[i].IdPrioritetaNavigation.Naziv;
                worksheet.Cells[i + 2, 5].Value = zahtjevi[i].IdProjektaNavigation.Naziv;
            }

            worksheet.Cells[1, 1, zahtjevi.Count + 1, 5].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "Zahtjevi.xlsx");
    }

    public async Task<IActionResult> Zadatak()
    {
        var zadaci = await ctx.Zadatci
            .AsNoTracking()
            .OrderBy(d => d.ImeZadatka)
            .Include(zadatak => zadatak.IdZahtjevaNavigation)
            .Include(zadatak => zadatak.IdStatusNavigation)
            .Include(zadatak => zadatak.OibosobaNavigation)
            .Include(zadatak => zadatak.IdPrioritetaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis zadataka";
            excel.Workbook.Properties.Author = "Ena Samaržija";
            var worksheet = excel.Workbook.Worksheets.Add("Zadatak");

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

        return File(content, ExcelContentType, "zadaci.xlsx");
    }

    public async Task<IActionResult> ZahtjeviPDF()
    {
        string naslov = "Popis zahtjeva";

        var zahtjevi = await ctx.Zahtjevi
            .AsNoTracking()
            .OrderBy(d => d.Naslov)
            .Include(z => z.IdVrsteZahtjevaNavigation)
            .Include(z => z.IdPrioritetaNavigation)
            .Include(z => z.IdProjektaNavigation)
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

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zahtjevi));

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
                column.PropertyName<Zahtjev>(x => x.IdZahtjeva);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("Id zahtjeva");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zahtjev>(x => x.Naslov);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Naziv zahtjeva", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zahtjev>(x => x.Opis);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zahtjev>(x => x.IdVrsteZahtjevaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Vrsta zahtjeva", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zahtjev>(x => x.IdPrioritetaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Prioritet", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zahtjev>(x => x.IdProjektaNavigation.Naziv);
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
            Response.Headers.Add("content-disposition", "inline; filename=zahtjevi.pdf");
            return File(pdf, "application/pdf");
            //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
        }
        else
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> ZadatakPDF()
    {
        string naslov = "Popis zadataka";

        var zadaci = await ctx.Zadatci
            .AsNoTracking()
            .OrderBy(d => d.ImeZadatka)
            .Include(zadatak => zadatak.IdZahtjevaNavigation)
            .Include(zadatak => zadatak.IdStatusNavigation)
            .Include(zadatak => zadatak.OibosobaNavigation)
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
                column.HeaderCell("Id zadatka");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.ImeZadatka);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Naziv zadatka", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.PlaniraniPocetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Planirani početak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.PlaniraniZavrsetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Planirani završetak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.StvarniPocetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Stvarni početak", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.StvarniZavrsetak);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Stvarni završetak", horizontalAlignment: HorizontalAlignment.Center);
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
                column.HeaderCell("Ime osobe", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Zadatak>(x => x.OibosobaNavigation.Prezime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Prezime osobe", horizontalAlignment: HorizontalAlignment.Center);
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
    public async Task<IActionResult> UveziZahtjev(IFormFile excelFile)
    {
        List<Zahtjev> noviZahtjevi = new List<Zahtjev>();
        List<Zahtjev> uspjeh = new List<Zahtjev>();
        List<Zahtjev> neuspjeh = new List<Zahtjev>();
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
                        string opis = worksheet.Cells[row, 2].Value?.ToString();
                        string vrstaZahtjeva = worksheet.Cells[row, 3].Value?.ToString();
                        string prioritet = worksheet.Cells[row, 4].Value?.ToString();
                        string projekt = worksheet.Cells[row, 5].Value?.ToString();


                        int idVrsteZahtjeva = ctx.VrsteZahtjeva
                            .Where(z => z.Naziv == vrstaZahtjeva)
                            .Select(z => z.IdVrsteZahtjeva)
                            .FirstOrDefault();

                        int idPrioriteta = ctx.Prioriteti
                            .Where(z => z.Naziv == prioritet)
                            .Select(z => z.IdPrioriteta)
                            .FirstOrDefault();

                        int idProjekta = ctx.Projekti
                            .Where(z => z.Naziv == projekt)
                            .Select(z => z.IdProjekta)
                        .FirstOrDefault();


                        if (idVrsteZahtjeva != 0 && idPrioriteta != 0 && idProjekta != 0 &&  naziv != null)
                        {
                            Zahtjev novi = new Zahtjev(naziv, opis, idVrsteZahtjeva, idPrioriteta, idProjekta);
                            noviZahtjevi.Add(novi);
                            broj.Add("Dodano");
                        }
                        else
                        {
                            broj.Add("Nije dodano");
                        }



                    }
                }
            }

            foreach (var var in noviZahtjevi)
            {

                ctx.Add(var);
                uspjeh.Add(var);

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
                    worksheet.Cells[i, worksheet.Dimension.Columns + 1].Value = "Dodano";
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
                foreach (var var in broj)
                {

                    worksheet.Cells[counte2r, brojkol].Value = var;
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
                Author = "Ena Samaržija",
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
    public async Task<IActionResult> UveziZadatak(IFormFile excelFile2)
    {
        List<Zadatak> noviZadaci = new List<Zadatak>();
        List<Zadatak> uspjeh = new List<Zadatak>();
        List<Zadatak> neuspjeh = new List<Zadatak>();
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

                        string naziv = worksheet.Cells[row, 1].Value?.ToString();
                        DateTime planiraniPocetak = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                        DateTime planiraniZavrsetak = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString());
                        DateTime stvarniPocetak = DateTime.Parse(worksheet.Cells[row, 4].Value?.ToString());
                        DateTime stvarniZavrsetak = DateTime.Parse(worksheet.Cells[row, 5].Value?.ToString());
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

                        int oib = 0;
                        if (osoba != null)
                        {
                            string[] dijelovi = osoba.Split(' ');

                            oib = ctx.Osobe
                                .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                                .Select(z => z.Oibosoba)
                                .FirstOrDefault();
                        }

                        if (idPrioriteta != 0 && idStatusa != 0 && idZahtjeva != 0 && oib != 0 && naziv != null)
                        {
                            Zadatak novi = new Zadatak(naziv, planiraniPocetak, planiraniZavrsetak, stvarniPocetak,
                                stvarniZavrsetak, idZahtjeva, idStatusa, oib, idPrioriteta);
                            noviZadaci.Add(novi);
                            broj.Add("Dodano");
                        }
                        else
                        {
                            broj.Add("Nije dodano");
                        }
                    }
                }
            }

            foreach (var var in noviZadaci)
            {

                ctx.Add(var);
                uspjeh.Add(var);

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
                    worksheet.Cells[i, worksheet.Dimension.Columns + 1].Value = "Dodano";
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
                foreach (var var in broj)
                {

                    worksheet.Cells[counte2r, brojkol].Value = var;
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