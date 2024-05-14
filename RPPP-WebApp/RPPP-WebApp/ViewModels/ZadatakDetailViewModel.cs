    using global::RPPP_WebApp.Models;
    using RPPP_WebApp.Models;
    using System.Collections.Generic;

    namespace RPPP_WebApp.ViewModels
    {
        public class ZadatakDetailViewModel
        {
            public IEnumerable<Zadatak> Zadaci { get; set; }
            public PagingInfo PagingInfo { get; set; }
            //public DokumentFilter Filter { get; set; }
        }
    }
