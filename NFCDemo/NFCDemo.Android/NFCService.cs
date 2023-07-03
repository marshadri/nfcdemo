using NFCDemo;
using System.Threading.Tasks;
using Android.Content;
using System;
using Com.Hanshow.Nfc.Util;
namespace NFCDemo.Droid
{
    public class NFCService : INFCService
    {

        // The unique instance of the class.
        private static readonly NFCService _instance = new NFCService();

        // The static property that controls the access to the singleton instance.
        public static NFCService Instance
        {
            get { return _instance; }
        }
        // Make the constructor private so no other class can create instances.
        private NFCService()
        {
            // Initialization code here.
        }
        public event Action<Intent> NewIntentReceived;

        private TaskCompletionSource<string> tcs;

        public Task<string> GetEslId()
        {
            tcs = new TaskCompletionSource<string>();
            return tcs.Task;
        }

        public void OnNewIntentReceived(Intent intent)
        {
            try
            {
                string eslId = NFCLib.GetEslIdAction(intent);
                tcs?.SetResult(eslId);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        }
    }
}

