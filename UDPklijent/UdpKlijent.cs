using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace UDPKlijent
{
    public class UdpKlijent
    {
        public static void Main(string[] args)
        {
            try
            {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 6000);
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Slanje zahteva za listu uređaja
                    string zahtev = "LISTA";
                    byte[] zahtevBytes = Encoding.UTF8.GetBytes(zahtev);
                    udpClient.Send(zahtevBytes, zahtevBytes.Length, serverEP);

                    // Primanje liste uređaja
                    byte[] responseBytes = udpClient.Receive(ref serverEP);
                    List<Uredjaji> uredjaji;
                    using (MemoryStream ms = new MemoryStream(responseBytes))
                    {
                        uredjaji = (List<Uredjaji>)formatter.Deserialize(ms);
                    }

                    Console.WriteLine("Dostupni uređaji:");
                    for (int i = 0; i < uredjaji.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {uredjaji[i].Ime}");
                    }

                    // Ažuriranje uređaja
                    Console.WriteLine("Unesite broj uređaja za podešavanje:");
                    int izbor = int.Parse(Console.ReadLine()) - 1;

                    if (izbor >= 0 && izbor < uredjaji.Count)
                    {
                        var izabraniUredjaj = uredjaji[izbor];

                        Console.WriteLine($"Izabrali ste uređaj: {izabraniUredjaj.Ime}");
                        Console.WriteLine("Trenutne funkcije i vrednosti:");
                        foreach (var funkcija1 in izabraniUredjaj.Funkcije)
                        {
                        Console.WriteLine($"{funkcija1.Key}: {funkcija1.Value}");
                        }

                    Console.WriteLine("Unesite ime funkcije za promenu:");
                        string funkcija = Console.ReadLine();
                        Console.WriteLine("Unesite novu vrednost:");
                        string novaVrednost = Console.ReadLine();

                        using (MemoryStream ms = new MemoryStream())
                        {
                            formatter.Serialize(ms, izabraniUredjaj.Ime);
                            formatter.Serialize(ms, funkcija);
                            formatter.Serialize(ms, novaVrednost);
                            byte[] dataToSend = ms.ToArray();
                            udpClient.Send(dataToSend, dataToSend.Length, serverEP);
                        }

                        // Primanje odgovora
                        responseBytes = udpClient.Receive(ref serverEP);
                        string odgovor = Encoding.UTF8.GetString(responseBytes);
                        Console.WriteLine($"Odgovor servera: {odgovor}");
                    }
                    else
                    {
                        Console.WriteLine("Pogrešan izbor uređaja.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška: {ex.Message}");
                }
        }
    }
}
        /* Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
         IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 6000);
         byte[] buffer = new byte[4096];
         BinaryFormatter formatter = new BinaryFormatter();
         int received = clientSocket.Receive(buffer);

         Console.WriteLine("Klijent je povezan na server.");
         while (true)
         {
             received = clientSocket.Receive(buffer);
             List<Uredjaji> uredjaji;
             using (MemoryStream ms = new MemoryStream(buffer, 0, received))
             {
                 uredjaji = (List<Uredjaji>)formatter.Deserialize(ms);
             }

             Console.WriteLine("Lista dostupnih uređaja:");
             for (int i = 0; i < uredjaji.Count; i++)
             {
                 Console.WriteLine($"{i + 1}. {uredjaji[i].Ime}");
             }

             int izbor;
             do
             {
                 Console.WriteLine("Unesite redni broj uređaja koji želite da podesite:");
                 izbor = int.Parse(Console.ReadLine()) - 1;
             } while (izbor < 0 || izbor >= uredjaji.Count);

             Uredjaji izabraniUredjaj = uredjaji[izbor];
             Console.WriteLine($"Izabrali ste uređaj: {izabraniUredjaj.Ime}");

             Console.WriteLine("Trenutne funkcije i vrednosti:");
             foreach (var funkcija in izabraniUredjaj.Funkcije)
             {
                 Console.WriteLine($"{funkcija.Key}: {funkcija.Value}");
             }

             Console.WriteLine("Unesite ime funkcije koju želite da promenite:");
             string funkcijazaPromenu = Console.ReadLine();


             if (izabraniUredjaj.Funkcije.ContainsKey(funkcijazaPromenu))
             {
                 Console.WriteLine("Unesite novu vrednost za funkciju:");
                 string novaVrednost = Console.ReadLine();

                 using (MemoryStream msSend = new MemoryStream())
                 {
                     formatter.Serialize(msSend, izabraniUredjaj.Ime);
                     formatter.Serialize(msSend, funkcijazaPromenu);
                     formatter.Serialize(msSend, novaVrednost);

                     clientSocket.Send(msSend.ToArray());
                 }
                 Console.WriteLine($"Funkcija {funkcijazaPromenu} ažurirana.");
             }
             else
             {
                 Console.WriteLine("Nepoznata funkcija. Pokušajte ponovo.");
             }

         }

     }
     catch (Exception ex)
     {
         Console.WriteLine($"Greška: {ex.Message}");
     }
 }
}*/


// Menjanje funkcija uređaja
/*Console.WriteLine("Unesite ime funkcije koju želite da promenite:");
string funkcijaZaPromenu = Console.ReadLine();


Console.WriteLine("Unesite komandu za uređaj (npr. 'Svetlo:Stanje:Uključeno'):");
string komanda = Console.ReadLine();

byte[] porukaBytes = Encoding.UTF8.GetBytes(komanda);
clientSocket.SendTo(porukaBytes, serverEP);

// Prijem odgovora od servera
byte[] prijemniBafer = new byte[1024];
EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any, 0);
int primljenoBajta = clientSocket.ReceiveFrom(prijemniBafer, ref posiljaocEP);

string odgovor = Encoding.UTF8.GetString(prijemniBafer, 0, primljenoBajta);
Console.WriteLine($"Odgovor servera: {odgovor}");

// Parsiranje serverovog odgovora
string[] delovi = odgovor.Split(':');
if (delovi.Length >= 3)
{
string imeUredjaja = delovi[0];
string funkcija = delovi[1];
string trenutnaVrednost = delovi[2];

Console.WriteLine($"Uređaj: {imeUredjaja}, Funkcija: {funkcija}, Trenutna vrednost: {trenutnaVrednost}");
Console.WriteLine($"Unesite novu vrednost za {funkcija}:");
string novaVrednost1 = Console.ReadLine();

// Slanje nove vrednosti serveru
string azuriranje = $"{imeUredjaja}:{funkcija}:{novaVrednost}";
porukaBytes = Encoding.UTF8.GetBytes(azuriranje);
clientSocket.SendTo(porukaBytes, serverEP);

Console.WriteLine($"Nova vrednost za {funkcija} je poslata serveru.");
}
else
{
Console.WriteLine("Server nije vratio validan odgovor.");
}

Console.WriteLine("Želite li poslati novu komandu? (da/ne):");
string nastavak = Console.ReadLine();
if (!nastavak.Equals("da", StringComparison.OrdinalIgnoreCase))
break;
}

Console.WriteLine("Klijent završava sa radom.");
clientSocket.Close();
}*/