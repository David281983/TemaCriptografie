using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cifru_N
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Criptare (+n)");
                Console.WriteLine("2. Decriptare (-n)");
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

                Console.ReadKey();
            }
        }

        // --- 1. CRIPTARE ---
        static void MeniuCriptare()
        {
            Console.Write("Introdu textul clar: ");
            string input = Console.ReadLine();

            Console.Write("Introdu cheia n (numar natural): ");
            if (int.TryParse(Console.ReadLine(), out int n))
            {
                // Criptarea este shift pozitiv (+n)
                string rezultat = ProceseazaText(input, n);
                Console.WriteLine($"Text criptat: {rezultat}");
            }
            else
            {
                Console.WriteLine("Cheia trebuie sa fie un numar intreg!");
            }
        }

        // --- 2. DECRIPTARE ---
        static void MeniuDecriptare()
        {
            Console.WriteLine("\n--- Decriptare ---");
            Console.Write("Introdu textul criptat: ");
            string input = Console.ReadLine();

            Console.Write("Introdu cheia n (cunoscuta): ");
            if (int.TryParse(Console.ReadLine(), out int n))
            {
                // Decriptarea este shift negativ (-n)
                string rezultat = ProceseazaText(input, -n);
                Console.WriteLine($"Text decriptat: {rezultat}");
            }
            else
            {
                Console.WriteLine("Cheia trebuie sa fie un numar intreg!");
            }
        }

        // --- 3. CRIPTANALIZA (Atac fara cheie) ---
        static void MeniuCriptanaliza()
        {
            Console.WriteLine("Vom incerca toate cele 26 de chei posibile.");
            Console.Write("Introdu textul criptat (ex: MJQQT): ");
            string input = Console.ReadLine();

            Console.WriteLine("\nRezultate posibile:");
            Console.WriteLine("Cheie | Text rezultat");

            // Spatiul cheilor este 0..25. Iteram prin toate.
            // Pentru a "sparge" codul, aplicam operatia de DECRIPTARE (-i)
            for (int i = 0; i < 26; i++)
            {
                string incercare = ProceseazaText(input, -i);
                Console.WriteLine($"n={i,2} | {incercare}");
            }
            Console.WriteLine("---------------------");
            Console.WriteLine("Analizeaza lista si gaseste textul care are sens.");
        }

        /// <summary>
        /// Functia nucleu care realizeaza translatia caracterelor.
        /// </summary>
        /// <param name="text">Textul de procesat</param>
        /// <param name="shift">Deplasarea n (pozitiva pt criptare, negativa pt decriptare)</param>
        /// <returns>Textul procesat</returns>
        static string ProceseazaText(string text, int shift)
        {
            StringBuilder sb = new StringBuilder();

            // Normalizam shift-ul pentru a fi in intervalul [0, 25]
            // Daca shift e 30, e la fel ca shift 4.
            // Daca shift e negativ (-5), trebuie gestionat corect.
            int finalShift = shift % 26;

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'A' : 'a';

                    // 1. Transformam litera in index 0-25
                    int originalPos = c - offset;

                    // 2. Aplicam shift-ul
                    int newPos = originalPos + finalShift;

                    // 3. Gestionam iesirea din alfabet (wrap-around)
                    // Daca newPos trece de 25 (ex: Z+1), modulo il aduce la 0
                    // Daca newPos e negativ (ex: A-1), trebuie sa devina 25 (Z)
                    newPos = newPos % 26;

                    if (newPos < 0)
                    {
                        newPos += 26;
                    }

                    // 4. Transformam inapoi in caracter
                    sb.Append((char)(newPos + offset));
                }
                else
                {
                    // Caracterele non-litera raman neschimbate
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
