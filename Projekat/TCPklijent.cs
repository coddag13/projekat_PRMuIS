using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace TCPklijent
{
    public class TCPklijent
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 50000);

            Console.WriteLine("Povezivanje sa TCP serverom...");
            clientSocket.Connect(serverEP);
            Console.WriteLine("Uspesno povezano!");

            Console.WriteLine("Unesite korisnicko ime i lozinku (format: korisnik:lozinka):");
            string login = Console.ReadLine();
            clientSocket.Send(Encoding.UTF8.GetBytes(login));

            byte[] buffer = new byte[1024];
            int receivedBytes = clientSocket.Receive(buffer);
            string odgovor = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
            Console.WriteLine($"Odgovor servera: {odgovor}");

            clientSocket.Close();
        }
    }
}
