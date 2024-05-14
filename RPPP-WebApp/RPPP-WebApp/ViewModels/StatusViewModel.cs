using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class StatusViewModel
    {
        public IEnumerable<Status> Statusi { get; set; }
        public PagingInfo PagingInfo { get; set; }
        //public DokumentFilter Filter { get; set; }
    }
}
