using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using Android.Nfc;
using Com.Hanshow.Nfc.Util;
using Xamarin.Forms;

namespace NFCDemo.Droid
{
    [Activity(Label = "NFCDemo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        NfcAdapter mAdapter;
        PendingIntent mPendingIntent;
        private NFCService mNfcService;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            mNfcService = NFCService.Instance;
            DependencyService.RegisterSingleton<INFCService>(NFCService.Instance);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            IntentFilter intentFilter = new IntentFilter();


            mAdapter = NfcAdapter.GetDefaultAdapter(this);



            IntentFilter[] filters = new IntentFilter[] {
                new IntentFilter(NfcAdapter.ActionNdefDiscovered),
                new IntentFilter(NfcAdapter.ActionTagDiscovered),
                new IntentFilter(NfcAdapter.ActionTechDiscovered)
};


            NfcAdapter adapter = NfcAdapter.GetDefaultAdapter(this);
            Intent intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);

            if ((int)Build.VERSION.SdkInt >= 31) // Android 12 (API level 31)
            {
                mPendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Mutable);
            }
            else
            {
                mPendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            //base.OnNewIntent(intent);

            //if (NfcAdapter.ActionTagDiscovered.Equals(intent.Action) || NfcAdapter.ActionTechDiscovered.Equals(intent.Action) || NfcAdapter.ActionNdefDiscovered.Equals(intent.Action))
            //{
            //    var tag = (Tag)intent.GetParcelableExtra(NfcAdapter.ExtraTag);

            //    var nfcService = DependencyService.Get<INfcService>() as NfcService;
            //    nfcService?.OnTagDiscovered(tag);
            //}
            mNfcService?.OnNewIntentReceived(intent);


        }
        protected override void OnResume()
        {
            //base.OnResume();

            //var nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
            //if (nfcAdapter != null)
            //{
            //    var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
            //    var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Mutable);

            //    var ndefFilter = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
            //    ndefFilter.AddDataType("*/*");

            //    var tagFilter = new IntentFilter(NfcAdapter.ActionTagDiscovered);
            //    tagFilter.AddCategory(Intent.CategoryDefault);

            //    var filters = new IntentFilter[] { ndefFilter, tagFilter }; 
            //    nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters,null);
            //}

            base.OnResume();
            if (mAdapter != null)
            {
                mAdapter.EnableForegroundDispatch(this, mPendingIntent, null, null);
            }
        }

        protected override void OnPause()
        {
            //base.OnPause();

            //var nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
            //nfcAdapter?.DisableForegroundDispatch(this);
            base.OnPause();
            if (mAdapter != null)
            {
                mAdapter.DisableForegroundDispatch(this);
                mAdapter.DisableForegroundNdefPush(this);
            }
        }
    }
}
