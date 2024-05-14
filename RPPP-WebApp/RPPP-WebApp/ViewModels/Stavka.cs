namespace RPPP_WebApp.ViewModels;

public class Stavka
{
    public int IdStavke { get; set; }
    
    public string Opis { get; set; }
    
    public DateTime DatumNastanka { get; set; }
    
    public string Format { get; set; }

    public int IdVrsteDokumenta { get; set; }

    public int IdProjekta { get; set; }
}