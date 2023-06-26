using System;
using System.Threading.Tasks;

namespace NFCDemo.Services
{
    public interface INfcService
    {
        Task<string> ReadTagIdAsync();
    }

    public static class EslCommands
    {
        public static byte[] EslIdData => new byte[] {
            (byte)0x00, (byte)0xC0, (byte)0x00,
            (byte)0x01, (byte)0x05, (byte)0x30,
            (byte)0xD0 };
        
    }

}

