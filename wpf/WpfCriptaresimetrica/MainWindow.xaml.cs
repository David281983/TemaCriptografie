using Microsoft.Win32; // Pentru OpenFileDialog
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CryptoApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeCombos();
        }

        private void InitializeCombos()
        {
            // Populare Algoritmi
            cmbAlgo.ItemsSource = new string[] { "AES", "DES", "TripleDES", "Rijndael", "RC2" };
            cmbAlgo.SelectedIndex = 0; // Default AES

            // Populare CipherMode (ex: CBC, ECB, CFB)
            cmbMode.ItemsSource = Enum.GetValues(typeof(CipherMode));
            cmbMode.SelectedItem = CipherMode.CBC;

            // Populare PaddingMode (ex: PKCS7, Zeros)
            cmbPadding.ItemsSource = Enum.GetValues(typeof(PaddingMode));
            cmbPadding.SelectedItem = PaddingMode.PKCS7;
        }

        // Handler pentru butonul Browse
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                txtFile.Text = dlg.FileName;
                lblStatus.Text = "Fisier selectat.";
            }
        }

        // Reseteaza cheile cand schimbam algoritmul (deoarece au lungimi diferite)
        private void OnSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            txtKey.Text = "";
            txtIV.Text = "";
            lblStatus.Text = "Setarile s-au schimbat. Genereaza noi chei.";
        }

        // Generare IV (Vector de Initializare)
        private void btnGenIV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SymmetricAlgorithm algo = GetAlgorithm(cmbAlgo.SelectedItem.ToString()))
                {
                    algo.GenerateIV();
                    txtIV.Text = Convert.ToBase64String(algo.IV);
                }
            }
            catch (Exception ex) { MessageBox.Show("Eroare: " + ex.Message); }
        }

        // Generare Cheie
        private void btnGenKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SymmetricAlgorithm algo = GetAlgorithm(cmbAlgo.SelectedItem.ToString()))
                {
                    algo.GenerateKey();
                    txtKey.Text = Convert.ToBase64String(algo.Key);
                }
            }
            catch (Exception ex) { MessageBox.Show("Eroare: " + ex.Message); }
        }

        // Functia principala de procesare (Criptare sau Decriptare)
        private async void btnAction_Click(object sender, RoutedEventArgs e)
        {
            bool isEncrypt = ((Button)sender).Name == "btnEncrypt";
            string inputFile = txtFile.Text;

            if (!File.Exists(inputFile))
            {
                MessageBox.Show("Selectati un fisier valid!");
                return;
            }

            if (string.IsNullOrEmpty(txtKey.Text))
            {
                MessageBox.Show("Generati sau introduceti o cheie!");
                return;
            }

            // Numele fisierului de iesire
            string outputFile = inputFile + (isEncrypt ? ".enc" : ".dec");

            btnEncrypt.IsEnabled = false;
            btnDecrypt.IsEnabled = false;
            lblStatus.Text = "Se proceseaza... Va rugam asteptati.";

            try
            {
                // Rulam operatia grea pe un alt thread, folosind Task.Run pentru a nu bloca UI-ul
                await Task.Run(() => ProcessFile(inputFile, outputFile, isEncrypt));

                lblStatus.Text = "Operatie finalizata cu succes!";
                MessageBox.Show($"Fisier creat:\n{outputFile}", "Succes");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Eroare!";
                MessageBox.Show($"Eroare la procesare: {ex.Message}");
            }
            finally
            {
                btnEncrypt.IsEnabled = true;
                btnDecrypt.IsEnabled = true;
            }
        }

        // Logica efectiva de criptare/decriptare
        private void ProcessFile(string inputPath, string outputPath, bool encrypt)
        {
            // Pentru a accesa controalele UI de pe alt thread, trebuie sa folosim Dispatcher
            // Sau, mai simplu, citim valorile inainte de a intra in Task.Run. 
            // Aici le vom prelua din UI prin Dispatcher.Invoke pentru siguranta in interiorul task-ului.

            string algoName = "";
            CipherMode mode = CipherMode.CBC;
            PaddingMode padding = PaddingMode.PKCS7;
            byte[] key = null;
            byte[] iv = null;

            Dispatcher.Invoke(() =>
            {
                algoName = cmbAlgo.SelectedItem.ToString();
                mode = (CipherMode)cmbMode.SelectedItem;
                padding = (PaddingMode)cmbPadding.SelectedItem;
                key = Convert.FromBase64String(txtKey.Text);

                // IV nu este necesar in modul ECB
                if (mode != CipherMode.ECB && !string.IsNullOrEmpty(txtIV.Text))
                {
                    iv = Convert.FromBase64String(txtIV.Text);
                }
            });

            using (SymmetricAlgorithm algorithm = GetAlgorithm(algoName))
            {
                algorithm.Mode = mode;
                algorithm.Padding = padding;
                algorithm.Key = key;
                if (iv != null) algorithm.IV = iv;

                // Cream transformatorul (ICryptoTransform)
                ICryptoTransform transform = encrypt ? algorithm.CreateEncryptor() : algorithm.CreateDecryptor();

                // Folosim FileStream pentru a citi si scrie bucata cu bucata (pentru fisiere mari)
                using (FileStream fsIn = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
                using (FileStream fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                using (CryptoStream cs = new CryptoStream(fsOut, transform, CryptoStreamMode.Write))
                {
                    // Copiem datele din input in CryptoStream (care scrie in output)
                    // CopyTo gestioneaza buffer-ul automat
                    fsIn.CopyTo(cs);
                }
            }
        }

        // Factory pattern simplu pentru instantierea algoritmului
        private SymmetricAlgorithm GetAlgorithm(string name)
        {
            switch (name)
            {
                case "AES": return Aes.Create();
                case "DES": return DES.Create();
                case "TripleDES": return TripleDES.Create();
                case "Rijndael": return Rijndael.Create();
                case "RC2": return RC2.Create();
                default: throw new ArgumentException("Algoritm necunoscut");
            }
        }
    }
}