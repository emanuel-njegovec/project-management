namespace RPPP_WebApp.ViewModels
{
    public class KarticaTransakcijeViewModel
    {
        public int IdKartice { get; set; }

        public int Ibankartice { get; set; }

        public decimal Stanje { get; set; }

        public string NazivProjekta { get; set; }

        public string NazivOsobe { get; set; }

        public IEnumerable<TransakcijaViewModel> Transakcije { get; set;}
        
        public KarticaTransakcijeViewModel() { 
            this.Transakcije = new List<TransakcijaViewModel>();
        }
    }
}
