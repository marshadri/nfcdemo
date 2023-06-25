using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFCDemo.Services;
using Xamarin.Forms;

namespace NFCDemo
{
    public partial class MainPage : ContentPage
    {
        INfcService nfcService;
        public MainPage()
        {
            InitializeComponent();
            nfcService = DependencyService.Get<INfcService>();

        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            ClearText();
            nfcService.ReadTagIdAsync().ContinueWith(x =>
            {
                var response = x.Result;
                if (x.Exception == null)
                {
                    lblEsl.Text = response;
                }
                else
                {
                    lblEsl.Text = "Unable to get ESL Id";
                }

            });
            DisplayAlert("NFC Demo", "Approach the tag", "OK");

        }
        void ClearText()
        {
            lblEsl.Text = string.Empty;
        }
    }
}

