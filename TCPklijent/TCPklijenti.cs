using System;
using System.Net.Sockets;
using System.Text;

namespace TCPklijent
{
    public class TCPklijent
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    Console.WriteLine("Povezivanje na server...");
                    clientSocket.Connect("127.0.0.1", 50000); // Server na localhost:50000
                    Console.WriteLine("Povezano!");

                    Console.WriteLine("Unesite korisničko ime: ");
                    string korisnickoIme = Console.ReadLine();
                    Console.WriteLine("Unesite lozinku: ");
                    string lozinka = Console.ReadLine();

                    string loginPodaci = $"{korisnickoIme}:{lozinka}";
                    clientSocket.Send(Encoding.UTF8.GetBytes(loginPodaci));

                    byte[] buffer = new byte[1024];
                    int received = clientSocket.Receive(buffer);
                    string odgovor = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Odgovor servera: {odgovor}");

                    if (odgovor == "USPESNO")
                    {
                        // Dalji koraci, npr. prikaz uređaja i izbor funkcije
                        Console.WriteLine("Prijava uspešna. Dobijate listu uređaja...");
                        // Logika za izbor uređaja i slanje komandi može ići ovde.
                    }
                    else
                    {
                        Console.WriteLine("Prijava neuspešna.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}
