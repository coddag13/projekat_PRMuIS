using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Uredjaj;




namespace UDPklijent
{
    public class UdpKlijent
    {
        static void Main(string[] args)
        {
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);
            EndPoint posiljaocEP = new IPEndPoint(IPAddress.Any,0);
            Uredjajii ur= new Uredjajii();
            byte[] prijemniBaf = new byte[1024];
            while (true)
            {
                try
                {

                    byte[] binMessage = Encoding.UTF8.GetBytes("UDP Klijent se povezao");
                    int brBajta = tcpSocket.SendTo(binMessage, 0, binMessage.Length, SocketFlags.None, serverEP);
                    Console.WriteLine($"Uspesno poslato {brBajta} ka {serverEP}");


                    brBajta = tcpSocket.ReceiveFrom(prijemniBaf, ref posiljaocEP);

                    string poruka = Encoding.UTF8.GetString(prijemniBaf, 0, brBajta);

                    Console.WriteLine($"Stigao je odgovor od {posiljaocEP}, duzine {brBajta}, Funkcija je :{poruka}");

                    string[] delovi = poruka.Split(':');
                    string funkcija = delovi[0];
                    string imeUredjaj = delovi[1];
                    Console.WriteLine(funkcija + " " + imeUredjaj);
                    foreach (var u in ur.SviUredjaji())
                    {

                        if (u.Ime.ToString() == imeUredjaj)
                        {

                            if (u.Ime.ToString() == "Svetlo")
                            {
                                foreach (var f in u.Funkcije)
                                {
                                    Console.WriteLine(f.Key.ToString());
                                    if (f.Key.ToString() == funkcija)
                                    {
                                        if (f.Key.ToString() == "intezitet")
                                        {
                                            Console.WriteLine($"Unesi novu vrijednost za {funkcija}(0%-100%):");
                                            string novaVrijednost = Console.ReadLine();
                                            u.Azuriranje(funkcija, novaVrijednost);
                                            Console.WriteLine($"Funkcija {funkcija} je azurirana na vrijednost->{novaVrijednost} ");
                                            binMessage = Encoding.UTF8.GetBytes(novaVrijednost + "%");
                                            brBajta = tcpSocket.SendTo(binMessage, 0, binMessage.Length, SocketFlags.None, serverEP);
                                            Console.WriteLine($"Uspesno poslato {brBajta} ka {serverEP}");
                                            break;
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Unesi novu vrijednost za {funkcija}(0-255):");
                                            string novaVrijednost = Console.ReadLine();
                                            u.Azuriranje(funkcija, novaVrijednost);
                                            Console.WriteLine($"Funkcija {funkcija} je azurirana na vrijednost->{novaVrijednost} ");
                                            binMessage = Encoding.UTF8.GetBytes(novaVrijednost);
                                            brBajta = tcpSocket.SendTo(binMessage, 0, binMessage.Length, SocketFlags.None, serverEP);
                                            Console.WriteLine($"Uspesno poslato {brBajta} ka {serverEP}");
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (u.Ime == "TV")
                            {

                            }
                        }
                        else
                        {
                            Console.WriteLine("Nije pronadjen uredjaj");
                        }
                    }
                    Console.WriteLine("AZURIRANJE VRIJEDNOSTI\n");

                    foreach (var a in ur.SviUredjaji())
                    {
                        if (a.Ime == "Svetlo")
                        {
                            foreach (var f in a.Funkcije)
                            {
                                Console.WriteLine(f.Key + " " + f.Value);
                            }
                        }
                    }


                    break;
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske tokom slanja poruke: \n{ex}");
                }
            }
            Console.WriteLine("Klijen zavrsava sa radom");
            tcpSocket.Close(); // Zatvaramo soket na kraju rada
            Console.ReadKey();
        }
    } 
}

