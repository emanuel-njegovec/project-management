using System.Linq.Expressions;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ProjektnaKarticaSort
    {
        public static IQueryable<ProjektnaKartica> ApplySort(this IQueryable<ProjektnaKartica> query, int sort, bool ascending)
        {
            Expression<Func<ProjektnaKartica, object>> orderSelector = sort switch
            {
                1 => d => d.IdKartice,
                2 => d => d.Ibankartice,
                3 => d => d.Stanje,
                4 => d => d.IdProjektaNavigation.Naziv,
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
