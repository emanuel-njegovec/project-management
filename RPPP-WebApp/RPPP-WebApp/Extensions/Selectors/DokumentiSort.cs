using System.Linq.Expressions;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors;

public static class DokumentiSort
{
    public static IQueryable<Dokument> ApplySort(this IQueryable<Dokument> query, int sort, bool ascending)
    {
        Expression<Func<Dokument, object>> orderSelector = sort switch
        {
            1 => d => d.DatumNastanka,
            2 => d => d.Stavka,
            3 => d => d.Format,
            4 => d => d.IdVrsteDokumenta,
            5 => d => d.IdProjekta,
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
