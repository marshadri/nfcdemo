using System;
using System.Threading.Tasks;

namespace NFCDemo.Services
{
    public interface INfcService
    {
        Task<string> ReadTagIdAsync();
    }

}

