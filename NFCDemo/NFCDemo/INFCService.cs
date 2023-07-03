using System;
using System.Threading.Tasks;

namespace NFCDemo
{
    public interface INFCService
    {
        Task<string> GetEslId();
    }
}
