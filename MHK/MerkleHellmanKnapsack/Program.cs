using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MerkleHellmanKnapsack
{
    class Program
    {
        static void Main(string[] args)
        {

            // 1. Initializare si Generare Chei
            // Alegem o dimensiune a blocului de 8 biți (pentru a cripta byte cu byte - 1 char ASCII)
            int n = 8;
            MerkleHellman mh = new MerkleHellman(n);

            Console.WriteLine("--- Generare Chei ---");
            mh.GenerateKeys();

            // Afisare chei
            Console.WriteLine("Cheia Privata (Supercrescatoare - W): " + string.Join(", ", mh.PrivateKey_W));
            Console.WriteLine($"Cheia Privata (q): {mh.PrivateKey_q}");
            Console.WriteLine($"Cheia Privata (r): {mh.PrivateKey_r}");
            Console.WriteLine("Cheia Publica (B): " + string.Join(", ", mh.PublicKey));
            Console.WriteLine();

            // 2. Criptare
            string textClar = "ALGO";
            Console.WriteLine($"--- Criptare Mesaj: '{textClar}' ---");

            List<BigInteger> textCriptat = mh.Encrypt(textClar);
            Console.WriteLine("Text Criptat (vector de sume): " + string.Join(", ", textCriptat));

            // 3. Decriptare
            Console.WriteLine("\n--- Decriptare ---");
            string textDecriptat = mh.Decrypt(textCriptat);
            Console.WriteLine($"Text Decriptat: '{textDecriptat}'");

            Console.ReadKey();
        }
    }

    public class MerkleHellman
    {
        private int _n; // Numarul de elemente din secventa (dimensiunea blocului in biti)
        private Random _rnd = new Random();

        // Cheile
        public List<BigInteger> PrivateKey_W { get; private set; } // Secventa Supercrescatoare
        public BigInteger PrivateKey_q { get; private set; }       // Modulul (q > suma W)
        public BigInteger PrivateKey_r { get; private set; }       // Multiplicatorul (coprim cu q)
        public List<BigInteger> PublicKey { get; private set; }    // Secventa Publica (Hard Knapsack)

        public MerkleHellman(int n = 8)
        {
            _n = n;
        }

        public void GenerateKeys()
        {
            // Pasul 1: Generarea secvenței supercrescătoare (W)
            // w_i > suma(w_0 ... w_{i-1})
            PrivateKey_W = new List<BigInteger>();
            BigInteger sum = 0;

            for (int i = 0; i < _n; i++)
            {
                // Generam un numar mai mare decat suma curenta
                // Adaugam un offset random intre 1 si 10 pentru variatie
                BigInteger nextVal = sum + _rnd.Next(1, 10);
                if (i == 0) nextVal = _rnd.Next(1, 5); // Primul element

                PrivateKey_W.Add(nextVal);
                sum += nextVal;
            }

            // Pasul 2: Alegerea lui q (Modulul)
            // q trebuie sa fie mai mare decat suma tuturor elementelor din W
            PrivateKey_q = sum + _rnd.Next(10, 50);

            // Pasul 3: Alegerea lui r (Multiplicatorul)
            // r trebuie sa fie in [1, q-1] si gcd(r, q) = 1
            do
            {
                PrivateKey_r = _rnd.Next(2, (int)PrivateKey_q - 1); // Simplificare pt BigInt mic, in realitate se genereaza random mare
            } while (BigInteger.GreatestCommonDivisor(PrivateKey_r, PrivateKey_q) != 1);

            // Pasul 4: Generarea Cheii Publice (B)
            // beta_i = (r * w_i) mod q
            PublicKey = new List<BigInteger>();
            foreach (var w in PrivateKey_W)
            {
                BigInteger beta = (PrivateKey_r * w) % PrivateKey_q;
                PublicKey.Add(beta);
            }
        }

        public List<BigInteger> Encrypt(string message)
        {
            List<BigInteger> cipherText = new List<BigInteger>();

            // Convertim mesajul in bytes
            byte[] bytes = Encoding.ASCII.GetBytes(message);

            foreach (byte b in bytes)
            {
                BigInteger currentBlockSum = 0;

                // Convertim byte-ul in biti si facem produsul scalar cu Cheia Publica
                // Problema rucsacului: Suma elementelor din cheie corespunzatoare bitilor de 1
                for (int i = 0; i < 8; i++)
                {
                    // Verificam bitul de la pozitia i (de la cel mai semnificativ la cel mai putin, sau invers)
                    // Aici luam conventia: bitul 0 corespunde indexului 0 din cheie (LSB in acest caz)
                    bool isBitSet = (b & (1 << i)) != 0;

                    if (isBitSet)
                    {
                        if (i < PublicKey.Count)
                            currentBlockSum += PublicKey[i];
                    }
                }
                cipherText.Add(currentBlockSum);
            }

            return cipherText;
        }

        public string Decrypt(List<BigInteger> cipherText)
        {
            StringBuilder sb = new StringBuilder();

            // Pasul 1: Calculam inversul modular al lui r
            // Avem nevoie de r^-1 astfel incat (r * r^-1) = 1 mod q
            BigInteger r_inverse = ModInverse(PrivateKey_r, PrivateKey_q);

            foreach (BigInteger c in cipherText)
            {
                // Pasul 2: Transformam elementul criptat (C) inapoi in problema usoara (C')
                // C' = (C * r^-1) mod q
                BigInteger c_prime = (c * r_inverse) % PrivateKey_q;
                if (c_prime < 0) c_prime += PrivateKey_q; // Corectie pt modulo negativ

                // Pasul 3: Rezolvam problema rucsacului supercrescator (Algoritm Greedy)
                byte decryptedByte = 0;

                // Iteram invers prin cheia privata W (de la cel mai mare la cel mai mic)
                for (int i = _n - 1; i >= 0; i--)
                {
                    if (PrivateKey_W[i] <= c_prime)
                    {
                        // Daca elementul incape in suma ramasa, il scadem si marcam bitul ca 1
                        c_prime -= PrivateKey_W[i];
                        decryptedByte |= (byte)(1 << i); // Setam bitul i
                    }
                }

                sb.Append((char)decryptedByte);
            }

            return sb.ToString();
        }

        // Helper: Algoritmul Euclid Extins pentru Invers Modular
        private BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0, x = 1;

            if (m == 1) return 0;

            while (a > 1)
            {
                // q is quotient
                BigInteger q = a / m;
                BigInteger t = m;

                // m is remainder now, process same as Euclid's algo
                m = a % m;
                a = t;
                t = y;

                // Update y and x
                y = x - q * y;
                x = t;
            }

            // Make x positive
            if (x < 0) x += m0;

            return x;
        }
    }
}