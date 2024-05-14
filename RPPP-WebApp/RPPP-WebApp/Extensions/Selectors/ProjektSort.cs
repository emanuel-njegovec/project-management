using System.Linq.Expressions;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors{

    public static class ProjektSort{

        public static IQueryable<Projekt> ApplySort(this IQueryable<Projekt> query, int sort, bool ascending){

            Expression<Func<Projekt, object>> orderSelector = sort switch {

                1 => p => p.IdProjekta,
                2 => p => p.Naziv,
                3 => p => p.IdVrsteProjekta,
                4 => p => p.Opis,
                5 => p => p.DatumPocetka,
                6 => p => p.DatumZavrsetka,
                7 => p => p.Oibnarucitelj,
                8 => p => p.Dokuments.OrderBy(d => d.DatumNastanka).FirstOrDefault().DatumNastanka,
                9 => p => p.EvidencijaUlogas.OrderBy(u => u.IdEvidencijaUloga).FirstOrDefault().IdEvidencijaUloga,
                10 => p => p.ProjektnaKarticas.OrderBy(k => k.IdKartice).FirstOrDefault().IdKartice,
                11 => p => p.Zahtjevs.OrderBy(z => z.Naslov).FirstOrDefault().Naslov,
                _ => null
 
            };

            if(orderSelector != null){

                query = ascending ? 
                        query.OrderBy(orderSelector) : 
                        query.OrderByDescending(orderSelector);

            }

            return query;

        }

    }

}