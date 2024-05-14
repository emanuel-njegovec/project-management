using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ZadatakFilter : IPageFilter
{
    public int? IdZadatka { get; set; }

    public string ImeZadatka { get; set; }

    [DataType(DataType.Date)] public DateTime? DatumOdPlan { get; set; }

    [DataType(DataType.Date)] public DateTime? DatumDoPlan { get; set; }

    [DataType(DataType.Date)] public DateTime? DatumOdStvar { get; set; }

    [DataType(DataType.Date)] public DateTime? DatumDoStvar { get; set; }

    public int? IdZahtjeva { get; set; }

    public int? IdStatus { get; set; }

    public int? Oibosoba { get; set; }

    public int? IdPrioriteta { get; set; }
    
    public virtual Prioritet IdPrioritetaNavigation { get; set; }

    public virtual Status IdStatusNavigation { get; set; }

    public virtual Zahtjev IdZahtjevaNavigation { get; set; }

    public virtual Osoba OibosobaNavigation { get; set; }

    public bool IsEmpty()
    {
        bool active = IdZadatka.HasValue
                      || DatumOdPlan.HasValue
                      || DatumDoPlan.HasValue
                      || DatumOdStvar.HasValue
                      || DatumDoStvar.HasValue
                      || IdZahtjeva.HasValue
                      || IdStatus.HasValue
                      || Oibosoba.HasValue
                      || IdPrioriteta.HasValue;
        return !active;
    }

    public override string ToString()
    {
        return string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}",
            IdZadatka,
            DatumOdPlan?.ToString("dd.MM.yyyy"),
            DatumDoPlan?.ToString("dd.MM.yyyy"),
            DatumOdStvar?.ToString("dd.MM.yyyy"),
            DatumDoStvar?.ToString("dd.MM.yyyy"),
            IdZahtjeva,
            IdStatus,
            Oibosoba,
            IdPrioriteta);
    }

    public static ZadatakFilter FromString(string s)
    {
        var filter = new ZadatakFilter();
        if (!string.IsNullOrEmpty(s))
        {
            string[] arr = s.Split('-', StringSplitOptions.None);

            if (arr.Length == 5)
            {
                filter.IdZadatka = string.IsNullOrWhiteSpace(arr[0]) ? new int?() : int.Parse(arr[0]);
                filter.DatumOdPlan = string.IsNullOrWhiteSpace(arr[1])
                    ? new DateTime?()
                    : DateTime.ParseExact(arr[1], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                filter.DatumDoPlan = string.IsNullOrWhiteSpace(arr[2])
                    ? new DateTime?()
                    : DateTime.ParseExact(arr[2], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                filter.DatumOdStvar = string.IsNullOrWhiteSpace(arr[2])
                    ? new DateTime?()
                    : DateTime.ParseExact(arr[3], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                filter.DatumDoStvar = string.IsNullOrWhiteSpace(arr[4])
                    ? new DateTime?()
                    : DateTime.ParseExact(arr[4], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                filter.IdZahtjeva = string.IsNullOrWhiteSpace(arr[5]) ? new int?() : int.Parse(arr[5]);
                filter.IdStatus = string.IsNullOrWhiteSpace(arr[6]) ? new int?() : int.Parse(arr[6]);
                filter.Oibosoba = string.IsNullOrWhiteSpace(arr[7]) ? new int?() : int.Parse(arr[7]);
                filter.IdPrioriteta = string.IsNullOrWhiteSpace(arr[8]) ? new int?() : int.Parse(arr[8]);
            }
        }

        return filter;
    }

    public IQueryable<Zadatak> Apply(IQueryable<Zadatak> query)
    {
        if (IdZadatka.HasValue)
        {
            query = query.Where(d => d.IdZadatka == IdZadatka.Value);
        }

        if (DatumOdPlan.HasValue)
        {
            query = query.Where(d => d.PlaniraniPocetak >= DatumOdPlan.Value);
        }

        if (DatumDoPlan.HasValue)
        {
            query = query.Where(d => d.PlaniraniZavrsetak <= DatumDoPlan.Value);
        }

        if (DatumOdStvar.HasValue)
        {
            query = query.Where(d => d.StvarniPocetak >= DatumOdStvar.Value);
        }

        if (DatumDoStvar.HasValue)
        {
            query = query.Where(d => d.StvarniZavrsetak <= DatumDoStvar.Value);
        }
        
        if (IdZahtjeva.HasValue)
        {
            query = query.Where(d => d.IdZahtjeva == IdZahtjeva.Value);
        }
        
        if (IdStatus.HasValue)
        {
            query = query.Where(d => d.IdStatus == IdStatus.Value);
        }
        
        if (Oibosoba.HasValue)
        {
            query = query.Where(d => d.Oibosoba == Oibosoba.Value);
        }
        
        if (IdPrioriteta.HasValue)
        {
            query = query.Where(d => d.IdPrioriteta == IdPrioriteta.Value);
        }
        

        return query;
    }
}

