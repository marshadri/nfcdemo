using System;
using System.Linq;
using System.Threading.Tasks;
using CoreFoundation;
using CoreNFC;
using Foundation;
using NFCDemo.iOS.Services;
using NFCDemo.Services;

[assembly: Xamarin.Forms.Dependency(typeof(NfcService))]
namespace NFCDemo.iOS.Services
{
    public class NfcService : NFCTagReaderSessionDelegate, INfcService
    {
        private NFCTagReaderSession readerSession;
        public TaskCompletionSource<string> TagIdTaskCompletionSource { get; set; }


        public Task<string> ReadTagIdAsync()
        {
            TagIdTaskCompletionSource = new TaskCompletionSource<string>();
            readerSession = new NFCTagReaderSession(NFCPollingOption.Iso14443, this, DispatchQueue.CurrentQueue)
            {
                AlertMessage = "Alert"
            };
            readerSession.BeginSession();
            return TagIdTaskCompletionSource.Task.ContinueWith<string>(rx =>
            {
                try
                {
                    if (rx.IsCompletedSuccessfully)
                    {
                        return rx.Result;
                    }
                    else if (rx.Exception != null)
                    {
                        throw rx.Exception;
                    }
                    return string.Empty;
                }
                finally
                {
                    if (rx.Exception != null)
                    {
                        readerSession.InvalidateSession(rx.Exception.ToString());
                    }
                    else
                    {
                        readerSession.InvalidateSession();
                    }
                }
            });
        }

        public override void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
        {
            var tcs = TagIdTaskCompletionSource;
            // handle detected tags if necessary
            var tag = tags.FirstOrDefault();
            session.ConnectTo(tag, (error) =>
            {
                if (error != null)
                {
                    tcs.TrySetResult(null);
                    return;
                }
                
                NativeLibrary.NFCLib lib = new NativeLibrary.NFCLib();
                if (tag is INFCMiFareTag miFareTag)
                {
                    lib.GetEslIdAction(tag, (response) =>
                    {
                        tcs.SetResult(response.ToString());
                    });
                }
               
                //if (tag is INFCMiFareTag miFareTag)
                //{
                //    NSData command = new NSData(Convert.ToBase64String(EslCommands.EslIdData), NSDataBase64DecodingOptions.IgnoreUnknownCharacters); // Read binary blocks

                //    miFareTag.SendMiFareCommand(command, delegate (NSData responseData, NSError _error)
                //    {
                //        if (_error != null)
                //        {
                //            tcs.TrySetResult(null);
                //            return;
                //        }

                //        var result = responseData.ToArray(); // Response data as byte array
                //        tcs.TrySetResult(BitConverter.ToString(result));
                //    });
                //}
            });
        }


        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            TagIdTaskCompletionSource.TrySetException(new Exception(error.LocalizedDescription));
             
        }

    }
}

