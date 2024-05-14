using RPPP_WebApp.Models;
using RPPP_WebApp.Views.Zadatak;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjevViewModel
    {
        public IEnumerable<Zahtjev> Zahtjevi { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public ZahtjevFilter Filter { get; set; }
    }
}
