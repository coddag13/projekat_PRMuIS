using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Uredjaj;

public class Server
{
    private readonly Dictionary<string, string> korisnici;

    private readonly Dictionary<string, Uredjaji> uredjaji;

    Dictionary<int, DateTime> korisnickiPortovi=new Dictionary<int, DateTime>();

    Korisnik korisnik = new Korisnik();
    public Server()
    {
        uredjaji = new Dictionary<string, Uredjaji>
        {
            { "Svetlo", new Svetlo() },
            { "Klima", new Klima() },
            { "TV", new TV() },
            { "Vrata", new Vrata() }
        };
       
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

        while (true)
        {
            var klijentSocket = serverSocket.Accept();
           // Mesavina(klijentSocket); 
           Serveri(klijentSocket); 
        }
    }
   
    public static void Main(string[] args)
    {
        var server = new Server();
        server.Pokreni();
    }
    private void Serveri(Socket klijentSocket)
    {
        try
        {
            UdpClient udpServer = new UdpClient(6000);
            Console.WriteLine("Server je pokrenut..");
            IPEndPoint udpClientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            klijentSocket.Blocking = false;

            while (true)
            {
                // Prvo proveravamo da li je TCP soket spreman za čitanje
                List<Socket> readSockets = new List<Socket> { klijentSocket };
                Socket.Select(readSockets, null, null, 10000); // Timeout 10 sekundi

                if (readSockets.Count > 0)
                {
                    // Obrada TCP klijenta
                    byte[] buffer = new byte[4096];
                    int brBajtova = klijentSocket.Receive(buffer);
                    string[] podaci = Encoding.UTF8.GetString(buffer, 0, brBajtova).Split(':');
                    Console.WriteLine("Povezan klijent.");

                    if (podaci.Length != 2)
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("GRESKA"));
                        continue;
                    }

                    string korisnickoIme = podaci[0];
                    string lozinka = podaci[1];

                    int port = korisnik.DodeliPort(korisnickoIme);
                    korisnickiPortovi.Add(port, DateTime.Now);

                    if (korisnici.TryGetValue(korisnickoIme, out var validnaLozinka) && validnaLozinka == lozinka)
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));
                    }
                    else
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                    }
                }

                // Obrada UDP klijenta (isti princip sa UDP)
                List<int> zatvaranjePortova = new List<int>();
                foreach (var portInfo in korisnickiPortovi)
                {
                    if ((DateTime.Now - portInfo.Value).TotalMinutes > 1)
                        if ((DateTime.Now - portInfo.Value).TotalSeconds > 30)
                        {
                            zatvaranjePortova.Add(portInfo.Key);
                        }
                }
                foreach (int port2 in zatvaranjePortova)
                {
                    korisnickiPortovi.Remove(port2);
                    Console.WriteLine($"Sesija za port {port2} je istekla. Port je zatvoren.");
                    string porukaZaKlijenta = "Sesija je istekla. Ponovno logovanje...";
                    Console.WriteLine($"{porukaZaKlijenta}");
                    byte[] porukaBytes = Encoding.UTF8.GetBytes(porukaZaKlijenta);

                    try
                    {
                        udpServer.Send(porukaBytes, porukaBytes.Length, udpClientEndPoint);
                        Console.WriteLine("Poruka o isteku sesije poslata klijentu.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška pri slanju poruke: {ex.Message}");
                    }
                }

                // Provera UDP soketa
                List<Socket> readUdpSockets = new List<Socket> { udpServer.Client };
                Socket.Select(readUdpSockets, null, null, 10000);

                if (readUdpSockets.Count > 0)
                {
                    byte[] receivedBytes = udpServer.Receive(ref udpClientEndPoint);
                    string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);

                    ObradaKomande(klijentSocket, primljenaPoruka, udpServer, udpClientEndPoint, receivedBytes);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greška: {e.Message}");
        }
    }
    /* private void Mesavina(Socket klijentSocket)
     {
         UdpClient udpServer = new UdpClient(6000);
         Console.WriteLine("Server je pokrenut..");
         IPEndPoint udpClientEndPoint = new IPEndPoint(IPAddress.Any, 0);

             try
             {
                 byte[] buffer = new byte[4096];
                 BinaryFormatter formatter = new BinaryFormatter();
                 int brBajtova = klijentSocket.Receive(buffer);
                 string[] podaci = Encoding.UTF8.GetString(buffer, 0, brBajtova).Split(':');
                 Console.WriteLine("Povezan klijent.");

                 if (podaci.Length != 2)
                 {
                     klijentSocket.Send(Encoding.UTF8.GetBytes("GRESKA"));
                     return;
                 }

                 string korisnickoIme = podaci[0];
                 string lozinka = podaci[1];

                 int port = korisnik.DodeliPort(korisnickoIme);
                 korisnickiPortovi.Add(port, DateTime.Now);

                 if (korisnici.TryGetValue(korisnickoIme, out var validnaLozinka) && validnaLozinka == lozinka)
                 {
                     klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));
                 }
                 else
                 {
                     klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                 }



                 List<Socket> readSockets = new List<Socket> { udpServer.Client };
                 Socket.Select(readSockets, null, null, 10000);

                 if (readSockets.Count > 0)
                 {
                     byte[] receivedBytes = udpServer.Receive(ref udpClientEndPoint);
                     string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);

                     ObradaKomande(klijentSocket,primljenaPoruka, udpServer, udpClientEndPoint, receivedBytes);
                 }

             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Greška: {ex.Message}");

            }
        

    }

    /*private  void ServerMetode(Socket klijentSocket)
    {
        UdpClient udpServer = new UdpClient(6000); // Napravite UDP server ovde

        try
        {
            while (true)
            {
                byte[] buffer = new byte[4096];
                int brBajtova = klijentSocket.Receive(buffer);
                string[] podaci = Encoding.UTF8.GetString(buffer, 0, brBajtova).Split(':');
                Console.WriteLine("Povezan klijent.");

                if (podaci.Length != 2)
                {
                    klijentSocket.Send(Encoding.UTF8.GetBytes("GRESKA"));
                    return;
                }

                string korisnickoIme = podaci[0];
                string lozinka = podaci[1];

                if (korisnici.TryGetValue(korisnickoIme, out var validnaLozinka) && validnaLozinka == lozinka)
                {
                    klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));
                }
                else
                {
                    klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                }

                // Ovdje pokrećemo UDP komunikaciju sa klijentom
                List<Socket> readSockets = new List<Socket> { udpServer.Client };
                Socket.Select(readSockets, null, null, 10000); // čekaj na podatke
                if (readSockets.Count > 0)
                {
                    IPEndPoint udpClientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = udpServer.Receive(ref udpClientEndPoint);
                    string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);
                    ObradaKomande(klijentSocket, primljenaPoruka, udpServer, udpClientEndPoint, receivedBytes);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greška: {e.Message}");
        }
    }*/

    /*private void ServerMetode1(Socket klijentSocket)
    {
        
        using (var udpServer = new UdpClient(6000))
        {
            IPEndPoint udpClientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    BinaryFormatter formatter = new BinaryFormatter();
                    int brBajtova = klijentSocket.Receive(buffer);
                    string[] podaci = Encoding.UTF8.GetString(buffer, 0, brBajtova).Split(':');
                    Console.WriteLine("Povezan klijent.");

                    if (podaci.Length != 2)
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("GRESKA"));
                        return;
                    }

                    string korisnickoIme = podaci[0];
                    string lozinka = podaci[1];

                    int port = korisnik.DodeliPort(korisnickoIme);
                    korisnickiPortovi.Add(port, DateTime.Now);

                    if (korisnici.TryGetValue(korisnickoIme, out var validnaLozinka) && validnaLozinka == lozinka)
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));
                    }
                    else
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                    }

                    List<int> zatvaranjePortova = new List<int>();
                    foreach (var portInfo in korisnickiPortovi)
                    {
                        if ((DateTime.Now - portInfo.Value).TotalSeconds > 30)
                        {
                            zatvaranjePortova.Add(portInfo.Key);
                        }
                    }

                    foreach (int port2 in zatvaranjePortova)
                    {
                        korisnickiPortovi.Remove(port2);
                        Console.WriteLine($"Sesija za port {port2} je istekla. Port je zatvoren.");
                        string porukaZaKlijenta = "Sesija je istekla. Ponovno logovanje...";
                        byte[] porukaBytes = Encoding.UTF8.GetBytes(porukaZaKlijenta);
                        udpServer.Send(porukaBytes, porukaBytes.Length, udpClientEndPoint);
                    }

                   
                    List<Socket> readSockets = new List<Socket> { udpServer.Client };
                    Socket.Select(readSockets, null, null, 10000);
                    if (readSockets.Count > 0)
                    {
                        byte[] receivedBytes = udpServer.Receive(ref udpClientEndPoint);
                        string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);
                        ObradaKomande(klijentSocket,primljenaPoruka, udpServer, udpClientEndPoint, receivedBytes);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Greška: {e.Message}");
            }
        }
    }*/


    private  void ObradaKomande(Socket klijentSocket,string primljenaPoruka, UdpClient udpServer, IPEndPoint udpClientEndPoint, byte[] receivedBytes)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        klijentSocket.Blocking = false;
       
            if (primljenaPoruka == "LISTA")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    formatter.Serialize(ms, uredjaji.Values.ToList());
                    byte[] response = ms.ToArray();
                    udpServer.Send(response, response.Length, udpClientEndPoint);
                }
                Console.WriteLine("Lista je poslata klijentu.");
            }
            else if (receivedBytes != null && receivedBytes.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(receivedBytes))
                {
                    string deviceName = (string)formatter.Deserialize(ms);
                    string function = (string)formatter.Deserialize(ms);
                    string newValue = (string)formatter.Deserialize(ms);

                    if (uredjaji.TryGetValue(deviceName, out var device))
                    {
                        device.AzurirajFunkciju(function, newValue);
                        string status = device.DobijStanje();
                        Console.WriteLine($"Obrada komande za uređaj: {deviceName}, funkcija: {function}, nova vrednost: {newValue}");

                        byte[] response = Encoding.UTF8.GetBytes($"Uspješno: {status}");
                        device.PrikaziSveUredjaje(uredjaji.Values.ToList());

                        udpServer.Send(response, response.Length, udpClientEndPoint);
                    }
                    else
                    {
                        byte[] response = Encoding.UTF8.GetBytes("Greška: Uređaj nije pronađen.");
                        udpServer.Send(response, response.Length, udpClientEndPoint);
                    }
                }
            }
        
    }
    
}

