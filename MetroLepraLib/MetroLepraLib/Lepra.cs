using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using Microsoft.Phone;

namespace MetroLepraLib
{
    public class Lepra
    {
        private Stream _captchaImageDataStream;
        private string _loginCode;

        public async Task<List<KeyValuePair<string, IEnumerable<string>>>> TryLogin(string captcha)
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
                                                        {
                                                            new KeyValuePair<string, string>("user", "dobroe-zlo"),
                                                            new KeyValuePair<string, string>("pass", "d22msept85y"),
                                                            new KeyValuePair<string, string>("captcha", captcha),
                                                            new KeyValuePair<string, string>("logincode", _loginCode),
                                                            new KeyValuePair<string, string>("x", "1"),
                                                            new KeyValuePair<string, string>("y", "6"),
                                                        });

            var message = new HttpRequestMessage(HttpMethod.Post, "http://leprosorium.ru/login/");
            message.Content = content;
            message.Headers.Add("Pragma", new []{"no-cache"});
            message.Headers.Add("Cache-Control", new[] { "no-cache" });

            var response = await client.SendAsync(message);

            return response.Headers.ToList();
        }

        public async Task LoadLoginPage()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://leprosorium.ru");

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