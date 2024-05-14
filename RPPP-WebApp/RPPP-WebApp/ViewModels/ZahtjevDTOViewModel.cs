using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ZahtjevDTOViewModel
{

    //Stock
    public int IdZahtjeva { get; set; }

    public string Naslov { get; set; }

    public string Opis { get; set; }

    public int IdVrsteZahtjeva { get; set; }

    public int IdPrioriteta { get; set; }

    public int IdProjekta { get; set; }

    public string VrstaZahtjeva { get; set; }

    public string Prioritet { get; set; }

    public string Projekt { get; set; }

    public List<ZadatakStavka> Zadaci { get; set; }

    public virtual Prioritet IdPrioritetaNavigation { get; set; }

    public virtual VrstaZahtjeva IdVrsteZahtjevaNavigation { get; set; }

    public virtual Projekt IdProjektaNavigation { get; set; }

    public IEnumerable<ZadatakViewModel> Zadatci { get; set; }

    public List<Models.ZadatakDTO> ZadatciPravi { get; set; }

    public ZahtjevDTOViewModel()
    {

        this.Zadatci = new List<ZadatakViewModel>();
    }
}