using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
                                                        {
                                                            new KeyValuePair<string, string>("user", login),
                                                            new KeyValuePair<string, string>("pass", password),
                                                            new KeyValuePair<string, string>("captcha", captcha),
                                                            new KeyValuePair<string, string>("logincode", _loginCode),
                                                        });

            var message = new HttpRequestMessage(HttpMethod.Post, "http://leprosorium.ru/login/");
            message.Content = content;

            var response = await client.SendAsync(message);

            var data = await response.Content.ReadAsStringAsync();
            GetResponseError(data);

            return response.Headers.ToList();
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

        public WriteableBitmap GetCaptcha()
        {
            var captcha = PictureDecoder.DecodeJpeg(_captchaImageDataStream);
            return captcha;
        }

        public Stream GetCaptchaStream()
        {
            return _captchaImageDataStream;
        }
    }
}