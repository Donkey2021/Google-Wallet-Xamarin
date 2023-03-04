using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace googlewallet
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void  Button_Clicked(System.Object sender, System.EventArgs e)
        {
            /*string MembershipCardAndroidJWT;
            HttpClient httpClient = new HttpClient();
            var uri = "";
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var ResponseSting = response.Content.ToString();
                MembershipCardAndroidJWT = ResponseSting.TrimStart('"').TrimEnd('"');
                DependencyService.Get<IGPayPass>().LoadGPayPass();
            }*/
            DependencyService.Get<IGPayPass>().LoadGPayPass();
        }
        void generatetoken()
        {

        }
    }
}

