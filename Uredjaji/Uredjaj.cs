using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uredjaji
{
    public class Uredjaj
    {
        public string Ime { get; set; }
        public int Port { get; set; }
        public Dictionary<string, string> Funkcije { get; set; }

        public Uredjaj(string ime, int port, Dictionary<string, string> funkcije)
        {
            Ime = ime;
            Port = port;
            Funkcije = funkcije;
        }

        public string IzvrsiKomandu(string komanda)
        {
            if (Funkcije.ContainsKey(komanda))
            {
                return $"Komanda '{komanda}' izvršena. Vrednost: {Funkcije[komanda]}";
            }
            else
            {
                return $"Komanda '{komanda}' nije podržana na uređaju {Ime}.";
            }
        }

        public override string ToString()
        {
            string funkcijeStr = string.Join(", ", Funkcije.Select(f => $"{f.Key}: {f.Value}"));
            return $"{Ime} (Port: {Port}) - Funkcije: {funkcijeStr}";
        }

        public static List<Uredjaj> GenerisiListuUredjaja()
        {
            return new List<Uredjaj>
            {
                new Uredjaj("Svetlo", 7001, new Dictionary<string, string> { { "Intenzitet", "50%" }, { "Boja", "Bela" } }),
                new Uredjaj("Klima", 7002, new Dictionary<string, string> { { "Temperatura", "22°C" } }),
                new Uredjaj("Ventilator", 7003, new Dictionary<string, string> { { "Brzina", "Medium" } }),
                new Uredjaj("TV", 7004, new Dictionary<string, string> { { "Kanal", "HBO" }, { "Glasnoća", "25%" } })
            };
        }
    }
    }
}
