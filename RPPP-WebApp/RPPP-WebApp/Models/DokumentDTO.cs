using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Models;

public class DokumentDTO
{
    public IEnumerable<Dokument> Dokuments { get; set; }
    public PagingInfo PagingInfo { get; set; }
    
    public int IdDokumenta { get; set; }

    public DateTime DatumNastanka { get; set; }

    public string Stavka { get; set; }

    public string Format { get; set; }

    public int IdVrsteDokumenta { get; set; }

    public int IdProjekta { get; set; }

    public string vrsta { get; set; }

    public string imeProjekta { get; set; }

    public virtual Projekt IdProjektaNavigation { get; set; }

    public virtual VrstaDokumenta IdVrsteDokumentaNavigation { get; set; }
}