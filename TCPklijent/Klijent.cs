﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Uredjaj;

namespace Klijent
{
    public class Klijent
    {
        List<(string, string)> IzabraneFunkcije = new List<(string, string)>();

        public static void Main(string[] args)
        {
            var klijent=new Klijent();
            while (true)
            {
                klijent.TCPKlijent();
            }
        }

        private void TCPKlijent()
        {
            try
            {
                using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    
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

                    string loginPodaci = $"{korisnickoIme}:{lozinka}";
                    clientSocket.Send(Encoding.UTF8.GetBytes(loginPodaci));

                    int received = clientSocket.Receive(buffer);
                    string odgovor = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Odgovor servera: {odgovor}");

                    /*if(odgovor =="USPESNO")
                    {
                        Korisnik korisnik=new Korisnik();
                        int port = korisnik.DodeliPort(korisnickoIme);
                        korisnik = new Korisnik(korisnickoIme, lozinka,true,port);
                        korisnik.PrikaziKorisnika();
                    }
                    else
                    {
                        Korisnik korisnik = new Korisnik(korisnickoIme, lozinka,false,0);
                        korisnik.PrikaziKorisnika();
                    }*/

                    if (odgovor == "USPESNO")
                    {
                        Console.WriteLine("Prijava uspešna.");
                        while (true)
                        {
                            UDPKlijent();  

                            Console.WriteLine("\nDa li želite ponovo da izaberete uređaj? (da/ne)");
                            string odgovorNaPitanje = Console.ReadLine();
                            if (odgovorNaPitanje.ToLower() != "da")
                            {
                                break; 
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

        private void UDPKlijent()
        {
            try
            {
                UdpClient udpClient = new UdpClient();
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 6000);
                BinaryFormatter formatter = new BinaryFormatter();

                string zahtev = "LISTA";
                byte[] zahtevBytes = Encoding.UTF8.GetBytes(zahtev);
                udpClient.Send(zahtevBytes, zahtevBytes.Length, serverEP);

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
                    //Console.WriteLine($"Korisnik je izabrao uredjaj:{izabraniUredjaj.Ime}");
                    //Console.WriteLine($"Azurirana vrednost funkcije:{funkcija} je {novaVrednost}");

                    using (MemoryStream ms = new MemoryStream())
                    {
                        formatter.Serialize(ms, izabraniUredjaj.Ime);
                        formatter.Serialize(ms, funkcija);
                        formatter.Serialize(ms, novaVrednost);
                        byte[] dataToSend = ms.ToArray();
                        udpClient.Send(dataToSend, dataToSend.Length, serverEP);
                    }

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

          