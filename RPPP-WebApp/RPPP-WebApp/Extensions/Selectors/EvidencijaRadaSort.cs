using RPPP_WebApp.Models;
using System.Linq.Expressions;
namespace RPPP_WebApp.Extensions.Selectors;

public static class EvidencijaRadaSort
{
    public static IQueryable<EvidencijaRada> ApplySort(this IQueryable<EvidencijaRada> query, int sort, bool ascending)
    {
        Expression<Func<EvidencijaRada, object>> orderSelector = sort switch
        {
            1 => d => d.Opis,
            2 => d => d.DatumRada,
            3 => d => d.VrijemeRada,
            4 => d => d.Oibosoba,
            5 => d => d.IdZadatka,
            6 => d => d.IdVrstePosla,
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