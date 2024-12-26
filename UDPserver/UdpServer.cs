/*using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpServer
{
    private readonly Dictionary<string, Uredjaji> uredjaji;

    public UdpServer()
    {
        // Lista uređaja sa inicijalnim vrednostima
        uredjaji = new Dictionary<string, Uredjaji>
        {
            { "Svetlo", new Svetlo() },
            { "Klima", new Klima() },
            { "TV", new TV() },
            { "Vrata", new Vrata() }
        };
    }

    public void Pokreni()
    {
        // Kreiranje UDP servera na portu 6000
        UdpClient udpServer = new UdpClient(6000);
        Console.WriteLine("UDP Server je pokrenut i osluškuje na portu 6000...");

        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                // Primanje poruke od TCP servera ili UDP klijenta
                byte[] receivedBytes = udpServer.Receive(ref clientEndPoint);
                string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);
                Console.WriteLine($"Poruka primljena od klijenta ({clientEndPoint}): {primljenaPoruka}");

                // Parsiranje poruke
                string[] delovi = primljenaPoruka.Split(':');
                if (delovi.Length == 3 && uredjaji.TryGetValue(delovi[0], out var uredjaj))
                {
                    string imeUredjaja = delovi[0];
                    string funkcija = delovi[1];
                    string novaVrednost = delovi[2];

                    // Ažuriranje funkcije uređaja
                    uredjaj.AzurirajFunkciju(funkcija, novaVrednost);
                    Console.WriteLine($"Uređaj '{imeUredjaja}' ažuriran: {uredjaj.DobijStanje()}");

                    // Slanje odgovora nazad klijentu
                    string odgovor = $"Uspješno ažurirano: {uredjaj.DobijStanje()}";
                    byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                    udpServer.Send(odgovorBytes, odgovorBytes.Length, clientEndPoint);
                }
                else
                {
                    // Neispravna poruka ili uređaj ne postoji
                    string greska = "Greška: Neispravna poruka ili uređaj ne postoji.";
                    Console.WriteLine(greska);
                    byte[] greskaBytes = Encoding.UTF8.GetBytes(greska);
                    udpServer.Send(greskaBytes, greskaBytes.Length, clientEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }

    public static void Main(string[] args)
    {
        var server = new UdpServer();
        server.Pokreni();
    }
}*/



using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

public class UDPServer
{
    private Dictionary<string, Uredjaji> uredjaji;

    public UDPServer()
    {
        uredjaji = new Dictionary<string, Uredjaji>
        {
            { "Svetlo", new Svetlo() },
            { "Klima", new Klima() },
            { "TV", new TV() },
            { "Vrata", new Vrata() }
        };

    }

   

    public void Pokreni()
    {
        var udpSocket = new UdpClient(6000);
        Console.WriteLine("UDP server pokrenut na portu 6000.");

        while (true)
        {
            IPEndPoint klijentEP = null;
            byte[] receivedBytes = udpSocket.Receive(ref klijentEP);

            string poruka = Encoding.UTF8.GetString(receivedBytes);
            string[] delovi = poruka.Split(':');
            if (delovi.Length == 3 && uredjaji.TryGetValue(delovi[0], out var uredjaj))
            {
                uredjaj.AzurirajFunkciju(delovi[1], delovi[2]);
                string odgovor = $"Uredjaj {uredjaj.Ime} ažuriran: {uredjaj.DobijStanje()}";
                udpSocket.Send(Encoding.UTF8.GetBytes(odgovor), odgovor.Length, klijentEP);
            }
        }
    }

    public static void Main(string[] args)
    {
        var server = new UDPServer();
        server.Pokreni();
    }

}
