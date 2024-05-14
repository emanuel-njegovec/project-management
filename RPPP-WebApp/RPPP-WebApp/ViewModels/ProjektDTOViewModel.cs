using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ProjektDTOViewModel{

    public int IdProjekta { get; set; }

    public string Naziv { get; set; }

    public string Opis { get; set; }

    public DateTime DatumPocetka { get; set; }

    public DateTime DatumZavrsetka { get; set; }

    public int Oibnarucitelj { get; set; }

    public int IdVrsteProjekta { get; set; }

    public virtual VrstaProjekta IdVrsteProjektaNavigation { get; set; }

    public virtual Narucitelj OibnaruciteljNavigation { get; set; }
    
    public string Narucitelj { get; set; }
    
    public string Vrsta { get; set; }
    
    public List<Stavka> Dokici { get; set; }
    
    public virtual Narucitelj OibNaruciteljNavigation { get; set; }
    
    public virtual VrstaProjekta IdVrstaProjektaNavigation { get; set; }
    
    public IEnumerable<DokumentiViewModel> Dokumenti { get; set; }
    
    public List<DokumentDTO> Dokuments { get; set; }

    public ProjektDTOViewModel()
    {
        this.Dokumenti = new List<DokumentiViewModel>();
    }
    
}