using System.Linq.Expressions;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ZahtjevSort
    {
        public static IQueryable<Zahtjev> ApplySort(this IQueryable<Zahtjev> query, int sort, bool ascending)
        {
            Expression<Func<Zahtjev, object>> orderSelector = sort switch
            {
                1 => d => d.Naslov,
                2 => d => d.Opis,
                3 => d => d.IdVrsteZahtjevaNavigation.Naziv,
                4 => d => d.IdPrioritetaNavigation.Naziv,
                5 => d => d.IdProjektaNavigation.Naziv,
                6 => d => d.Zadataks.OrderBy(z => z.ImeZadatka).FirstOrDefault().ImeZadatka,
                _ => null
            };

            if (orderSelector != null)
            {
                query = ascending ?
                       query.OrderBy(orderSelector) :
                       query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}
