/*using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TCPklijent
{
    public class TCPklijent
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    Console.WriteLine("Povezivanje na server...");
                    clientSocket.Connect("127.0.0.1", 50000); // Server na localhost:50000
                    Console.WriteLine("Povezano!");

                    byte[] buffer = new byte[4096];
                    BinaryFormatter formatter = new BinaryFormatter();

                    while (true)
                    {
                        // Unos podataka za prijavu
                        Console.WriteLine("Unesite korisničko ime: ");
                        string korisnickoIme = Console.ReadLine();
                        Console.WriteLine("Unesite lozinku: ");
                        string lozinka = Console.ReadLine();

                        string loginPodaci = $"{korisnickoIme}:{lozinka}";
                        clientSocket.Send(Encoding.UTF8.GetBytes(loginPodaci));

                        // Odgovor servera
                        int received = clientSocket.Receive(buffer);
                        string odgovor = Encoding.UTF8.GetString(buffer, 0, received);
                        Console.WriteLine($"Odgovor servera: {odgovor}");

                        if (odgovor == "USPESNO")
                        {
                            // Primanje liste uređaja
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

                            // Biranje uređaja
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

                            // Menjanje funkcija uređaja
                            Console.WriteLine("Unesite ime funkcije koju želite da promenite:");
                            string funkcijaZaPromenu = Console.ReadLine();

                            if (izabraniUredjaj.Funkcije.ContainsKey(funkcijaZaPromenu))
                            {
                                Console.WriteLine("Unesite novu vrednost za funkciju:");
                                string novaVrednost = Console.ReadLine();

                                using (MemoryStream msSend = new MemoryStream())
                                {
                                    formatter.Serialize(msSend, izabraniUredjaj.Ime);
                                    formatter.Serialize(msSend, funkcijaZaPromenu);
                                    formatter.Serialize(msSend, novaVrednost);

                                    clientSocket.Send(msSend.ToArray());
                                }
                                Console.WriteLine($"Funkcija {funkcijaZaPromenu} ažurirana.");
                            }
                            else
                            {
                                Console.WriteLine("Nepoznata funkcija. Pokušajte ponovo.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Prijavljivanje nije uspelo, pokušajte ponovo!");
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}*/

using System;
using System.Net.Sockets;
using System.Text;

namespace TCPklijent
{
    public class TCPklijent
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    Console.WriteLine("Povezivanje na server...");
                    clientSocket.Connect("127.0.0.1", 50000); // Server na localhost:50000
                    Console.WriteLine("Povezano!");

                    Console.WriteLine("Unesite korisničko ime: ");
                    string korisnickoIme = Console.ReadLine();
                    Console.WriteLine("Unesite lozinku: ");
                    string lozinka = Console.ReadLine();

                    string loginPodaci = $"{korisnickoIme}:{lozinka}";
                    clientSocket.Send(Encoding.UTF8.GetBytes(loginPodaci));

                    byte[] buffer = new byte[1024];
                    int received = clientSocket.Receive(buffer);
                    string odgovor = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Odgovor servera: {odgovor}");

                    if (odgovor == "USPESNO")
                    {
                        // Dalji koraci, npr. prikaz uređaja i izbor funkcije
                        Console.WriteLine("Prijava uspešna. Dobijate listu uređaja...");
                        // Logika za izbor uređaja i slanje komandi može ići ovde.
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
}
