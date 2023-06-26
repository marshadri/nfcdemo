using System;
using Android.Nfc;
using NFCDemo.Services;
using System.Threading.Tasks;
using NFCDemo.Droid.Services;
using Android.Nfc.Tech;
using System.IO;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(NfcService))]
namespace NFCDemo.Droid.Services
{
    public class NfcService : Java.Lang.Object, INfcService
    {
        public TaskCompletionSource<string> TagIdTaskCompletionSource { get; set; }

        public Task<string> ReadTagIdAsync()
        {
            TagIdTaskCompletionSource = new TaskCompletionSource<string>();
            return TagIdTaskCompletionSource.Task;
        }

        public void OnTagDiscovered(Tag tag)
        {
            IsoDep isoDep = IsoDep.Get(tag);
            try
            {
                isoDep.Connect();
                byte[] command = EslCommands.EslIdData;
                byte[] response = isoDep.Transceive(command);
                // process the response
                string responseString = BitConverter.ToString(response);//.Replace("-", string.Empty);
                
                TagIdTaskCompletionSource.SetResult(responseString);
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                try { TagIdTaskCompletionSource.SetException(ex); } catch { }
            }
            finally
            {
                try { isoDep.Close(); } catch { }
            }
        }

    }

}

