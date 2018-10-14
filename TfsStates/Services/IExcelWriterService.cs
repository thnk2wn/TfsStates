using System.Collections.Generic;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface IExcelWriterService
    {
        void Write(string filename, List<TfsInfo> items, string projectBaseUrl);
    }
}