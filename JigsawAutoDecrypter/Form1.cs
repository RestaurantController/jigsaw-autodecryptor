using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace JigsawAutoDecrypter
{
    public partial class Form1 : Form
    {
        internal const string EncryptionPassword = @"OoIsAwwF32cICQoLDA0ODe==";
        static String appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static String appDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        internal static string WorkFolderPath = Path.Combine(appDataRoamingPath, @"System32Work\");
        private static readonly string EncryptedFileListPath = Path.Combine(WorkFolderPath, @"EncryptedFileList.txt");
        public Form1()
        {
            InitializeComponent();
        }

        private static void DecryptFile(string path, string encryptionExtension)
        {
            try
            {
                if (!path.EndsWith(encryptionExtension))
                    return;
                var decryptedFilePath = path.Remove(path.Length - encryptionExtension.Length);
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Key = Convert.FromBase64String(EncryptionPassword);
                    aes.IV = new byte[] { 0, 1, 0, 3, 5, 3, 0, 1, 0, 0, 2, 0, 6, 7, 6, 0 };
                    DecryptFile(aes, path, decryptedFilePath);
                }
            }
            catch
            {
                return;
            }
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void DecryptFile(SymmetricAlgorithm alg, string inputFile, string outputFile)
        {
            var buffer = new byte[65536];

            using (var streamIn = new FileStream(inputFile, FileMode.Open))
            using (var streamOut = new FileStream(outputFile, FileMode.Create))
            using (var decrypt = new CryptoStream(streamOut, alg.CreateDecryptor(), CryptoStreamMode.Write))
            {
                int bytesRead;
                do
                {
                    bytesRead = streamIn.Read(buffer, 0, buffer.Length);
                    if (bytesRead != 0)
                        decrypt.Write(buffer, 0, bytesRead);
                }
                while (bytesRead != 0);
            }
        }

        internal static HashSet<string> GetEncryptedFiles()
        {
            var ecf = new HashSet<string>();
            if (File.Exists(EncryptedFileListPath))
            {
                foreach (var path in File.ReadAllLines(EncryptedFileListPath))
                {
                    ecf.Add(path);
                }
            }

            return ecf;
        }

        internal static void DecryptFiles(string encryptionExtension)
        {
            foreach (var file in GetEncryptedFiles())
            {
                try
                {
                    var ef = file + encryptionExtension;
                    DecryptFile(ef, encryptionExtension);
                    File.Delete(ef);

                }
                catch
                {
                    // ignored
                }
            }
            File.Delete(EncryptedFileListPath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "")
            {
                return;
            }
            button1.Enabled = false;
            label5.Text = "Status: Decrypting files....";
            textBox1.ReadOnly = true;
            DecryptFiles(textBox1.Text);
            MessageBox.Show(this, "Decryption Complete!" + Environment.NewLine + "Please check your files. They should've been decrypted." + Environment.NewLine + "If not, make sure all encrypted files are accessible and try again.", "Jigsaw Auto-Decrypter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            button1.Enabled = true;
            label5.Text = "Status: Decryption Complete!";
            textBox1.ReadOnly = false;
        }
    }
}
