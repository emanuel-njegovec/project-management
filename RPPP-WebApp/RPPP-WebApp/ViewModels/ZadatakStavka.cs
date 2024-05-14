namespace RPPP_WebApp.ViewModels
{
    public class ZadatakStavka
    {
        public int IdStavke { get; set; }

        public string ImeZadatka { get; set; }

        public DateTime PlaniraniPocetak { get; set; }

        public DateTime PlaniraniZavrsetak { get; set; }

        public DateTime StvarniPocetak { get; set; }

        public DateTime StvarniZavrsetak { get; set; }

        public string Zahtjev { get; set; }

        public string Status { get; set; }

        public string Osoba { get; set; }

        public string Prioritet { get; set; }
    }
}
