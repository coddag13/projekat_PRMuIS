using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uredjaj
{
    [Serializable]
    public class Uredjajii
    {
        public string Ime { get; set; }
        public int Port { get; set; }
        public Dictionary<string, string> Funkcije { get; set; }

        public List<string> EvidencijaPromjena { get; set; }

        public DateTime PoslednjaPromjena { get; set; }

        public List<Uredjajii> u {  get; set; }
        public Uredjajii(string ime,int port)
        {
            Ime= ime;
            Port= port;
            Funkcije= new Dictionary<string, string>();
            EvidencijaPromjena = new List<string>();
            PoslednjaPromjena = DateTime.Now;
        }

        public Uredjajii(string ime, int port, Dictionary<string, string> funkcije) 
        {
            Ime = ime;
            Port= port;
            Funkcije = funkcije;
            EvidencijaPromjena= new List<string>();
            PoslednjaPromjena= DateTime.Now;
            
        }

        public Uredjajii()
        {
            u = new List<Uredjajii>
            {
                 new Uredjajii("Svetlo", 7001, new Dictionary<string, string> { { "Intenzitet", "50%" }, { "Boja", "Bela" } }),
                new Uredjajii("Klima", 7002, new Dictionary<string, string> { { "Temperatura", "22°C" } }),
                new Uredjajii("Ventilator", 7003, new Dictionary<string, string> { { "Brzina", "Medium" } }),
                new Uredjajii("TV", 7004, new Dictionary<string, string> { { "Kanal", "HBO" }, { "Glasnoća", "25%" } })

            };
        }

       public void Azuriranje(string fja,string vr)
        {
            if(Funkcije.ContainsKey(fja))
            {
                Funkcije[fja] = vr;
            }
            else
            {
                Funkcije.Add(fja, vr);
            }
            PoslednjaPromjena = DateTime.Now;

            EvidencijaPromjena.Add($"[{PoslednjaPromjena}] {Ime}: {fja} promijenjena je na novu vrijednost: {vr}");
        }

       
        public string PrikazTrenutnogStanja()
        {
            string trenStanje = $"Uređaj: {Ime}, Port: {Port}, Trernutno stanje funkcija:";
            foreach (var funkcija in Funkcije)
            {
                trenStanje += $"({funkcija.Key}: {funkcija.Value});";
            }
            trenStanje.Substring(0, trenStanje.Length - 1);

            return trenStanje;
        }

        public string PrikazEvidencijePromjena()
        {
            return string.Join("\n",EvidencijaPromjena);
        }

        public List<Uredjajii> SviUredjaji()
        {
            return u;
        }
        public void AzurirajListu(List<Uredjajii> noviUredjaji)
        {
            u = noviUredjaji;
        }

    }
}
