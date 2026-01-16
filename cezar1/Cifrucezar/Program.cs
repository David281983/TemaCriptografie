using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cifrucezar
{
    internal class Program
    {
        // Cheia standard pentru Cezar este 3
        const int CAESAR_KEY = 3;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Criptare text (+3)");
                Console.WriteLine("2. Decriptare text (-3)");
                Console.WriteLine("3. Criptanaliza");
                Console.WriteLine("0. Iesire");

                string optiune = Console.ReadLine();

                switch (optiune)
                {
                    case "1":
                        MeniuCriptare();
                        break;
                    case "2":
                        MeniuDecriptare();
                        break;
                    case "3":
                        MeniuCriptanaliza();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Optiune invalida!");
                        break;
                }

                Console.WriteLine("\nApasa orice tasta pentru a reveni la meniu...");
                Console.ReadKey();
            }
        }

        static void MeniuCriptare()
        {
            Console.Write("Introdu textul clar: ");
            string input = Console.ReadLine();

            string rezultat = ProceseazaText(input, CAESAR_KEY);
            Console.WriteLine($"Text criptat: {rezultat}");
        }

        static void MeniuDecriptare()
        {
            Console.Write("Introdu textul criptat: ");
            string input = Console.ReadLine();

            // Decriptarea este opusul criptarii, deci trimitem cheia cu minus (-3)
            string rezultat = ProceseazaText(input, -CAESAR_KEY);
            Console.WriteLine($"Text clar (decriptat): {rezultat}");
        }

        static void MeniuCriptanaliza()
        {
            Console.WriteLine("Se vor afisa toate cele 25 de posibile decalaje.");
            Console.Write("Introdu textul criptat necunoscut: ");
            string input = Console.ReadLine();

            Console.WriteLine("\nRezultate posibile:");

            // Incercam toate cheile posibile de la 1 la 25
            // Deoarece decriptam, scadem i (folosim -i)
            for (int i = 1; i < 26; i++)
            {
                string incercare = ProceseazaText(input, -i);
                Console.WriteLine($"Shift -{i,2}: {incercare}");
            }
        }

        /// <summary>
        /// Functia principala care muta literele cu un anumit numar de pozitii (cheia).
        /// </summary>
        static string ProceseazaText(string text, int key)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in text)
            {
                // Verificam daca caracterul este litera
                if (char.IsLetter(c))
                {
                    // Determinam punctul de start (A majuscula sau a minuscula)
                    char offset = char.IsUpper(c) ? 'A' : 'a';

                    // Formula matematica:
                    // 1. (c - offset): transformam litera in index 0-25 (ex: A=0, B=1)
                    // 2. + key: adaugam deplasarea (ex: +3 sau -3)
                    // 3. % 26: asiguram rotirea (Z devine C)
                    // 4. + 26: corectie pentru numere negative in C# (pentru decriptare)
                    // 5. % 26: aplicam din nou modulo pentru siguranta
                    int pozitieInitiala = c - offset;
                    int pozitieNoua = (pozitieInitiala + key) % 26;

                    // Corectie pentru modulo negativ (ex: decriptare A -> -3)
                    if (pozitieNoua < 0)
                    {
                        pozitieNoua += 26;
                    }

                    char caracterNou = (char)(pozitieNoua + offset);
                    sb.Append(caracterNou);
                }
                else
                {
                    // Caracterele care nu sunt litere (spatiu, punct, numere) raman neschimbate
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
