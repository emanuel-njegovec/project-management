namespace RPPP_WebApp.Views.Projekt;

public class ProjektDTO
{
    public ProjektDTO(string naziv, string opis, DateTime datumPocetka, DateTime datumZavrsetka, int oibNarucitelja, string idVrsteProjekta){
        Naziv = naziv;
        Opis = opis;
        DatumPocetka = datumPocetka;
        DatumZavrsetka = datumZavrsetka;
        Oibnarucitelj = oibNarucitelja;
        VrstaProjekta = idVrsteProjekta;
    }

    public ProjektDTO()
    {
        
    }
    public string Naziv { get; set; }

    public string Opis { get; set; }

    public DateTime DatumPocetka { get; set; }

    public DateTime DatumZavrsetka { get; set; }

    public int Oibnarucitelj { get; set; }

    public string VrstaProjekta { get; set; }

}