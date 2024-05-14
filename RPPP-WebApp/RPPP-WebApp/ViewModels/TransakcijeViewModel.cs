using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class TransakcijeViewModel
    {
        public IEnumerable<TransakcijaViewModel> Transakcije { get; set; }
        public PagingInfo PagingInfo { get; set; }
        //public DokumentFilter Filter { get; set; }
    }
}
