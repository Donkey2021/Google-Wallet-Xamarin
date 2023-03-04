using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Walletobjects.v1;
using Google.Apis.Walletobjects.v1.Data;
using googlewallet.Droid;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace googlewallet
{
    public class DemoFlight
    {
        /// <summary>
        /// Path to service account key file from Google Cloud Console. Environment
        /// variable: GOOGLE_APPLICATION_CREDENTIALS.
        /// </summary>
        public static string keyFilePath;

        /// <summary>
        /// Service account credentials for Google Wallet APIs
        /// </summary>
        public static ServiceAccountCredential credentials;

        /// <summary>
        /// Google Wallet service client
        /// </summary>
        public static WalletobjectsService service;

        public DemoFlight()
        {
            var d = Android.OS.Environment.ExternalStorageDirectory;
            var x = Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath;
            EnvironmentVariable.Set("GOOGLE_APPLICATION_CREDENTIALS", x + "/walletproject-379608-8bf2c3cd5632 (1).json");
            keyFilePath = Environment.GetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS") ?? x + "/walletproject-379608-8bf2c3cd5632 (1).json";
            Auth();
        }
        public void Auth()
        {
            credentials = (ServiceAccountCredential)GoogleCredential
                .FromFile(keyFilePath)
                .CreateScoped(new List<string>
                {
        "https://www.googleapis.com/auth/wallet_object.issuer"
                })
                .UnderlyingCredential;

            service = new WalletobjectsService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials
                });
        }
        /// <summary>
        /// Create an object.
        /// </summary>
        /// <param name="issuerId">The issuer ID being used for this request.</param>
        /// <param name="classSuffix">Developer-defined unique ID for this pass class.</param>
        /// <param name="objectSuffix">Developer-defined unique ID for this pass object.</param>
        /// <returns>The pass object ID: "{issuerId}.{objectSuffix}"</returns>
        public string CreateObject(string issuerId, string classSuffix, string objectSuffix)
        {
            // Check if the object exists
            Stream responseStream = service.Flightobject
                .Get($"{issuerId}.{objectSuffix}")
                .ExecuteAsStream();

            StreamReader responseReader = new StreamReader(responseStream);
            JObject jsonResponse = JObject.Parse(responseReader.ReadToEnd());

            if (!jsonResponse.ContainsKey("error"))
            {
                Console.WriteLine($"Object {issuerId}.{objectSuffix} already exists!");
                return $"{issuerId}.{objectSuffix}";
            }
            else if (jsonResponse["error"].Value<int>("code") != 404)
            {
                // Something else went wrong...
                Console.WriteLine(jsonResponse.ToString());
                return $"{issuerId}.{objectSuffix}";
            }

            // See link below for more information on required properties
            // https://developers.google.com/wallet/tickets/boarding-passes/rest/v1/flightobject
            FlightObject newObject = new FlightObject
            {
                Id = $"{issuerId}.{objectSuffix}",
                ClassId = $"{issuerId}.{classSuffix}",
                State = "ACTIVE",
                HeroImage = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = "https://farm4.staticflickr.com/3723/11177041115_6e6a3b6f49_o.jpg"
                    },
                    ContentDescription = new LocalizedString
                    {
                        DefaultValue = new TranslatedString
                        {
                            Language = "en-US",
                            Value = "Hero image description"
                        }
                    }
                },
                TextModulesData = new List<TextModuleData>
    {
      new TextModuleData
      {
        Header = "Text module header",
        Body = "Text module body",
        Id = "TEXT_MODULE_ID"
      }
    },
                LinksModuleData = new LinksModuleData
                {
                    Uris = new List<Google.Apis.Walletobjects.v1.Data.Uri>
      {
        new Google.Apis.Walletobjects.v1.Data.Uri
        {
          UriValue = "http://maps.google.com/",
          Description = "Link module URI description",
          Id = "LINK_MODULE_URI_ID"
        },
        new Google.Apis.Walletobjects.v1.Data.Uri
        {
          UriValue = "tel:6505555555",
          Description = "Link module tel description",
          Id = "LINK_MODULE_TEL_ID"
        }
      }
                },
                ImageModulesData = new List<ImageModuleData>
    {
      new ImageModuleData
      {
        MainImage = new Image
        {
          SourceUri = new ImageUri
          {
            Uri = "http://farm4.staticflickr.com/3738/12440799783_3dc3c20606_b.jpg"
          },
          ContentDescription = new LocalizedString
          {
            DefaultValue = new TranslatedString
            {
              Language = "en-US",
              Value = "Image module description"
            }
          }
        },
        Id = "IMAGE_MODULE_ID"
      }
    },
                Barcode = new Barcode
                {
                    Type = "QR_CODE",
                    Value = "QR code"
                },
                Locations = new List<LatLongPoint>
    {
      new LatLongPoint
      {
        Latitude = 37.424015499999996,
        Longitude = -122.09259560000001
      }
    },
                PassengerName = "Passenger name",
                BoardingAndSeatingInfo = new BoardingAndSeatingInfo
                {
                    BoardingGroup = "B",
                    SeatNumber = "42"
                },
                ReservationInfo = new ReservationInfo
                {
                    ConfirmationCode = "Confirmation code"
                }
            };

            responseStream = service.Flightobject
                .Insert(newObject)
                .ExecuteAsStream();
            responseReader = new StreamReader(responseStream);
            jsonResponse = JObject.Parse(responseReader.ReadToEnd());

            Console.WriteLine("Object insert response");
            Console.WriteLine(jsonResponse.ToString());

            return $"{issuerId}.{objectSuffix}";
        }

        /// <summary>
        /// Generate a signed JWT that creates a new pass class and object.
        /// <para />
        /// When the user opens the "Add to Google Wallet" URL and saves the pass to
        /// their wallet, the pass class and object defined in the JWT are created.
        /// This allows you to create multiple pass classes and objects in one API
        /// call when the user saves the pass to their wallet.
        /// <para />
        /// The Google Wallet C# library uses Newtonsoft.Json.JsonPropertyAttribute
        /// to specify the property names when converting objects to JSON. The
        /// Newtonsoft.Json.JsonConvert.SerializeObject method will automatically
        /// serialize the object with the right property names.
        /// </summary>
        /// <param name="issuerId">The issuer ID being used for this request.</param>
        /// <param name="classSuffix">Developer-defined unique ID for this pass class.</param>
        /// <param name="objectSuffix">Developer-defined unique ID for the pass object.</param>
        /// <returns>An "Add to Google Wallet" link.</returns>
        public string CreateJWTNewObjects(string issuerId, string classSuffix, string objectSuffix)
        {
            // Ignore null values when serializing to/from JSON
            JsonSerializerSettings excludeNulls = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            // See link below for more information on required properties
            // https://developers.google.com/wallet/tickets/boarding-passes/rest/v1/flightclass
            FlightClass newClass = new FlightClass
            {
                Id = $"{issuerId}.{classSuffix}",
                IssuerName = "Issuer name",
                ReviewStatus = "UNDER_REVIEW",
                LocalScheduledDepartureDateTime = "2023-07-02T15:30:00",
                FlightHeader = new FlightHeader
                {
                    Carrier = new FlightCarrier
                    {
                        CarrierIataCode = "LX"
                    },
                    FlightNumber = "123"
                },
                Origin = new AirportInfo
                {
                    AirportIataCode = "LAX",
                    Terminal = "1",
                    Gate = "A2"
                },
                Destination = new AirportInfo
                {
                    AirportIataCode = "SFO",
                    Terminal = "2",
                    Gate = "C3"
                }
            };

            // See link below for more information on required properties
            // https://developers.google.com/wallet/tickets/boarding-passes/rest/v1/flightobject
            FlightObject newObject = new FlightObject
            {
                Id = $"{issuerId}.{objectSuffix}",
                ClassId = $"{issuerId}.{classSuffix}",
                State = "ACTIVE",
                HeroImage = new Image
                {
                    SourceUri = new ImageUri
                    {
                        Uri = "https://farm4.staticflickr.com/3723/11177041115_6e6a3b6f49_o.jpg"
                    },
                    ContentDescription = new LocalizedString
                    {
                        DefaultValue = new TranslatedString
                        {
                            Language = "en-US",
                            Value = "Hero image description"
                        }
                    }
                },
                TextModulesData = new List<TextModuleData>
    {
      new TextModuleData
      {
        Header = "Text module header",
        Body = "Text module body",
        Id = "TEXT_MODULE_ID"
      }
    },
                LinksModuleData = new LinksModuleData
                {
                    Uris = new List<Google.Apis.Walletobjects.v1.Data.Uri>
      {
        new Google.Apis.Walletobjects.v1.Data.Uri
        {
          UriValue = "http://maps.google.com/",
          Description = "Link module URI description",
          Id = "LINK_MODULE_URI_ID"
        },
        new Google.Apis.Walletobjects.v1.Data.Uri
        {
          UriValue = "tel:6505555555",
          Description = "Link module tel description",
          Id = "LINK_MODULE_TEL_ID"
        }
      }
                },
                ImageModulesData = new List<ImageModuleData>
    {
      new ImageModuleData
      {
        MainImage = new Image
        {
          SourceUri = new ImageUri
          {
            Uri = "http://farm4.staticflickr.com/3738/12440799783_3dc3c20606_b.jpg"
          },
          ContentDescription = new LocalizedString
          {
            DefaultValue = new TranslatedString
            {
              Language = "en-US",
              Value = "Image module description"
            }
          }
        },
        Id = "IMAGE_MODULE_ID"
      }
    },
                Barcode = new Barcode
                {
                    Type = "QR_CODE",
                    Value = "QR code"
                },
                Locations = new List<LatLongPoint>
    {
      new LatLongPoint
      {
        Latitude = 37.424015499999996,
        Longitude = -122.09259560000001
      }
    },
                PassengerName = "Passenger name",
                BoardingAndSeatingInfo = new BoardingAndSeatingInfo
                {
                    BoardingGroup = "B",
                    SeatNumber = "42"
                },
                ReservationInfo = new ReservationInfo
                {
                    ConfirmationCode = "Confirmation code"
                }
            };

            // Create JSON representations of the class and object
            JObject serializedClass = JObject.Parse(
                JsonConvert.SerializeObject(newClass, excludeNulls));
            JObject serializedObject = JObject.Parse(
                JsonConvert.SerializeObject(newObject, excludeNulls));

            // Create the JWT as a JSON object
            JObject jwtPayload = JObject.Parse(JsonConvert.SerializeObject(new
            {
                iss = credentials.Id,
                aud = "google",
                origins = new List<string>
    {
      "www.example.com"
    },
                typ = "savetowallet",
                payload = JObject.Parse(JsonConvert.SerializeObject(new
                {
                    // The listed classes and objects will be created
                    // when the user saves the pass to their wallet
                    flightClasses = new List<JObject>
      {
        serializedClass
      },
                    flightObjects = new List<JObject>
      {
        serializedObject
      }
                }))
            }));

            // Deserialize into a JwtPayload
            JwtPayload claims = JwtPayload.Deserialize(jwtPayload.ToString());

            // The service account credentials are used to sign the JWT
            RsaSecurityKey key = new RsaSecurityKey(credentials.Key);
            SigningCredentials signingCredentials = new SigningCredentials(
                key, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken jwt = new JwtSecurityToken(
                new JwtHeader(signingCredentials), claims);
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);

            Console.WriteLine("Add to Google Wallet link");
            Console.WriteLine($"https://pay.google.com/gp/v/save/{token}");

            return token;
            //return $"https://pay.google.com/gp/v/save/{token}";
        }
    }

}

