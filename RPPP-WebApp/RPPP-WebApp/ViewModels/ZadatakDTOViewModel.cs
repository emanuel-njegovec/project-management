using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ZadatakDTOViewModel
{
    
    //Stock
    public int IdZadatka { get; set; }

    public string ImeZadatka { get; set; }
    public DateTime PlaniraniPocetak { get; set; }

    public DateTime PlaniraniZavrsetak { get; set; }

    public DateTime StvarniPocetak { get; set; }

    public DateTime StvarniZavrsetak { get; set; }

    public int IdZahtjeva { get; set; }

    public int IdStatus { get; set; }

    public int Oibosoba { get; set; }

    public int IdPrioriteta { get; set; }
    
    public string Persona { get; set; }
    
    public string vrstaposlica { get; set; }
    
    public string imezadatka { get; set; }
    
    public List<PosaoStavka> Poslovi { get; set; }
    
    public virtual Prioritet IdPrioritetaNavigation { get; set; }
    public virtual Status IdStatusNavigation { get; set; }

    public virtual Zahtjev IdZahtjevaNavigation { get; set; }

    public virtual Osoba OibosobaNavigation { get; set; }
    
    public IEnumerable<EvidencijaRadaViewModel> Radovi { get; set; } 
    
    public List<EvidencijaRadaDTO> RadoviPravi { get; set; } 
    
    public ZadatakDTOViewModel()
    {
        
        this.Radovi = new List<EvidencijaRadaViewModel>();
    }
    
  
       

}
