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

public class ProjektnaKarticaMDv2UvozIzvozController : Controller
{
    private readonly DBContext ctx;
    private readonly IWebHostEnvironment environment;
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ProjektnaKarticaMDv2UvozIzvozController(DBContext ctx, IWebHostEnvironment environment)
    {
        this.ctx = ctx;
        this.environment = environment;
    }

    public async Task<IActionResult> KarticeMD2()
    {
        var kartice = await ctx.ProjektneKartice
            .AsNoTracking()
            .OrderBy(d => d.IdKartice).Include(kartica => kartica.IdProjektaNavigation)
            .Include(kartica => kartica.OibosobaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis projektnih kartica i transakcija";
            excel.Workbook.Properties.Author = "Emanuel Njegovec";
            var worksheet = excel.Workbook.Worksheets.Add("ProjektneKartice");

            worksheet.Cells[1, 1].Value = "ID kartice";
            worksheet.Cells[1, 2].Value = "IBAN kartice";
            worksheet.Cells[1, 3].Value = "Stanje";
            worksheet.Cells[1, 4].Value = "Projekt";
            worksheet.Cells[1, 5].Value = "Zadužena osoba";
            worksheet.Cells[1, 6].Value = "Transakcije";

            for (int i = 0; i < kartice.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = kartice[i].IdKartice;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = kartice[i].Ibankartice;
                worksheet.Cells[i + 2, 3].Value = kartice[i].Stanje;
                worksheet.Cells[i + 2, 4].Value = kartice[i].IdProjektaNavigation.Naziv;
                worksheet.Cells[i + 2, 5].Value = kartice[i].OibosobaNavigation.Ime + ' ' + kartice[i].OibosobaNavigation.Prezime;

                List<Transakcija> transakcijeKartica =
                    ctx.Transakcije.Where(d => d.IdKartice == kartice[i].IdKartice).AsNoTracking().ToList();
                StringBuilder blder = new StringBuilder();
                foreach (var transakcija in transakcijeKartica)
                {
                    blder.Append(transakcija.Iznos + ", ");
                }

                if (blder.ToString().Contains(", "))
                {
                    blder.Remove(blder.Length - 2, 2);
                }

                worksheet.Cells[i + 2, 6].Value = blder;
            }

            worksheet.Cells[1, 1, kartice.Count + 1, 6].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "ProjektneKarticeMD.xlsx");
    }

    public async Task<IActionResult> ProjektneKartice()
    {
        var kartice = await ctx.ProjektneKartice
            .AsNoTracking()
            .OrderBy(d => d.IdKartice).Include(kartica => kartica.IdProjektaNavigation)
            .Include(kartica => kartica.OibosobaNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis projektnih kartica";
            excel.Workbook.Properties.Author = "Emanuel Njegovec";
            var worksheet = excel.Workbook.Worksheets.Add("ProjektneKartice");

            worksheet.Cells[1, 1].Value = "ID kartice";
            worksheet.Cells[1, 2].Value = "IBAN kartice";
            worksheet.Cells[1, 3].Value = "Stanje";
            worksheet.Cells[1, 4].Value = "Projekt";
            worksheet.Cells[1, 5].Value = "Zadužena osoba";

            for (int i = 0; i < kartice.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = kartice[i].IdKartice;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = kartice[i].Ibankartice;
                worksheet.Cells[i + 2, 3].Value = kartice[i].Stanje;
                worksheet.Cells[i + 2, 4].Value = kartice[i].IdProjektaNavigation.Naziv;
                worksheet.Cells[i + 2, 5].Value = kartice[i].OibosobaNavigation.Ime + ' ' + kartice[i].OibosobaNavigation.Prezime;
            }

            worksheet.Cells[1, 1, kartice.Count + 1, 5].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "ProjektneKartice.xlsx");
    }

    public async Task<IActionResult> Transakcije()
    {
        var transakcije = await ctx.Transakcije
            .AsNoTracking()
            .OrderBy(d => d.IdTransakcije).Include(transakcija => transakcija.IdKarticeNavigation)
            .Include(transakcija => transakcija.IdVrsteTransakcijeNavigation)
            .ToListAsync();


        byte[] content;

        using (ExcelPackage excel = new ExcelPackage())
        {
            excel.Workbook.Properties.Title = "Popis svih transakcija";
            excel.Workbook.Properties.Author = "Emanuel Njegovec";
            var worksheet = excel.Workbook.Worksheets.Add("Transakcije");

            worksheet.Cells[1, 1].Value = "ID transakcije";
            worksheet.Cells[1, 2].Value = "Iznos";
            worksheet.Cells[1, 3].Value = "drugi IBAN za transakciju";
            worksheet.Cells[1, 4].Value = "Datum";
            worksheet.Cells[1, 5].Value = "Vrijeme";
            worksheet.Cells[1, 6].Value = "Projektna kartica";
            worksheet.Cells[1, 7].Value = "Vrsta transakcije";

            for (int i = 0; i < transakcije.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = transakcije[i].IdTransakcije;
                worksheet.Cells[i + 2, 1].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 2, 2].Value = transakcije[i].Iznos;
                worksheet.Cells[i + 2, 3].Value = transakcije[i].Iban2zaTransakciju;
                worksheet.Cells[i + 2, 4].Value = transakcije[i].Datum.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 5].Value = transakcije[i].Vrijeme.ToString(@"hh\:mm\:ss");
                worksheet.Cells[i + 2, 6].Value = transakcije[i].IdKarticeNavigation.Ibankartice;
                worksheet.Cells[i + 2, 7].Value = transakcije[i].IdVrsteTransakcijeNavigation.Opis;
            }

