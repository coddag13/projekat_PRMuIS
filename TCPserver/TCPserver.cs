using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Uredjaj;
using UDPserver;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;




namespace TCPserver
{
    public class TCPserver
    {
        private static UdpServer udpServer;
        static void Main(string[] args)
        {
            Random random = new Random();
            Uredjajii u = new Uredjajii();

            List<Uredjajii> uredjaji = u.SviUredjaji();
            Dictionary<string, string> korisnici = new Dictionary<string, string>
            {
                { "teodora", "333" },
                { "danilo", "666" }
            };
           
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);

            serverSocket.Bind(serverEP);

            serverSocket.Listen(5);


            Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {serverEP}");

            Socket acceptedSocket = serverSocket.Accept();

            IPEndPoint clientEP = acceptedSocket.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Povezao se novi klijent! Njegova adresa je {clientEP}");

         
            byte[] buffer = new byte[4096];
            BinaryFormatter formatter = new BinaryFormatter();


            while (true)
            {
                try
                {
                    int brBajta = acceptedSocket.Receive(buffer);
                    if (brBajta == 1)
                    {
                        Console.WriteLine("Klijent je zavrsio sa radom");
                        break;
                    }
                    string poruka = Encoding.UTF8.GetString(buffer, 0, brBajta);
                    Console.WriteLine("Poruka klijenta: " + poruka + " " + brBajta);

                    string[] djelovi = poruka.Split(':');
                    Console.WriteLine(djelovi.Length + " " + djelovi[0] + " " + djelovi[1]);
                    if (djelovi.Length == 2 && korisnici.ContainsKey(djelovi[0]) && korisnici[djelovi[0]] == djelovi[1])
                    {
                        Console.WriteLine("Prijavljivanje je uspesno\n");
                        string odgovor = "USPESNO";
                        brBajta = acceptedSocket.Send(Encoding.UTF8.GetBytes(odgovor));

                        int udpPort = random.Next(50000, 60000);
                        using (MemoryStream ms = new MemoryStream())
                        {

                            formatter.Serialize(ms, udpPort);
                            formatter.Serialize(ms, uredjaji);

                            
                            acceptedSocket.Send(ms.ToArray());
                        }

                        brBajta = acceptedSocket.Receive(buffer); ;
                        string komanda = "";
                        string imeUredjaja = "";
                        using (MemoryStream ms = new MemoryStream(buffer, 0, brBajta))
                        {
                            komanda = (string)formatter.Deserialize(ms);
                            komanda = komanda.ToLower();
                            imeUredjaja = (string)formatter.Deserialize(ms);
                            Console.WriteLine("Funkcija->" + imeUredjaja);
                            Console.WriteLine("Komanda->" + komanda);
                           
                        }


                        UdpClient udpClient = new UdpClient();
                        IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Loopback, 6000); // UDP Server na portu 6000

                        byte[] messageBytes = Encoding.UTF8.GetBytes(komanda + ":" + imeUredjaja);
                        udpClient.Send(messageBytes, messageBytes.Length, udpEndPoint);
                        Console.WriteLine($"Poruka: {komanda} {imeUredjaja} je  poslata UDP serveru.");

                        messageBytes = udpClient.Receive(ref udpEndPoint);
                        string message = Encoding.UTF8.GetString(messageBytes);
                        Console.WriteLine($"Poruka od udp servera->Azurirana vrijednost funkcije {komanda} za {imeUredjaja} je " + message);
                        foreach (var s in uredjaji)
                        {
                            if (s.Ime == imeUredjaja)
                            {
                                s.Azuriranje(komanda, message);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Prijavljivanje nije uspesno\n");
                        string odgovor = "NEUSPESNO";
                        brBajta = acceptedSocket.Send(Encoding.UTF8.GetBytes(odgovor));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            Console.WriteLine("Server zavrsava sa radom");
            Console.ReadKey();
            acceptedSocket.Close();
            serverSocket.Close();
        }
    }
}
