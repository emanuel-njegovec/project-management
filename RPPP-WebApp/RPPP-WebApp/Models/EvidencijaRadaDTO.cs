using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Models;

public class EvidencijaRadaDTO
{
    public IEnumerable<EvidencijaRada> EviRadovi { get; set; }
    public PagingInfo PagingInfo { get; set; }
    //public DokumentFilter Filter { get; set; }
    
    public int IdEvidencijaRad { get; set; }

    public string Opis { get; set; }
    public DateTime DatumRada { get; set; }
    public int VrijemeRada { get; set; }
    
    public int Oibosoba { get; set; }

    public int IdZadatka { get; set; }

    public int IdVrstePosla { get; set; }
    
    public string Persona { get; set; }
    
    public string vrstaposlica { get; set; }
    
    public string imezadatka { get; set; }
    
    

    public virtual VrstaPosla IdVrstePoslaNavigation { get; set; }

    public virtual Zadatak IdZadatkaNavigation { get; set; }

    public virtual Osoba OibosobaNavigation { get; set; }
}