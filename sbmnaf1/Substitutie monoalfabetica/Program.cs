using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substitutie_monoalfabetica
{
    internal class Program
    {
        // Alfabetul standard
        const string ALFABET_STANDARD = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Frecventa standard a literelor in Limba Engleza (de la cea mai frecventa la cea mai rara)
        // E, T, A, O, I, N, ...
        const string FRECVENTA_ENGLEZA = "ETAOINSHRDLCUMWFGYPBVKJXQZ";

        // (Optional) Pentru limba romana, ordinea ar fi aprox: "AIE... "

        // Cheia curenta (o vom genera aleatoriu sau o setam manual)
        static string cheieCurenta = "";

        static void Main(string[] args)
        {
            // Generam o cheie la start pentru a avea cu ce lucra
            GenereazaCheieAleatorie();

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Cheia Curenta: {cheieCurenta}");
                Console.WriteLine($"Alfabet Ref. : {ALFABET_STANDARD}");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("1. Genereaza o noua cheie aleatorie");
                Console.WriteLine("2. Criptare text");
                Console.WriteLine("3. Decriptare text");
                Console.WriteLine("4. Criptanaliza (Analiza de Frecventa)");
                Console.WriteLine("0. Iesire");

                string opt = Console.ReadLine();

                switch (opt)
                {
                    case "1":
                        GenereazaCheieAleatorie();
                        Console.WriteLine("Cheie noua generata!");
                        Console.ReadKey();
                        break;
                    case "2":
                        MeniuCriptare();
                        break;
                    case "3":
                        MeniuDecriptare();
                        break;
                    case "4":
                        MeniuCriptanaliza();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Optiune invalida.");
                        break;
                }
            }
        }

        static void GenereazaCheieAleatorie()
        {
            // Folosim LINQ pentru a ordona aleatoriu caracterele din alfabet
            Random rnd = new Random();
            cheieCurenta = new string(ALFABET_STANDARD.OrderBy(x => rnd.Next()).ToArray());
        }

        static void MeniuCriptare()
        {
            Console.WriteLine("\n--- Criptare ---");
            Console.Write("Introdu textul clar: ");
            string input = Console.ReadLine().ToUpper(); // Lucram doar cu majuscule pt simplitate

            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                int index = ALFABET_STANDARD.IndexOf(c);
                if (index != -1)
                {
                    // Inlocuim litera din alfabetul standard cu cea din cheie
                    sb.Append(cheieCurenta[index]);
                }
                else
                {
                    sb.Append(c); // Caracterele speciale raman la fel
                }
            }

            Console.WriteLine($"Text Criptat: {sb.ToString()}");
            Console.ReadKey();
        }

        static void MeniuDecriptare()
        {
            Console.WriteLine("\n--- Decriptare ---");
            Console.Write("Introdu textul criptat: ");
            string input = Console.ReadLine().ToUpper();

            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                int index = cheieCurenta.IndexOf(c);
                if (index != -1)
                {
                    // Inlocuim litera din cheie cu cea din alfabetul standard (operatia inversa)
                    sb.Append(ALFABET_STANDARD[index]);
                }
                else
                {
                    sb.Append(c);
                }
            }

            Console.WriteLine($"Text Decriptat: {sb.ToString()}");
            Console.ReadKey();
        }

        static void MeniuCriptanaliza()
        {
            Console.WriteLine("\n--- Criptanaliza (Frecventa) ---");
            Console.WriteLine("Aceasta metoda incearca sa ghiceasca textul bazandu-se pe statistica limbii engleze.");
            Console.WriteLine("NOTA: Functioneaza bine doar pe texte LUNGI.");
            Console.Write("Introdu textul criptat: ");
            string input = Console.ReadLine().ToUpper();

            // 1. Calculam frecventa literelor in textul criptat
            Dictionary<char, int> frecventeText = new Dictionary<char, int>();
            foreach (char c in ALFABET_STANDARD) frecventeText[c] = 0; // init cu 0

            int totalLitere = 0;
            foreach (char c in input)
            {
                if (ALFABET_STANDARD.Contains(c))
                {
                    frecventeText[c]++;
                    totalLitere++;
                }
            }

            // 2. Sortam literele din textul criptat descrescator dupa aparitii
            // Ex: Daca 'X' apare de 10 ori si 'Q' de 2 ori -> Lista: X, Q, ...
            var litereSortateDupaFrecventa = frecventeText
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .ToList();

            // 3. Afisam statistica
            Console.WriteLine("\nStatistica textului criptat:");
            for (int i = 0; i < 5; i++) // Afisam top 5
            {
                char lit = litereSortateDupaFrecventa[i];
                Console.WriteLine($"Litera '{lit}' apare de {frecventeText[lit]} ori.");
            }

            // 4. Incercam decriptarea automata
            // Mapam:
            // Cea mai frecventa litera din CIPHER -> 'E' (cea mai frecventa din ENG)
            // A doua cea mai frecventa din CIPHER -> 'T'
            // etc.

            // Construim un dictionar de mapare presupusa
            Dictionary<char, char> mapareGhicita = new Dictionary<char, char>();
            for (int i = 0; i < 26; i++)
            {
                char literaCriptata = litereSortateDupaFrecventa[i];
                char literaPresupusaDecriptata = FRECVENTA_ENGLEZA[i];

                mapareGhicita[literaCriptata] = literaPresupusaDecriptata;
            }

            // 5. Reconstruim textul pe baza maparii statistice
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (mapareGhicita.ContainsKey(c))
                {
                    sb.Append(mapareGhicita[c]);
                }
                else
                {
                    sb.Append(c);
                }
            }

            Console.WriteLine("\n--- Rezultat Criptanaliza (GHICIT) ---");
            Console.WriteLine(sb.ToString());
            Console.WriteLine("\nObs: Daca textul este scurt, rezultatul va fi gresit deoarece statistica are nevoie de volum mare de date.");
            Console.ReadKey();
        }
    }
}
