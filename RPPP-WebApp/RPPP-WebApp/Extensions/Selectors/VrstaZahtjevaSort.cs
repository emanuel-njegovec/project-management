using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class VrstaZahtjevaSort
    {
        public static IQueryable<VrstaZahtjeva> ApplySort(this IQueryable<VrstaZahtjeva> query, int sort, bool ascending)
        {
            Expression<Func<VrstaZahtjeva, object>> orderSelector = sort switch
            {
                1 => d => d.Naziv,
                2 => d => d.Opis,
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
