namespace RPPP_WebApp.Views.Zahtjev;

public class ZahtjevDTO
{
    public ZahtjevDTO(int idZahtjeva, string naslov, string opis, string nazVrsta, string nazPrioritet, string nazProjekt)
    {
        this.IdZahtjeva = idZahtjeva;
        this.Naslov = naslov;
        this.Opis = opis;
        this.NazVrsta = nazVrsta;
        this.NazPrioritet = nazPrioritet;
        this.NazProjekt = nazProjekt;
    }

    public ZahtjevDTO()
    {

    }
    
    public int IdZahtjeva { get; set; }
    public string Naslov { get; set; }
    public string Opis { get; set; }
    public string NazVrsta { get; set; }
    public string NazPrioritet { get; set; }
    public string NazProjekt { get; set; }
}