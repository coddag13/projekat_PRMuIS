using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uredjaj;
using Microsoft.CodeAnalysis.Formatting;


namespace TCPklijent
{
    public class TCPklijenti
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50000);
            byte[] buffer= new byte[4096];


            Console.WriteLine("Povezivanje sa TCP serverom...");
            clientSocket.Connect(serverEP);
            Console.WriteLine("Uspesno povezano!");

            while (true)
            {
                try
                {
                    Console.WriteLine("Unesite username: ");
                    string korisnickoIme=Console.ReadLine();
                    Console.WriteLine("Unesite lozinku: ");
                    string lozinka=Console.ReadLine();
                    string provjeraUnosa = $"{korisnickoIme}:{lozinka}";
                    int brBajt = clientSocket.Send(Encoding.UTF8.GetBytes(provjeraUnosa));
                    if(brBajt == 0)
                    {
                        break;
                    }
                    brBajt = clientSocket.Receive(buffer);
                    string odg = Encoding.UTF8.GetString(buffer, 0, brBajt);

                    Console.WriteLine("Prijava: "+ odg);
                    if(odg=="Uspjesna")
                    {
                        List<Uredjajii> uredjaji = new List<Uredjajii>();
                        brBajt = clientSocket.Receive(buffer);
                        odg = Encoding.UTF8.GetString(buffer, 0, brBajt);
                        int udpPort = 0;
                        using (MemoryStream ms = new MemoryStream(buffer, 0, brBajt))
                        {
                            udpPort = (int)formatter.Deserialize(ms);
                            List<Uredjajii> lista = (List<Uredjajii>)formatter.Deserialize(ms);
                            uredjaji = lista;
                          
                        }
                        Console.WriteLine("Lista dostupnih uredjaja:");
                        for (int i = 0; i < uredjaji.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {uredjaji[i].Ime} (Port: {uredjaji[i].Port = udpPort})");
                        }

                        // Biranje uređaja
                        int izbor;
                        do
                        {
                            Console.WriteLine("Unesite redni broj uredjaja koji zelite da podesite:");
                            izbor = Int32.Parse(Console.ReadLine()) - 1;
                        } while (izbor < 0 || izbor >= uredjaji.Count);

                        Uredjajii izabrani = uredjaji[izbor];
                        Console.WriteLine($"Izabrali ste: {izabrani.Ime}");
                        Console.WriteLine("[Ime funkcije | Vrednost]");
                        Console.WriteLine("--------------------------");
                        foreach (var v in izabrani.Funkcije)
                        {
                            Console.WriteLine("--" + v.ToString());
                        }
                        // Promjena funkcija 
                        Console.WriteLine("Unesite funkciju kojoj je potrebna promjena:");
                        string funkcija = Console.ReadLine();
                        using (MemoryStream ms = new MemoryStream())
                        {

                            formatter.Serialize(ms, funkcija);
                            formatter.Serialize(ms, izabrani.Ime.ToString());

                            //byte[] data = ms.ToArray();
                            clientSocket.Send(ms.ToArray());
                        }
                    }
                    else
                    {
                        Console.WriteLine("Neuspjesna prijava,molim Vas unesite validne parametre");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            Console.WriteLine("Klijent je zavrsio sa svojim radom");
            Console.ReadKey();
            clientSocket.Close();

            Console.WriteLine("Unesite korisnicko ime i lozinku (format: korisnik:lozinka):");
            string login = Console.ReadLine();
            clientSocket.Send(Encoding.UTF8.GetBytes(login));

           
            int receivedBytes = clientSocket.Receive(buffer);
            string odgovor = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Console.WriteLine($"Odgovor servera: {odgovor}");

            clientSocket.Close();
        }
    }
}
