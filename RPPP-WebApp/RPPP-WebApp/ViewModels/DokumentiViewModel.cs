using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class DokumentiViewModel
{
    public IEnumerable<Dokument> Dokumenti { get; set; }

    public PagingInfo PagingInfo { get; set; }
    
    public int IdDokumenta { get; set; }

    public DateTime DatumNastanka { get; set; }

    public string Stavka { get; set; }

    public string Format { get; set; }

    public int IdVrsteDokumenta { get; set; }

    public int IdProjekta { get; set; }

    public virtual Projekt IdProjektaNavigation { get; set; }

    public virtual VrstaDokumenta IdVrsteDokumentaNavigation { get; set; }
}
