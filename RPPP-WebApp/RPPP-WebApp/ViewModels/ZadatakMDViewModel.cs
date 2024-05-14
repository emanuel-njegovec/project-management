using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;


public class ZadatakMDViewModel
{
    public IEnumerable<Zadatak> Zadaci { get; set; }
    public PagingInfo PagingInfo { get; set; }
    public ZadatakFilter Filter { get; set; }
}