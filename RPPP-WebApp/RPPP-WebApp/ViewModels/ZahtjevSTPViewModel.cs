using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjevSTPViewModel
    {
        public IEnumerable<Zahtjev> Zahtjevii { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public ZahtjevFilter Filter { get; set; }
    }
}
