using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
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

    private readonly List<(string,int)> klijenti = new List<(string,int)>();

    private UdpClient udpServer = new UdpClient(6000); // Jedan UdpClient za ceo server

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
            Task.Run(() => Serveri(klijentSocket));
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
            Console.WriteLine("Server je pokrenut...");
            
            IPEndPoint udpClientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            klijentSocket.Blocking = false;

            Uredjaji device = null;
            string korisnickoIme = null;
            int port = 0;

            while (true)
            {

                List<Socket> readSockets = new List<Socket> { klijentSocket };
                Socket.Select(readSockets, null, null, 10000);

                if (readSockets.Count > 0)
                {
                    byte[] buffer = new byte[4096];
                    int brBajtova = klijentSocket.Receive(buffer);
                    string[] podaci = Encoding.UTF8.GetString(buffer, 0, brBajtova).Split(':');
                    Console.WriteLine("Povezan klijent.");

                    if (podaci.Length != 2)
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("GRESKA"));
                        continue;
                    }

                    korisnickoIme = podaci[0];
                    string lozinka = podaci[1];

                    port = korisnik.DodeliPort(korisnickoIme);
                    korisnickiPortovi[port] = DateTime.Now;

                    if (korisnici.TryGetValue(korisnickoIme, out var validnaLozinka) && validnaLozinka == lozinka)
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));
                        klijenti.Add((korisnickoIme, port));
                    }
                    else
                    {
                        klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                    }
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
                    klijenti.Remove((korisnickoIme, port2));

                    udpServer.Send(porukaBytes, porukaBytes.Length, udpClientEndPoint);

                    break;
                }

                List<Socket> readUdpSockets = new List<Socket> { udpServer.Client };
                Socket.Select(readUdpSockets, null, null, 10000);

                if (readUdpSockets.Count > 0)
                {
                    byte[] receivedBytes = udpServer.Receive(ref udpClientEndPoint);
                    string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);

                    if (primljenaPoruka == "ne")
                    {
                        Console.Clear();
                        Console.WriteLine($"Korisnik: {korisnickoIme} i njegov port je: {port}");
                        device?.PrikaziSveUredjaje(uredjaji.Values.ToList());
                    }

                    device = ObradaKomande(klijentSocket, primljenaPoruka, udpServer, udpClientEndPoint, receivedBytes);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greška: {e.Message}");
        }
    }



    private Uredjaji ObradaKomande(Socket klijentSocket,string primljenaPoruka, UdpClient udpServer, IPEndPoint udpClientEndPoint, byte[] receivedBytes)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        //klijentSocket.Blocking = false;
        Uredjaji device=null;
        
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

                    if (uredjaji.TryGetValue(deviceName, out device))
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
        return device;
    } 
}

