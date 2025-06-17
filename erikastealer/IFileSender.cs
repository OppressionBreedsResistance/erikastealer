using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erikastealer
{
    public interface IFileSender
    {
        Task SendFileAsync(string filePath);
    }
}
