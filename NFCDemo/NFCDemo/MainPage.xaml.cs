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
            nfcService.OnLog += NfcService_OnLog;
        }

        private void NfcService_OnLog(object sender, (string Type, string message) e)
        {
            lblLogs.FormattedText.Spans.Add(new Span() { Text = $"{e.Type}: {e.message} {Environment.NewLine}" });
        } 

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            lblEsl.Text =  await nfcService.ReadTagIdAsync();
        }
        void ClearText()
        {
            lblEsl.Text = "[Placeholder]";
        }

        void ButtonClearLogs_Clicked(System.Object sender, System.EventArgs e)
        {
            lblLogs.FormattedText.Spans.Clear();
        }
    }
}