            worksheet.Cells[1, 1, transakcije.Count + 1, 7].AutoFitColumns();

            content = excel.GetAsByteArray();
        }

        return File(content, ExcelContentType, "Transakcije.xlsx");
    }

    public async Task<IActionResult> ProjektneKarticePDF()
    {
        string naslov = "Popis projektnih kartica";

        var kartice = await ctx.ProjektneKartice
            .AsNoTracking()
            .OrderBy(d => d.IdKartice).Include(kartica => kartica.IdProjektaNavigation)
            .Include(kartica => kartica.OibosobaNavigation)
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

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(kartice));

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
                column.PropertyName<ProjektnaKartica>(x => x.IdKartice);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("ID kartice");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<ProjektnaKartica>(x => x.Ibankartice);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("IBAN kartice", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<ProjektnaKartica>(x => x.Stanje);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Stanje", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<ProjektnaKartica>(x => x.IdProjektaNavigation.Naziv);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<ProjektnaKartica>(x => x.OibosobaNavigation.Ime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Ime osobe", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<ProjektnaKartica>(x => x.OibosobaNavigation.Prezime);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Prezime osobe", horizontalAlignment: HorizontalAlignment.Center);
            });
        });

        #endregion

        byte[] pdf = report.GenerateAsByteArray();

        if (pdf != null)
        {
            Response.Headers.Add("content-disposition", "inline; filename=kartice.pdf");
            return File(pdf, "application/pdf");
            //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
        }
        else
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> TransakcijePDF()
    {
        string naslov = "Popis transakcija";

        var transakcije = await ctx.Transakcije
            .AsNoTracking()
            .OrderBy(d => d.IdTransakcije).Include(transakcija => transakcija.IdKarticeNavigation)
            .Include(transakcija => transakcija.IdVrsteTransakcijeNavigation)
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

        report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(transakcije));

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
                column.PropertyName<Transakcija>(x => x.IdTransakcije);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(1);
                column.Width(2);
                column.HeaderCell("ID transakcije");
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Transakcija>(x => x.Iznos);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Iznos", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Transakcija>(x => x.Iban2zaTransakciju);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("IBAN2 za transakciju", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Transakcija>(x => x.Datum);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Datum", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Transakcija>(x => x.Vrijeme);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Vrijeme", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Transakcija>(x => x.IdKarticeNavigation.Ibankartice);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("IBAN kartice", horizontalAlignment: HorizontalAlignment.Center);
            });

            columns.AddColumn(column =>
            {
                column.PropertyName<Transakcija>(x => x.IdVrsteTransakcijeNavigation.Opis);
                column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                column.IsVisible(true);
                column.Order(2);
                column.Width(3);
                column.HeaderCell("Vrsta transakcije", horizontalAlignment: HorizontalAlignment.Center);
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

                        int oibic = 0;
                        if (osoba != null)
                        {
                            string[] dijelovi = osoba.Split(' ');

                            oibic = ctx.Osobe
                                .Where(z => z.Ime == dijelovi[0] && z.Prezime == dijelovi[1])
                                .Select(z => z.Oibosoba)
                                .FirstOrDefault();
                        }

                        if (idPrioriteta != 0 && idStatusa != 0 && idZahtjeva != 0 && oibic != 0 && naziv != null)
                        {
                            Zadatak novi = new Zadatak(naziv, planiraniPocetak, planiraniZavrsetak, stvarniPocetak,
                                stvarniZavrsetak, idZahtjeva, idStatusa, oibic, idPrioriteta);
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
                        DateTime datumrada = DateTime.Parse(worksheet.Cells[row, 2].Value?.ToString());
                        int vrijeme = int.Parse(worksheet.Cells[row, 3].Value?.ToString());
                        string osoba = worksheet.Cells[row, 4].Value?.ToString();
                        string zadatak = worksheet.Cells[row, 5].Value?.ToString();
                        string vrstaposla = worksheet.Cells[row, 6].Value?.ToString();

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



                        if (idZad != 0 && idVrstaPosla != 0 && oibic != 0 && opis != null)
                        {
                            EvidencijaRada nova = new EvidencijaRada(opis, datumrada, vrijeme, oibic, idZad, idVrstaPosla);
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
