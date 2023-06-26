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
                AlertMessage = "Hold tag near!"
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
                    tcs.TrySetResult($"Error: {error.LocalizedDescription}");
                    return;
                }
                try
                {
                    using (NativeLibrary.NFCLib lib = new NativeLibrary.NFCLib())
                    {
                        lib.GetEslIdAction(tag, (response) =>
                        {
                            tcs.TrySetResult(response.ToString());
                        });
                    }
                    tcs.TrySetResult("Trying to get Esl Id!");

                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                

            });
        }


        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            TagIdTaskCompletionSource.TrySetException(new Exception(error.LocalizedDescription));
             
        }

    }
}

