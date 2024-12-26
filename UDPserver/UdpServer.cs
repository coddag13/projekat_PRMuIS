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
}
