using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;



public class UdpServer
{
    private readonly Dictionary<string, Uredjaji> uredjaji;
    public UdpServer()
    {
        uredjaji = new Dictionary<string, Uredjaji>
        {
            { "Svetlo", new Svetlo() },
            { "Klima", new Klima() },
            { "TV", new TV() },
            { "Vrata", new Vrata() }
        };
    }

    /*public void Pokreni()
    {
        UdpClient udpServer = new UdpClient(6000);
        Console.WriteLine("UDP Server je pokrenut i osluškuje na portu 6000...");

        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        BinaryFormatter formatter = new BinaryFormatter();

        while (true)
        {
            try
            {
                byte[] receivedBytes = udpServer.Receive(ref clientEndPoint);
                string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);
                Console.WriteLine($"Poruka primljena od klijenta ({clientEndPoint}): {primljenaPoruka}");

                if (primljenaPoruka == "LISTA")
                {
                    string jsonLista = JsonSerializer.Serialize(new List<Uredjaji>(uredjaji.Values));
                    byte[] odgovorBytes = Encoding.UTF8.GetBytes(jsonLista);
                    udpServer.Send(odgovorBytes, odgovorBytes.Length, clientEndPoint);
                    Console.WriteLine("Lista uređaja poslata klijentu.");
                }
                else
                {
                    string[] delovi = primljenaPoruka.Split(':');
                    if (delovi.Length == 3 && uredjaji.TryGetValue(delovi[0], out var uredjaj))
                    {
                        string funkcija = delovi[1];
                        string novaVrednost = delovi[2];

                        uredjaj.AzurirajFunkciju(funkcija, novaVrednost);
                        Console.WriteLine($"Uređaj '{delovi[0]}' ažuriran: {uredjaj.DobijStanje()}");

                        string odgovor = $"Uspješno ažurirano: {uredjaj.DobijStanje()}";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpServer.Send(odgovorBytes, odgovorBytes.Length, clientEndPoint);
                    }
                    else
                    {
                        string greska = "Greška: Neispravna poruka ili uređaj ne postoji.";
                        byte[] greskaBytes = Encoding.UTF8.GetBytes(greska);
                        udpServer.Send(greskaBytes, greskaBytes.Length, clientEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }*/

    public void Pokreni()
    {
        UdpClient udpServer = new UdpClient(6000);
        Console.WriteLine("UDP Server je pokrenut i osluškuje na portu 6000...");

        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        BinaryFormatter formatter = new BinaryFormatter();

        while (true)
        {
            try
            {
                // Primanje poruke od klijenta
                byte[] receivedBytes = udpServer.Receive(ref clientEndPoint);
                string primljenaPoruka = Encoding.UTF8.GetString(receivedBytes);
               //Console.WriteLine($"Primljena poruka od klijenta: {primljenaPoruka}");

                if (primljenaPoruka == "LISTA")
                {
                    // Slanje liste uređaja
                    using (MemoryStream ms = new MemoryStream())
                    {
                        formatter.Serialize(ms, new List<Uredjaji>(uredjaji.Values));
                        byte[] odgovorBytes = ms.ToArray();
                        udpServer.Send(odgovorBytes, odgovorBytes.Length, clientEndPoint);
                    }
                    Console.WriteLine("Lista uređaja poslata klijentu.");
                }
                else
                {
                    // Obrada komandi za ažuriranje uređaja
                    using (MemoryStream ms = new MemoryStream(receivedBytes))
                    {
                        string imeUredjaja = (string)formatter.Deserialize(ms);
                        string funkcija = (string)formatter.Deserialize(ms);
                        string novaVrednost = (string)formatter.Deserialize(ms);

                        if (uredjaji.TryGetValue(imeUredjaja, out var uredjaj))
                        {
                            uredjaj.AzurirajFunkciju(funkcija, novaVrednost);
                            Console.WriteLine($"Korisnik je izabrao ređaj:'{imeUredjaja}' ažuriran: {uredjaj.DobijStanje()}");

                            string odgovor = $"Uspješno ažurirano: {uredjaj.DobijStanje()}";
                            byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                            udpServer.Send(odgovorBytes, odgovorBytes.Length, clientEndPoint);
                        }
                        else
                        {
                            string greska = "Greška: Uređaj nije pronađen.";
                            byte[] greskaBytes = Encoding.UTF8.GetBytes(greska);
                            udpServer.Send(greskaBytes, greskaBytes.Length, clientEndPoint);
                        }
                    }
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
}
