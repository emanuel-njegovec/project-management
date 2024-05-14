using RPPP_WebApp.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class StatusSort
    {
        public static IQueryable<Status> ApplySort(this IQueryable<Status> query, int sort, bool ascending)
        {
            Expression<Func<Status, object>> orderSelector = sort switch
            {
                1 => d => d.Naziv,
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
