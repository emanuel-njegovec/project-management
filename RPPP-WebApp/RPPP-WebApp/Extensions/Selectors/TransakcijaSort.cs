using System.Linq.Expressions;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class TransakcijaSort
    {
        public static IQueryable<Transakcija> ApplySort(this IQueryable<Transakcija> query, int sort, bool ascending)
        {
            Expression<Func<Transakcija, object>> orderSelector = sort switch
            {
                1 => d => d.IdTransakcije,
                2 => d => d.Iznos,
                3 => d => d.Iban2zaTransakciju,
                4 => d => d.Datum,
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
