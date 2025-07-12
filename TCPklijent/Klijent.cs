using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Uredjaj;
using System.Threading;

namespace Klijenti
{
    public class Klijent
    {
        List<(string, string)> IzabraneFunkcije = new List<(string, string)>();
        private readonly Dictionary<string, string> korisnici;
        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 6000);
        BinaryFormatter formatter = new BinaryFormatter();

        public static void Main(string[] args)
        {
            var klijent = new Klijent();

            klijent.TCPKlijent();
        }

        public Klijent()
        {
            korisnici = new Dictionary<string, string>
            {
                { "teodora", "333" },
                { "danilo", "666" }
            };
        }

        public void TCPKlijent()
        {
            while (true)
            {
                try
                {
                    using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {

                        Korisnik korisnik = new Korisnik();
                        var klijent = new Klijent();
                        Console.WriteLine("Povezivanje na server...");
                        clientSocket.Connect("127.0.0.1", 50000);
                        Console.WriteLine("Povezano!");

                        byte[] buffer = new byte[4096];
                        BinaryFormatter formatter = new BinaryFormatter();

                        Console.WriteLine("Unesite korisničko ime: ");
                        string korisnickoIme = Console.ReadLine();
                        Console.WriteLine("Unesite lozinku: ");
                        string lozinka = Console.ReadLine();

                        while (!(korisnici.TryGetValue(korisnickoIme, out var validnaLozinka) && validnaLozinka == lozinka))
                        {
                            Console.WriteLine("Unesite korisničko ime: ");
                            korisnickoIme = Console.ReadLine();
                            Console.WriteLine("Unesite lozinku: ");
                            lozinka = Console.ReadLine();
                        }

                        string loginPodaci = $"{korisnickoIme}:{lozinka}";
                        clientSocket.Send(Encoding.UTF8.GetBytes(loginPodaci));

                        int received = clientSocket.Receive(buffer);
                        string odgovor = Encoding.UTF8.GetString(buffer, 0, received);
                        Console.WriteLine($"Odgovor servera: {odgovor}");

                        if (odgovor == "USPESNO")
                        {
                            Console.WriteLine("Prijava uspešna.");
                            int primljena = clientSocket.Receive(buffer);
                            string odgovor1 = Encoding.UTF8.GetString(buffer, 0, primljena);

                            Console.WriteLine($"Dodeljeni port za korisnika je {odgovor1}");

                            while (true)
                            {
                                UDPKlijent();
                                string odgovorNaPitanje;

                                do
                                {
                                    Console.WriteLine("\nDa li želite ponovo da izaberete uređaj? (da/ne)");
                                    odgovorNaPitanje = Console.ReadLine();

                                } while (odgovorNaPitanje.ToLower() != "da" && odgovorNaPitanje.ToLower() != "ne");

                                if (odgovorNaPitanje.ToLower() == "ne")
                                {
                                   
                                    string zahtev = "ne";
                                    byte[] zahtevBytes = Encoding.UTF8.GetBytes(zahtev);
                                    udpSocket.SendTo(zahtevBytes, serverEP);
                                    Thread.Sleep(1000);
                                    udpSocket.Close();
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    Console.Clear();
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Prijava neuspešna.");
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška: {ex.Message}");
                }
            }
        }

        public void UDPKlijent()
        {
            try
            {
                string zahtev = "LISTA";
                byte[] zahtevBytes = Encoding.UTF8.GetBytes(zahtev);
                udpSocket.SendTo(zahtevBytes, serverEP);

                EndPoint remoteEP = (EndPoint)serverEP;
                byte[] responseBytes = new byte[9000];
                int receivedBytes = udpSocket.ReceiveFrom(responseBytes, ref remoteEP);
                string poruka = Encoding.UTF8.GetString(responseBytes, 0, receivedBytes);

                if (poruka == "Sesija je istekla. Ponovno logovanje...")
                {
                    Console.Clear();
                    Console.WriteLine("Sesija je istekla. Bićete vraćeni na prijavu.");

                    IzabraneFunkcije.Clear();

                    udpSocket.Close();
                    udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    TCPKlijent();
                    return;
                }
                else
                {
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
                            IzabraneFunkcije.Add((funkcija1.Key, funkcija1.Value));
                        }

                        Console.WriteLine("Unesite ime funkcije za promenu:");
                        string funkcija = Console.ReadLine();
                        bool funkcijaPronadjena = false;

                        while (!funkcijaPronadjena)
                        {
                            for (int i = 0; i < IzabraneFunkcije.Count; i++)
                            {
                                if (funkcija == IzabraneFunkcije[i].Item1)
                                {
                                    funkcijaPronadjena = true;
                                    break;
                                }
                            }

                            if (!funkcijaPronadjena)
                            {
                                Console.WriteLine("Funkcija nije pronađena. Ponovo unesite ime funkcije za promenu:");
                                funkcija = Console.ReadLine();
                            }
                        }

                        Console.WriteLine("Unesite novu vrednost:");
                        string novaVrednost = Console.ReadLine();

                        using (MemoryStream ms = new MemoryStream())
                        {
                            formatter.Serialize(ms, izabraniUredjaj.Ime);
                            formatter.Serialize(ms, funkcija);
                            formatter.Serialize(ms, novaVrednost);
                            byte[] dataToSend = ms.ToArray();
                            udpSocket.SendTo(dataToSend, serverEP);
                        }

                        responseBytes = new byte[9000];
                        receivedBytes = udpSocket.ReceiveFrom(responseBytes, ref remoteEP);
                        string odgovor1 = Encoding.UTF8.GetString(responseBytes, 0, receivedBytes);

                        Console.WriteLine($"Odgovor servera: {odgovor1}");
                    }
                    else
                    {
                        Console.WriteLine("Pogrešan izbor uređaja.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}

