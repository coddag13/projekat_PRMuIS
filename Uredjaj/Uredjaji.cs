using System;
using System.Collections.Generic;
using System.Linq;

// Apstraktna klasa za uređaje
[Serializable]
public abstract class Uredjaji
{
    public string Ime { get; set; }
    public Dictionary<string, string> Funkcije { get; set; }

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
}

// Implementacija klase Svetlo
[Serializable]
public class Svetlo : Uredjaji
{
    public Svetlo() : base("Svetlo")
    {
        Funkcije.Add("Intenzitet", "50%"); // Podrazumevani intenzitet
        Funkcije.Add("Boja", "Bela");     // Podrazumevana boja
        Funkcije.Add("Stanje","Ukljuceno");
    }
}

// Implementacija klase Klima
[Serializable]
public class Klima : Uredjaji
{
    public Klima() : base("Klima")
    {
        Funkcije.Add("Temperatura", "22°C"); // Podrazumevana temperatura
        Funkcije.Add("Rezim", "hladi");      // Podrazumevani režim
    }

    public override void AzurirajFunkciju(string funkcija, string novaVrednost)
    {
        if (funkcija == "Temperatura")
        {
            if (int.TryParse(novaVrednost.Replace("°C", ""), out _))
            {
                base.AzurirajFunkciju(funkcija, novaVrednost);
            }
            else
            {
                Console.WriteLine($"Nevažeća vrednost za temperaturu: {novaVrednost}");
            }
        }
        else if (funkcija == "Rezim")
        {
            if (novaVrednost == "hladi" || novaVrednost == "greje" || novaVrednost == "ventilacija")
            {
                base.AzurirajFunkciju(funkcija, novaVrednost);
            }
            else
            {
                Console.WriteLine($"Nevažeći režim rada: {novaVrednost}");
            }
        }
        else
        {
            Console.WriteLine($"Nepoznata funkcija za klimu: {funkcija}");
        }
    }
}

// Implementacija klase TV
[Serializable]
public class TV : Uredjaji
{
    public TV() : base("TV")
    {
        Funkcije.Add("Kanal", "1");         // Podrazumevani kanal
        Funkcije.Add("JacinaZvuka", "20"); // Podrazumevana jačina zvuka
    }
}

// Implementacija klase Vrata
[Serializable]
public class Vrata : Uredjaji
{
    public Vrata() : base("Vrata")
    {
        Funkcije.Add("Otvoreno", "Ne");   // Podrazumevano zatvorena
        Funkcije.Add("Zakljucano", "Ne"); // Podrazumevano otključana
    }

    public override void AzurirajFunkciju(string funkcija, string novaVrednost)
    {
        if (funkcija == "Otvoreno" || funkcija == "Zakljucano")
        {
            if (novaVrednost.ToLower() == "da" || novaVrednost.ToLower() == "ne")
            {
                base.AzurirajFunkciju(funkcija, novaVrednost == "da" ? "Da" : "Ne");
            }
            else
            {
                Console.WriteLine($"Nevažeća vrednost za {funkcija}: {novaVrednost}");
            }
        }
        else
        {
            Console.WriteLine($"Nepoznata funkcija za vrata: {funkcija}");
        }
    }
}
