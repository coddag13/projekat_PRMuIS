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
