/*using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace UDPKlijent
{
    public class UdpKlijent
    {
        public static void Main(string[] args)
        {
            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 6000); // Server endpoint

                Console.WriteLine("Klijent je povezan na server.");
                while (true)
                {
                    // Slanje inicijalne poruke serveru
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
                        string novaVrednost = Console.ReadLine();

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}*/


using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPklijent
{
    public class UDPklijent
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 6000);

                    Console.WriteLine("Unesite komandu za uređaj (npr. 'Svetlo:intezitet=70%'):");
                    string komanda = Console.ReadLine();

                    byte[] messageBytes = Encoding.UTF8.GetBytes(komanda);
                    udpClient.Send(messageBytes, messageBytes.Length, serverEP);

                    IPEndPoint responseEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] responseBytes = udpClient.Receive(ref responseEP);
                    string response = Encoding.UTF8.GetString(responseBytes);

                    Console.WriteLine($"Odgovor servera: {response}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}
