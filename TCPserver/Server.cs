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

        Console.WriteLine("TCP server pokrenut. Čekam klijente...");

        while (true)
        {
            var klijentSocket = serverSocket.Accept();
            Console.WriteLine("Povezan klijent.");
            Task.Run(() => TCPServer(klijentSocket));
            Task.Run(() =>
            {
                UDPServer(klijentSocket);
            });
        }
    }

    private void TCPServer(Socket klijentSocket)
    {
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

                if (korisnici.TryGetValue(korisnickoIme, out var validnaLozinka) && validnaLozinka == lozinka)
                {
                    klijentSocket.Send(Encoding.UTF8.GetBytes("USPESNO"));
                }
                else
                {
                    klijentSocket.Send(Encoding.UTF8.GetBytes("NEUSPESNO"));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greška: {e.Message}");
        }  
    }

    private void UDPServer(Socket klijentSocket)
    {
        Korisnik korisnik=new Korisnik();
        
        
        UdpClient udpServer = new UdpClient(6000);
        Console.WriteLine("Server je pokrenut..");

        IPEndPoint udpClientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        klijentSocket.Blocking = false;

        Random random = new Random();
        int ciklus=random.Next(10, 50);

        while (true)
        {
            try
            {
                List<Socket> readSockets = new List<Socket> { udpServer.Client };
                Socket.Select(readSockets, null, null, 1000000);
                string korisnickoIme = korisnik.DodeliIme();
                int port = korisnik.DodeliPort(korisnickoIme);
                

                if (readSockets.Count > 0)
                {
                    byte[] receivedBytes = udpServer.Receive(ref udpClientEndPoint);
                    string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);

                    ObradaKomande(primljenaPoruka, udpServer, udpClientEndPoint, receivedBytes);
                }

                
                string vremenskaOznaka = DateTime.Now.ToString("HH:mm:ss");
                string[] vreme = vremenskaOznaka.Split(':');
                int s = int.Parse(vreme[2]);
                if (s==ciklus)
                {
                    port = 0;
                    Console.WriteLine("Port je zatvoren.Ako zelite da nastavite morate opet da se prijavite.");
                    korisnik.DodeliPort(korisnickoIme);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
                
            }
        }
    }

    private void ObradaKomande(string primljenaPoruka, UdpClient udpServer, IPEndPoint udpClientEndPoint, byte[] receivedBytes)
    {
        BinaryFormatter formatter = new BinaryFormatter();

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
                    //device.PrikaziUredjaj(newValue,status,function);
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


    public static void Main(string[] args)
    {
        var server = new Server();
        server.Pokreni();
    }

}

