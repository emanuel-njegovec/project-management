using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjevFilter : IPageFilter
    {
        public int? IdZahtjeva { get; set; }
        public string Naslov { get; set; }
        public string Opis { get; set; }
        public int? IdVrsteZahtjeva { get; set; }
        public int? IdPrioriteta { get; set; }
        public int? IdProjekta { get; set; }
        public virtual Prioritet IdPrioritetaNavigation { get; set; }

        public virtual Projekt IdProjektaNavigation { get; set; }

        public virtual VrstaZahtjeva IdVrsteZahtjevaNavigation { get; set; }

        public bool IsEmpty()
        {
            bool active = IdZahtjeva.HasValue
                          || IdVrsteZahtjeva.HasValue
                          || IdPrioriteta.HasValue
                          || IdProjekta.HasValue;
            return !active;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}-{3}",
                IdZahtjeva,
                IdVrsteZahtjeva,
                IdPrioriteta,
                IdProjekta);
        }

        public static ZahtjevFilter FromString(string s)
        {
            var filter = new ZahtjevFilter();
            if (!string.IsNullOrEmpty(s))
            {
                string[] arr = s.Split('-', StringSplitOptions.None);

                if (arr.Length == 4)
                {
                    filter.IdZahtjeva = string.IsNullOrWhiteSpace(arr[0]) ? new int?() : int.Parse(arr[0]);
                    filter.IdVrsteZahtjeva = string.IsNullOrWhiteSpace(arr[1]) ? new int?() : int.Parse(arr[1]);
                    filter.IdPrioriteta = string.IsNullOrWhiteSpace(arr[2]) ? new int?() : int.Parse(arr[2]);
                    filter.IdProjekta = string.IsNullOrWhiteSpace(arr[3]) ? new int?() : int.Parse(arr[3]);                }
            }

            return filter;
        }

        public IQueryable<Zahtjev> Apply(IQueryable<Zahtjev> query)
        {
            if (IdZahtjeva.HasValue)
            {
                query = query.Where(d => d.IdZahtjeva == IdZahtjeva.Value);
            }
            if (IdVrsteZahtjeva.HasValue)
            {
                query = query.Where(d => d.IdVrsteZahtjeva == IdVrsteZahtjeva.Value);
            }
            if (IdPrioriteta.HasValue)
            {
                query = query.Where(d => d.IdPrioriteta == IdPrioriteta.Value);
            }
            if (IdProjekta.HasValue)
            {
                query = query.Where(d => d.IdProjekta == IdProjekta.Value);
            }
            return query;
        }
    }
}