
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace UDPserver
{
    public class UdpServer
    {
        static void Main(string[] args)
        {
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
            udpSocket.Bind(serverEP);

            Console.WriteLine($"UDP Server pokrenut na {serverEP} i čeka poruke...");
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            UdpClient udpServer = new UdpClient(6000);
            Console.WriteLine("UDP server radi i slusa na portu 6000");
            IPEndPoint clientEndPoint=new IPEndPoint(IPAddress.Any , 0);
            
            byte[] baferPrijemnePoruke = new byte[1024];
            

            while (true)
            {
                byte[] received = udpServer.Receive(ref clientEndPoint);
                string poruka = Encoding.UTF8.GetString(received);
                Console.WriteLine($"Primljena poruka od {clientEP}: {poruka}");

                int brBajtova = udpSocket.ReceiveFrom(baferPrijemnePoruke, ref clientEP);
                string mess = Encoding.UTF8.GetString(baferPrijemnePoruke, 0, brBajtova);

                Console.WriteLine("\n----------------------------------------------------------------------------------------\n");
                Console.WriteLine($"Stiglo je {brBajtova} bajta od {clientEP}, poruka:\n{mess}");

                byte[] binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                brBajtova = udpSocket.SendTo(binarnaPoruka, 0, binarnaPoruka.Length, SocketFlags.None, clientEP); 
                Console.WriteLine($"Poslata je poruka duzine {brBajtova} ka {clientEP}");

                brBajtova = udpSocket.ReceiveFrom(baferPrijemnePoruke, ref clientEP); 
                poruka = Encoding.UTF8.GetString(baferPrijemnePoruke, 0, brBajtova);
                Console.WriteLine($"Poruka primljena od UDP KLIJENTA\nAzurirana vrijednost je->: {poruka}");

                binarnaPoruka = Encoding.UTF8.GetBytes(poruka);
                udpServer.Send(binarnaPoruka, binarnaPoruka.Length, clientEndPoint);

                break;
            }
            Console.WriteLine("Server zavrsava sa radom");
            udpSocket.Close(); 
            Console.ReadKey();
        }
    }
}
