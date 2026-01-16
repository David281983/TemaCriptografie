using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substitutiepolialfabetica
{
    internal class Program
    {
        // Alfabetul standard (textul clar)
        const string ALFABET_CLAR = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Cele 3 alfabete de substitutie (Cheia)
        static readonly string[] ALFABETE_CIPHER = {
            "FRQSPYMHNJOELKDVAGXTIWBUZC", // Alfabet 1 (index 0)
            "SWZCINJTELAFQUMKPXDOVBRGHY", // Alfabet 2 (index 1)
            "CITYWLNZEVOMQGUPJFXBRAHSKD"  // Alfabet 3 (index 2)
        };

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Cifrul Vigenere Substitutie Polialfabetica n=3");
                Console.WriteLine("Schema de criptare:");
                Console.WriteLine($"1: {ALFABETE_CIPHER[0]}");
                Console.WriteLine($"2: {ALFABETE_CIPHER[1]}");
                Console.WriteLine($"3: {ALFABETE_CIPHER[2]}");
                Console.WriteLine("--------------------------------------------------------");
                Console.WriteLine("1. Criptare");
                Console.WriteLine("2. Decriptare");
                Console.WriteLine("0. Iesire");

                string opt = Console.ReadLine();

                switch (opt)
                {
                    case "1":
                        MeniuCriptare();
                        break;
                    case "2":
                        MeniuDecriptare();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Optiune invalida.");
                        break;
                }
                Console.WriteLine("\nApasa orice tasta pentru a continua...");
                Console.ReadKey();
            }
        }

        static void MeniuCriptare()
        {
            Console.WriteLine("\n--- Criptare ---");
            Console.Write("Introdu textul clar (ex: ABB BCC AB): ");
            string input = Console.ReadLine().ToUpper();

            StringBuilder sb = new StringBuilder();
            int contorLitere = 0; // Tine evidenta a cata litera este pentru a alege alfabetul

            foreach (char c in input)
            {
                int indexClar = ALFABET_CLAR.IndexOf(c);

                if (indexClar != -1) // Este litera
                {
                    // Determinam care din cele 3 alfabete il folosim
                    // Pozitia 0 -> Alfabet 0, Pozitia 1 -> Alfabet 1, Pozitia 2 -> Alfabet 2, Pozitia 3 -> Alfabet 0...
                    int indexAlfabet = contorLitere % 3;

                    // Luam litera corespunzatoare din alfabetul selectat
                    char literaCriptata = ALFABETE_CIPHER[indexAlfabet][indexClar];

                    sb.Append(literaCriptata);

                    // Incrementam contorul doar pentru litere (ignoram spatiile la rotatie)
                    contorLitere++;
                }
                else
                {
                    sb.Append(c); // Caracterele speciale raman la fel
                }
            }

            Console.WriteLine($"Text Criptat: {sb.ToString()}");
            // Verificare pentru exemplul dat: "FWI RZT FW"
        }

        static void MeniuDecriptare()
        {
            Console.WriteLine("\n--- Decriptare ---");
            Console.Write("Introdu textul criptat (ex: FWI RZT FW): ");
            string input = Console.ReadLine().ToUpper();

            StringBuilder sb = new StringBuilder();
            int contorLitere = 0;

            foreach (char c in input)
            {
                // Trebuie sa verificam in care alfabet sa cautam
                int indexAlfabet = contorLitere % 3;
                string alfabetCurent = ALFABETE_CIPHER[indexAlfabet];

                // Cautam litera criptata in alfabetul specific acelei pozitii
                int indexCriptat = alfabetCurent.IndexOf(c);

                if (indexCriptat != -1) // Este litera valida in alfabetul curent
                {
                    // Gasim litera originala la acelasi index in alfabetul clar
                    char literaClara = ALFABET_CLAR[indexCriptat];

                    sb.Append(literaClara);
                    contorLitere++;
                }
                else
                {
                    // Daca nu e in alfabet (ex: spatiu), il punem direct
                    sb.Append(c);
                }
            }

            Console.WriteLine($"Text Decriptat: {sb.ToString()}");
        }
    }
}
