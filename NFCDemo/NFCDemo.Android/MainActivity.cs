using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using Android.Nfc;
using NFCDemo.Droid.Services;
using NFCDemo.Services;
using Xamarin.Forms;

namespace NFCDemo.Droid
{
    [Activity(Label = "NFCDemo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (NfcAdapter.ActionTagDiscovered.Equals(intent.Action) || NfcAdapter.ActionTechDiscovered.Equals(intent.Action) || NfcAdapter.ActionNdefDiscovered.Equals(intent.Action))
            {
                var tag = (Tag)intent.GetParcelableExtra(NfcAdapter.ExtraTag);

                var nfcService = DependencyService.Get<INfcService>() as NfcService;
                nfcService?.OnTagDiscovered(tag);
            }
           
        }
        protected override void OnResume()
        {
            base.OnResume();

            var nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
            if (nfcAdapter != null)
            {
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Mutable);

                var ndefFilter = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
                ndefFilter.AddDataType("*/*");

                var tagFilter = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                tagFilter.AddCategory(Intent.CategoryDefault);

                var filters = new IntentFilter[] { ndefFilter, tagFilter }; 
                nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters,null);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            var nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
            nfcAdapter?.DisableForegroundDispatch(this);
        }
    }
}
