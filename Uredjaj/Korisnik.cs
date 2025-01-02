using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uredjaj
{
    public class Korisnik
    {
        string Ime { get; set; } = "";
        string Prezime { get; set; } = "";
        string KorisnickoIme { get; set; } = "";
        string Lozinka { get; set; } = "";
        public bool StatusPrijave { get; set; } = false;  
        public int DodeljeniPort { get; set; } = 0;

        public Korisnik(string korisnickoIme, string lozinka, bool statusPrijave, int dodeljeniPort)
        {
            KorisnickoIme = korisnickoIme;
            Lozinka = lozinka;
            StatusPrijave = statusPrijave;
            DodeljeniPort = dodeljeniPort;
        }

        public Korisnik() { }
        public string DodeliIme()
        {
            return KorisnickoIme;
        }

        public void PrikaziKorisnika()
        {
            Console.WriteLine($"Korisnicko ime:{KorisnickoIme}");
            Console.WriteLine($"Lozinka:{Lozinka}");
            Console.WriteLine($"Status prijave:{StatusPrijave}");
            Console.WriteLine($"Dodeljeni port:{DodeljeniPort}");
        }

        public int DodeliPort(string korisnickoIme)
        {
            int hashCode = korisnickoIme.GetHashCode();
            int port = 50001 + (hashCode % 100);

            return port;
        }
    }
}
