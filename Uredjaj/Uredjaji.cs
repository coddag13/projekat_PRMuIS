using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[Serializable]
public abstract class Uredjaji
{
    public string Ime { get; set; }
    public Dictionary<string, string> Funkcije { get; set; }
    public List<string> LogAktivnosti { get; set; } = new List<string>();

    public Uredjaji(string ime)
    {
        Ime = ime;
        Funkcije = new Dictionary<string, string>();
    }
    
    public virtual void AzurirajFunkciju(string funkcija, string novaVrednost)
    {
        if (Funkcije.ContainsKey(funkcija))
        {
            Funkcije[funkcija] = novaVrednost;
        }
        else
        {
            throw new ArgumentException($"Funkcija {funkcija} nije pronađena.");
        }
    }

    public virtual string DobijStanje()
    {
        return string.Join(", ", Funkcije.Select(f => $"{f.Key}: {f.Value}"));
    }

  
    public void PrikaziSveUredjaje(List<Uredjaji> uredjaji)
    {
        if (uredjaji == null || !uredjaji.Any())
        {
            Console.WriteLine("Nema uređaja za prikaz.");
            return;
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"Ime uređaja",-20} {"Funkcija",-20} {"Vrednost",-15} {"Vremenska oznaka",-20}");
        Console.WriteLine(new string('-', 80));

        foreach (var uredjaj in uredjaji)
        {
            foreach (var funkcija in uredjaj.Funkcije)
            {
                string vremenskaOznaka = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine($"{uredjaj.Ime,-20} {funkcija.Key,-20} {funkcija.Value,-15} {vremenskaOznaka,-20}");
            }
        }

        Console.WriteLine(new string('-', 80));
    }



}

[Serializable]
public class Svetlo : Uredjaji
{
    public Svetlo() : base("Svetlo")
    {
        Funkcije.Add("Intenzitet", "50%"); 
        Funkcije.Add("Boja", "Bela");     
        Funkcije.Add("Stanje","Ukljuceno");
    }
}


[Serializable]
public class Klima : Uredjaji
{
    public Klima() : base("Klima")
    {
        Funkcije.Add("Temperatura", "22°C"); 
        Funkcije.Add("Rezim", "hladi");      
    }
}

[Serializable]
public class TV : Uredjaji
{
    public TV() : base("TV")
    {
        Funkcije.Add("Kanal", "1");         
        Funkcije.Add("JacinaZvuka", "20"); 
    }
}


[Serializable]
public class Vrata : Uredjaji
{
    public Vrata() : base("Vrata")
    {
        Funkcije.Add("Otvoreno", "Ne");   
        Funkcije.Add("Zakljucano", "Ne"); 
    }
}
