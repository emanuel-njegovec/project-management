using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ProjektViewModel{

    public IEnumerable<Projekt> Projekt { get; set; }

    public PagingInfo PagingInfo { get; set; }
    
}