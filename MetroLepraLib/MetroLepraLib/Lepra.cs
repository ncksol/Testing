using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using Microsoft.Phone;

namespace MetroLepraLib
{
    public class Lepra
    {
        private Stream _captchaImageDataStream;
        private string _loginCode;
        

        public String Error { get; set; }

        public async Task<List<KeyValuePair<string, IEnumerable<string>>>> TryLogin(string captcha, string login = "dobroe-zlo", string password="")
        {
            var cookieContainer = new CookieContainer();

            var clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = cookieContainer;

            var client = new HttpClient(clientHandler);
            
            var content = new FormUrlEncodedContent(new[]
                                                        {
                                                            new KeyValuePair<string, string>("user", login),
                                                            new KeyValuePair<string, string>("pass", password),
                                                            new KeyValuePair<string, string>("captcha", captcha),
                                                            new KeyValuePair<string, string>("logincode", _loginCode),
                                                            new KeyValuePair<string, string>("save", "1")
                                                        });

            var message = new HttpRequestMessage(HttpMethod.Post, "http://leprosorium.ru/login/");
            message.Content = content;

            var response = await client.SendAsync(message);

            var data = await response.Content.ReadAsStringAsync();
            GetResponseError(data);

            return response.Headers.ToList();
        }

        public void Foo(string captcha, string login = "dobroe-zlo", string password = "")
        {
            var postString = String.Format("user={0}&pass={1}&captcha={2}&logincode={3}&save=1", login, password, captcha, _loginCode);
            var byteArray = Encoding.UTF8.GetBytes(postString);

            var client = (HttpWebRequest)WebRequest.Create("http://leprosorium.ru/login/");
            client.AllowAutoRedirect = false;
            client.Method = "POST";
            client.ContentType = "application/x-www-form-urlencoded";
            client.ContentLength = byteArray.Length;

            client.BeginGetRequestStream(GetRequestStreamCallback, new KeyValuePair<HttpWebRequest, byte[]>(client, byteArray));
        }

        private void GetRequestStreamCallback(IAsyncResult callbackResult)
        {
            var data = (KeyValuePair<HttpWebRequest, byte[]>)callbackResult.AsyncState;

            var myRequest = data.Key;
            // End the stream request operation
            var postStream = myRequest.EndGetRequestStream(callbackResult);

            // Create the post data
            var byteArray = data.Value;

            // Add the post data to the web request
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            myRequest.BeginGetResponse(GetResponsetStreamCallback, myRequest);
        }

        private async void GetResponsetStreamCallback(IAsyncResult callbackResult)
        {
            var request = (HttpWebRequest)callbackResult.AsyncState;
            var response = (HttpWebResponse)request.EndGetResponse(callbackResult);

            var authCookie = response.Headers["Set-Cookie"];

            var sIdIndex = authCookie.IndexOf("lepro.sid=");
            var sid = authCookie.Substring(sIdIndex);
            sid = sid.Substring(0, sid.IndexOf(';'));
            var sIdParts = sid.Split('=');

            var uidIndex = authCookie.IndexOf("lepro.uid=");
            var uid = authCookie.Substring(uidIndex);
            uid = uid.Substring(0, uid.IndexOf(';'));
            var uidParts = uid.Split('=');

            var rnbumIndex = authCookie.IndexOf("lepro.rnbum=");
            var rnbum = authCookie.Substring(rnbumIndex);
            rnbum = rnbum.Substring(0, rnbum.IndexOf(';'));
            var rnbumParts = rnbum.Split('=');

            var gstqcsaahbv20Index = authCookie.IndexOf("lepro.gstqcsaahbv20=");
            var gstqcsaahbv20 = authCookie.Substring(gstqcsaahbv20Index);
            gstqcsaahbv20 = gstqcsaahbv20.Substring(0, gstqcsaahbv20.IndexOf(';'));
            var gstqcsaahbv20Parts = gstqcsaahbv20.Split('=');

            var saveIndex = authCookie.IndexOf("lepro.save=");
            var save = authCookie.Substring(saveIndex);
            save = save.Substring(0, save.IndexOf(';'));
            var saveParts = save.Split('=');

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(sIdParts[0], sIdParts[1]));
            cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(uidParts[0], uidParts[1]));
            cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(rnbumParts[0], rnbumParts[1]));
            cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(gstqcsaahbv20Parts[0], gstqcsaahbv20Parts[1]));
            cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(saveParts[0], saveParts[1]));


            var handler = new HttpClientHandler {CookieContainer = cookieContainer};

            var client = new HttpClient(handler);
            var message = new HttpRequestMessage(HttpMethod.Get, "http://leprosorium.ru/");
            
            var response2 = await client.SendAsync(message);

            var content = await response2.Content.ReadAsStringAsync();
        }

        private void GetResponseError(string htmlData)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlData);

            var errorDiv = htmlDocument.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("class", String.Empty) == "error" && x.Id != "js-noJsError");
            if(errorDiv == null)
                return;

            Error = errorDiv.InnerText;
        }

        public async Task LoadLoginPage()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://leprosorium.ru/");

            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(await response.Content.ReadAsStreamAsync());

            var loginCodeElement = htmlDocument.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("name", String.Empty) == "logincode");
            _loginCode = loginCodeElement.GetAttributeValue("value", String.Empty);

            var captchaElement = htmlDocument.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("alt", String.Empty) == "captcha");

            _captchaImageDataStream = await client.GetStreamAsync("http://leprosorium.ru" + captchaElement.GetAttributeValue("src", String.Empty));
        }

        public Stream GetCaptchaStream()
        {
            return _captchaImageDataStream;
        }
    }
}