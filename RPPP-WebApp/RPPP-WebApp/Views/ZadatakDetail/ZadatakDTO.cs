namespace RPPP_WebApp.Views.ZadatakDetail;

public class ZadatakDTO
{
    public ZadatakDTO(string imeZadatka, DateTime planiraniPocetak, DateTime planiraniZavrsetak, DateTime stvarniPocetak, DateTime stvarniZavrsetak, string nazZahtjeva, string nazStatus, string nazOsoba, string nazPrioriteta)
    {
        ImeZadatka = imeZadatka;
        PlaniraniPocetak = planiraniPocetak;
        PlaniraniZavrsetak = planiraniZavrsetak;
        StvarniPocetak = stvarniPocetak;
        StvarniZavrsetak = stvarniZavrsetak;
        NazZahtjeva = nazZahtjeva;
        NazStatus = nazStatus;
        NazOsoba = nazOsoba;
        NazPrioriteta = nazPrioriteta;
    }

    public ZadatakDTO()
    {
        
    }
    public string ImeZadatka { get; set; }
    
    public DateTime PlaniraniPocetak { get; set; }

    public DateTime PlaniraniZavrsetak { get; set; }

    public DateTime StvarniPocetak { get; set; }

    public DateTime StvarniZavrsetak { get; set; }

    public String NazZahtjeva { get; set; }

    public String NazStatus { get; set; }

    public String NazOsoba { get; set; }

    public String NazPrioriteta { get; set; }

}