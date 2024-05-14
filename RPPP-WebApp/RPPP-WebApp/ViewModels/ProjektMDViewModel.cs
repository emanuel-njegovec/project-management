using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ProjektMDViewModel{

    public IEnumerable<Projekt> Projekti { get; set; }

    public PagingInfo PagingInfo { get; set; }
    
    public ProjektFilter Filter { get; set; }
    
}