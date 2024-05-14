namespace RPPP_WebApp.ViewModels
{
    public class TransakcijaViewModel
    {
        public int IdTransakcije { get; set; }

        public decimal Iznos { get; set; }

        public int Iban2zaTransakciju { get; set; }

        public DateTime Datum { get; set; }

        public TimeSpan Vrijeme { get; set; }

        public int IbanKartice { get; set; }

        public string Opis { get; set; }
    }
}
