using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreFoundation;
using CoreNFC;
using Foundation;
using NFCDemo.iOS.Services;
using NFCDemo.Services;
using static CoreFoundation.DispatchSource;

[assembly: Xamarin.Forms.Dependency(typeof(NfcService))]
namespace NFCDemo.iOS.Services
{
    public class NfcService : NFCTagReaderSessionDelegate, INfcService
    {
        private NFCTagReaderSession readerSession;

        public event EventHandler<(string Type, string message)> OnLog;

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
                    Log($"Error: {error.LocalizedDescription}");
                    return;
                }
                if (!NativeLibrary.NFCLib.IsSupportedTag(tag))
                {
                    Log($"Error: Tag is not supported!");
                    return;
                }

                try
                {
                    using (NativeLibrary.NFCLib lib = new NativeLibrary.NFCLib())
                    {
                        Log("Trying to get Esl Id!");
                        lib.GetEslIdAction(tag, (response) =>
                        {
                            Log(response.ToString());
                            if (response.Error != null)
                            {
                                Log(response.Error.Description);
                            }
                            else
                            {
                                var data = Convert(response.Data);
                                if(data.ContainsKey("data"))
                                {
                                    tcs.TrySetResult(data["data"]);
                                }
                                else
                                {
                                    Log("Unable to find data", "error");
                                }
                            }
                        });
                    }


                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }


            });
        }

        private static Dictionary<string, string> Convert(NSDictionary nativeDict)
        {
            return nativeDict.ToDictionary<KeyValuePair<NSObject, NSObject>, string, string>(
                item => (NSString)item.Key, item => item.Value.ToString());
        }

        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            TagIdTaskCompletionSource.TrySetException(new Exception(error.LocalizedDescription));

        }

        public void Log(string message, string type = "info")
        {
            OnLog?.Invoke(this, (type, message));
        }
    }
}

