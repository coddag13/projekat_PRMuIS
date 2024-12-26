/*using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

public class TCPServer
{
    private readonly Dictionary<string, string> korisnici;
    private readonly List<Uredjaji> uredjaji;

    public TCPServer()
    {
        // Lista korisnika (korisničko ime i lozinka)
        korisnici = new Dictionary<string, string>
        {
            { "teodora", "333" },
            { "danilo", "666" }
        };

        // Lista uređaja
        uredjaji = new List<Uredjaji>
        {
            new Svetlo(),
            new Klima(),
            new TV(),
            new Vrata()
        };
    }

    public void Pokreni()
    {
        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, 50000));
        serverSocket.Listen(5);

        Console.WriteLine("TCP server pokrenut. Čekam klijente...");

        while (true)
        {
            var klijentSocket = serverSocket.Accept();
            Console.WriteLine("Povezan klijent.");
            _ = ObradiKlijenta(klijentSocket); // Pokreće se obrada u asinkronom režimu
        }
    }

    private async Task ObradiKlijenta(Socket klijentSocket)
    {
        try
        {
            byte[] buffer = new byte[4096];
            BinaryFormatter formatter = new BinaryFormatter();

            // Prijava korisnika
            int brBajtova = klijentSocket.Receive(buffer);
            string[] podaci = Encoding.UTF8.GetString(buffer, 0, brBajtova).Split(':');

            if (podaci.Length == 2 && korisnici.TryGetValue(podaci[0], out var lozinka) && lozinka == podaci[1])
            {
                klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));

                // Slanje liste uređaja klijentu
                using (MemoryStream ms = new MemoryStream())
                {
                    formatter.Serialize(ms, uredjaji);
                    klijentSocket.Send(ms.ToArray());
                }

                Console.WriteLine("Lista uređaja poslata klijentu.");

                // Prijem naredbe za ažuriranje funkcije uređaja
                brBajtova = klijentSocket.Receive(buffer);
                using (MemoryStream ms = new MemoryStream(buffer, 0, brBajtova))
                {
                    string imeUredjaja = (string)formatter.Deserialize(ms);
                    string funkcija = (string)formatter.Deserialize(ms);
                    string novaVrednost = (string)formatter.Deserialize(ms);

                    // Pronađi uređaj i ažuriraj funkciju
                    var uredjaj = uredjaji.Find(u => u.Ime == imeUredjaja);
                    if (uredjaj != null)
                    {
                        try
                        {
                            uredjaj.AzurirajFunkciju(funkcija, novaVrednost);
                            Console.WriteLine($"Funkcija '{funkcija}' uređaja '{imeUredjaja}' ažurirana na '{novaVrednost}'.");

                            klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine(ex.Message);
                            klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Uređaj '{imeUredjaja}' nije pronađen.");
                        klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                    }
                }
            }
            else
            {
                klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greška: {e.Message}");
        }
        finally
        {
            klijentSocket.Close();
        }
    }

    public static void Main(string[] args)
    {
        var server = new TCPServer();
        server.Pokreni();
    }
}*/


using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;

public class TCPServer
{
    private Dictionary<string, string> korisnici;

    public TCPServer()
    {
        korisnici = new Dictionary<string, string>
        {
            { "teodora", "333" },
            { "danilo", "666" }
        };

    }

   

    public void Pokreni()
    {
        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, 50000));
        serverSocket.Listen(5);

        Console.WriteLine("TCP server pokrenut.");

        while (true)
        {
            var klijentSocket = serverSocket.Accept();
            Console.WriteLine("Povezan klijent.");

            Task.Run(() => ObradiKlijenta(klijentSocket));
        }
    }

    private void ObradiKlijenta(Socket klijentSocket)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int brBajtova = klijentSocket.Receive(buffer);
            string[] podaci = Encoding.UTF8.GetString(buffer, 0, brBajtova).Split(':');

            if (podaci.Length == 2 && korisnici.TryGetValue(podaci[0], out var lozinka) && lozinka == podaci[1])
            {
                klijentSocket.Send(Encoding.UTF8.GetBytes("Uspesno"));
                int udpPort = new Random().Next(50000, 60000);
                klijentSocket.Send(Encoding.UTF8.GetBytes(udpPort.ToString()));
            }
            else
            {
                klijentSocket.Send(Encoding.UTF8.GetBytes("Neuspesno"));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greška: {e.Message}");
        }
        finally
        {
            klijentSocket.Close();
        }
    }

    public static void Main(string[] args)
    {
        var server = new TCPServer();
        server.Pokreni();
    }
}
