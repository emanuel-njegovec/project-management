
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class ZadatakViewModel
{
    public IEnumerable<Zadatak> Zadatci { get; set; }

    public PagingInfo PagingInfo { get; set; }
    //public DokumentFilter Filter { get; set; }

    public int IdZadatka { get; set; }

    public string ImeZadatka { get; set; }
    public DateTime PlaniraniPocetak { get; set; }
    public DateTime PlaniraniZavrsetak { get; set; }
    public DateTime StvarniPocetak { get; set; }
    public DateTime StvarniZavrsetak { get; set; }
    public int Oibosoba { get; set; }
    public int IdZahtjeva { get; set; }
    public int IdStatus { get; set; }
    public int IdPrioriteta { get; set; }



    public virtual Status IdStatusNavigation { get; set; }

    public virtual Zahtjev IdZahtjevaNavigation { get; set; }

    public virtual Osoba OibosobaNavigation { get; set; }

    public virtual Prioritet IdPrioritetaNavigation { get; set; }
}