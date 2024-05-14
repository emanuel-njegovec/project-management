using RPPP_WebApp.Models;
using System.Linq.Expressions;
namespace RPPP_WebApp.Extensions.Selectors;

public static class ZadatakSort
{
    public static IQueryable<Zadatak> ApplySort(this IQueryable<Zadatak> query, int sort, bool ascending)
    {
        Expression<Func<Zadatak, object>> orderSelector = sort switch
        {
            1 => d => d.ImeZadatka,
            2 => d => d.PlaniraniPocetak,
            3 => d => d.PlaniraniZavrsetak,
            4 => d => d.StvarniPocetak,
            5 => d => d.StvarniZavrsetak,
            6 => d => d.IdZahtjeva,
            7 => d => d.IdStatus,
            8 => d => d.Oibosoba,
            9 => d => d.IdPrioriteta,
            10 => d => d.EvidencijaRada.OrderBy(u => u.Opis).FirstOrDefault().Opis,
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