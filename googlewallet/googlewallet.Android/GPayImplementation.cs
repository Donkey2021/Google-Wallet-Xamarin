using System.Collections.Generic;
using Android.Content;
using Android.Gms.Pay;
using Android.Net;
using googlewallet.Droid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(GPayImplementation))]
namespace googlewallet.Droid
{
    public class GPayImplementation : IGPayPass
    {
        //https://github.com/google-pay/pass-converter

        public void LoadGPayPass()
        {
            var ticks = System.DateTime.Now.Ticks;
            var guid = System.Guid.NewGuid().ToString();
            string classSuffix = ticks.ToString() + '_' + guid;

            ticks = System.DateTime.Now.Ticks;
            guid = System.Guid.NewGuid().ToString();
            string objectSuffix = ticks.ToString() + '_' + guid;

            string issuerId = "5350-2331-5996";

            DemoFlight demoFlight = new DemoFlight();
            var JWToken=demoFlight.CreateJWTNewObjects(issuerId, classSuffix, objectSuffix);
            string JWT= JWToken.TrimStart('"').TrimEnd('"');
            //create context
            Context context = Android.App.Application.Context;
            string url = "https://pay.google.com/gp/v/save/" + JWT;

            //Send the https request to google pay pass class object via Android intent
            Intent intent = new Intent(Intent.ActionView, Uri.Parse(url));

            //Assign new task Flag for intent otherwise runtime exepption will return
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
      }
}

