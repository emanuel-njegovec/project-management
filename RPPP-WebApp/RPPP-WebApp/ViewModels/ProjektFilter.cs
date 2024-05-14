using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ProjektFilter : IPageFilter
{
    public int? IdProjekta { get; set; }

    public string Naziv { get; set; }

    public string Opis { get; set; }

    [DataType(DataType.Date)] public DateTime? DatumPocetka { get; set; }

    [DataType(DataType.Date)] public DateTime? DatumZavrsetka { get; set; }

    public int? Oibnarucitelj { get; set; }

    public int? IdVrsteProjekta { get; set; }

    public virtual VrstaProjekta IdVrsteProjektaNavigation { get; set; }

    public virtual Narucitelj OibnaruciteljNavigation { get; set; }
    
   

    public bool IsEmpty()
    {
        bool active = IdProjekta.HasValue
                      || DatumPocetka.HasValue
                      || DatumZavrsetka.HasValue
                      || Oibnarucitelj.HasValue
                      || IdVrsteProjekta.HasValue;
        return !active;
    }

    public override string ToString()
    {
        return string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}",
            IdProjekta,
            DatumPocetka,
            DatumZavrsetka,
            Opis,
            Naziv,
            Oibnarucitelj,
            IdVrsteProjekta);
    }

    public static ProjektFilter FromString(string s)
    {
        var filter = new ProjektFilter();

        if (!string.IsNullOrEmpty(s))
        {
            string[] args = s.Split('-');

            if (args.Length == 7)
            {
                filter.IdProjekta = string.IsNullOrWhiteSpace(args[0]) 
                    ? new int?() 
                    : int.Parse(args[0]);
                filter.DatumPocetka = string.IsNullOrWhiteSpace(args[1])
                    ? new DateTime()
                    : DateTime.ParseExact(args[1], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                filter.DatumZavrsetka = string.IsNullOrWhiteSpace(args[2])
                    ? new DateTime()
                    : DateTime.ParseExact(args[2], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                filter.Opis = string.IsNullOrWhiteSpace(args[3])
                    ? null
                    : args[3];
                filter.Naziv = string.IsNullOrWhiteSpace(args[4])
                    ? null
                    : args[4];
                filter.Oibnarucitelj = string.IsNullOrWhiteSpace(args[5]) 
                    ? new int?() 
                    : int.Parse(args[5]);
                filter.IdVrsteProjekta = string.IsNullOrWhiteSpace(args[6]) 
                    ? new int?() 
                    : int.Parse(args[6]);

            }
        }

        return filter;
    }

    public IQueryable<Projekt> Apply(IQueryable<Projekt> query)
    {
        if (IdProjekta.HasValue)
        {
            query = query.Where(d => d.IdProjekta == IdProjekta.Value);
        }

        if (DatumPocetka.HasValue)
        {
            query = query.Where(d => d.DatumPocetka >= DatumPocetka.Value);

        }
        
        if (DatumZavrsetka.HasValue)
        {
            query = query.Where(d => d.DatumZavrsetka <= DatumZavrsetka.Value);
        }
        
        if (!Opis.IsNullOrEmpty())
        {
            query = query.Where(d => d.Opis == Opis);
        }
        
        if (!Naziv.IsNullOrEmpty())
        {
            query = query.Where(d => d.Naziv == Naziv);
        }
        
        if (Oibnarucitelj.HasValue)
        {
            query = query.Where(d => d.Oibnarucitelj == Oibnarucitelj.Value);
        }
        
        if (IdVrsteProjekta.HasValue)
        {
            query = query.Where(d => d.IdVrsteProjekta == IdVrsteProjekta.Value);
        }

        return query;
    }
}