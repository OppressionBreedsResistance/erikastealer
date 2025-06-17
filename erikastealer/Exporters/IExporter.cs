using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erikastealer.Exporters
{
    public interface IExporter
    {
        string CreateBackup();
    }
}
