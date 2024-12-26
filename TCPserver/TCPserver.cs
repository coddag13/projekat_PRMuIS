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
}
