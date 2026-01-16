using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayFair
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Cifru Playfair");
                Console.WriteLine("Nota: Literele 'I' si 'J' sunt tratate ca aceeasi litera ('I').");
                Console.WriteLine("1. Criptare");
                Console.WriteLine("2. Decriptare");
                Console.WriteLine("0. Iesire");
                Console.Write("\nAlege optiunea: ");

                string opt = Console.ReadLine();

                if (opt == "0") break;

                Console.Write("\nIntrodu cheia (cuvant secret): ");
                string key = Console.ReadLine();

                if (opt == "1")
                {
                    Console.Write("Introdu textul clar: ");
                    string input = Console.ReadLine();
                    string encrypted = PlayfairEncrypt(input, key);
                    Console.WriteLine($"\nText Criptat: {encrypted}");
                }
                else if (opt == "2")
                {
                    Console.Write("Introdu textul criptat: ");
                    string input = Console.ReadLine();
                    string decrypted = PlayfairDecrypt(input, key);
                    Console.WriteLine($"\nText Decriptat: {decrypted}");
                }

                Console.WriteLine("\nApasa orice tasta pentru a continua...");
                Console.ReadKey();
            }
        }

        // --- GENERAREA MATRICEI ---
        static char[,] GenereazaMatrice(string key)
        {
            char[,] matrice = new char[5, 5];
            string alfabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ"; // Lipseste J
            string keyNormalizata = "";
            HashSet<char> folosite = new HashSet<char>();

            // 1. Procesam cheia: Upper, J->I, pastram doar litere
            key = key.ToUpper().Replace("J", "I");
            foreach (char c in key)
            {
                if (char.IsLetter(c) && !folosite.Contains(c))
                {
                    keyNormalizata += c;
                    folosite.Add(c);
                }
            }

            // 2. Completam cu restul alfabetului
            string cheieCompleta = keyNormalizata;
            foreach (char c in alfabet)
            {
                if (!folosite.Contains(c))
                {
                    cheieCompleta += c;
                    folosite.Add(c);
                }
            }

            // 3. Umplem matricea 5x5
            int index = 0;
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    matrice[r, c] = cheieCompleta[index];
                    index++;
                }
            }

            // (Optional) Afisam matricea pentru debug
            AfiseazaMatrice(matrice);
            return matrice;
        }

        // --- PREGATIREA TEXTULUI (Criptare) ---
        static string PregatesteText(string input)
        {
            // Upper, J->I, doar litere
            StringBuilder sb = new StringBuilder();
            foreach (char c in input.ToUpper())
            {
                if (char.IsLetter(c))
                    sb.Append(c == 'J' ? 'I' : c);
            }
            string raw = sb.ToString();

            // Inserare 'X' intre dubluri si padding la final
            StringBuilder textFinal = new StringBuilder();
            for (int i = 0; i < raw.Length; i++)
            {
                char c1 = raw[i];
                // Daca e ultimul caracter sau urmatorul e diferit, il adaugam normal
                // Daca urmatorul e la fel, adaugam X intre ele
                if (i + 1 < raw.Length && raw[i + 1] == c1)
                {
                    textFinal.Append(c1);
                    textFinal.Append('X');
                }
                else
                {
                    textFinal.Append(c1);
                }
            }

            // Daca lungimea e impara, adaugam X la final
            if (textFinal.Length % 2 != 0)
                textFinal.Append('X');

            return textFinal.ToString();
        }

        // --- LOGICA DE CRIPTARE ---
        static string PlayfairEncrypt(string text, string key)
        {
            var matrice = GenereazaMatrice(key);
            var textPregatit = PregatesteText(text);
            StringBuilder rezultat = new StringBuilder();

            for (int i = 0; i < textPregatit.Length; i += 2)
            {
                char c1 = textPregatit[i];
                char c2 = textPregatit[i + 1];

                var pos1 = GasestePozitie(matrice, c1);
                var pos2 = GasestePozitie(matrice, c2);

                int r1 = pos1.Item1, c1Idx = pos1.Item2;
                int r2 = pos2.Item1, c2Idx = pos2.Item2;

                if (r1 == r2) // Acelasi rand -> Shift Dreapta
                {
                    rezultat.Append(matrice[r1, (c1Idx + 1) % 5]);
                    rezultat.Append(matrice[r2, (c2Idx + 1) % 5]);
                }
                else if (c1Idx == c2Idx) // Aceeasi coloana -> Shift Jos
                {
                    rezultat.Append(matrice[(r1 + 1) % 5, c1Idx]);
                    rezultat.Append(matrice[(r2 + 1) % 5, c2Idx]);
                }
                else // Dreptunghi -> Swap coloane
                {
                    rezultat.Append(matrice[r1, c2Idx]);
                    rezultat.Append(matrice[r2, c1Idx]);
                }
            }

            // Formatam iesirea in grupuri de cate 2 (optional)
            return GrupeazaText(rezultat.ToString());
        }

        // --- LOGICA DE DECRIPTARE ---
        static string PlayfairDecrypt(string text, string key)
        {
            var matrice = GenereazaMatrice(key);
            // La decriptare eliminam spatiile si ignoram non-literele
            StringBuilder cleanText = new StringBuilder();
            foreach (char c in text.ToUpper())
                if (char.IsLetter(c)) cleanText.Append(c);

            string input = cleanText.ToString();
            StringBuilder rezultat = new StringBuilder();

            for (int i = 0; i < input.Length; i += 2)
            {
                char c1 = input[i];
                char c2 = input[i + 1];

                var pos1 = GasestePozitie(matrice, c1);
                var pos2 = GasestePozitie(matrice, c2);

                int r1 = pos1.Item1, c1Idx = pos1.Item2;
                int r2 = pos2.Item1, c2Idx = pos2.Item2;

                if (r1 == r2) // Acelasi rand -> Shift Stanga
                {
                    // +5 asigura ca rezultatul modulo e pozitiv in C#
                    rezultat.Append(matrice[r1, (c1Idx - 1 + 5) % 5]);
                    rezultat.Append(matrice[r2, (c2Idx - 1 + 5) % 5]);
                }
                else if (c1Idx == c2Idx) // Aceeasi coloana -> Shift Sus
                {
                    rezultat.Append(matrice[(r1 - 1 + 5) % 5, c1Idx]);
                    rezultat.Append(matrice[(r2 - 1 + 5) % 5, c2Idx]);
                }
                else // Dreptunghi -> Swap coloane (la fel ca la criptare)
                {
                    rezultat.Append(matrice[r1, c2Idx]);
                    rezultat.Append(matrice[r2, c1Idx]);
                }
            }

            return rezultat.ToString();
        }

        // --- HELPER FUNCTIONS ---
        static Tuple<int, int> GasestePozitie(char[,] matrice, char c)
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    if (matrice[i, j] == c)
                        return Tuple.Create(i, j);
            return null;
        }

        static void AfiseazaMatrice(char[,] matrice)
        {
            Console.WriteLine("\n--- Matricea Cheii ---");
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                    Console.Write(matrice[i, j] + " ");
                Console.WriteLine();
            }
            Console.WriteLine("----------------------");
        }

        static string GrupeazaText(string text)
        {
            // Adauga un spatiu la fiecare 2 litere pentru lizibilitate
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                sb.Append(text[i]);
                if (i % 2 != 0 && i != text.Length - 1) sb.Append(" ");
            }
            return sb.ToString();
        }
    }
}
