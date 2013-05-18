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

        public async Task<string> TryLogin(string captcha)
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

            var response = await client.PostAsync("http://leprosorium.ru/login/", content);

            var headers = response.Headers.ToList();
            var htmlData = await response.Content.ReadAsStringAsync();
            return SaveHTMLFile(htmlData);
        }

        public async Task<WriteableBitmap> LoadLoginPage()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://leprosorium.ru");
            var headers = response.Headers.ToList();

            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(await response.Content.ReadAsStreamAsync());

            var loginCodeElement = htmlDocument.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("name", String.Empty) == "logincode");
            _loginCode = loginCodeElement.GetAttributeValue("value", String.Empty);

            var captchaElement = htmlDocument.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("alt", String.Empty) == "captcha");

            _captchaImageDataStream = await client.GetStreamAsync("http://leprosorium.ru" + captchaElement.GetAttributeValue("src", String.Empty));

            return GetCaptcha();
        }

        public WriteableBitmap GetCaptcha()
        {
            var captcha = PictureDecoder.DecodeJpeg(_captchaImageDataStream);
            return captcha;
        }

        private string SaveHTMLFile(string html)
        {
            var fileName = "TextFile1.htm";
            var isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();
            if (isolatedStorageFile.FileExists(fileName))
            {
                isolatedStorageFile.DeleteFile(fileName);
            }

            var data = Encoding.UTF8.GetBytes(html);
            using (var writer = new BinaryWriter(isolatedStorageFile.CreateFile(fileName)))
            {
                writer.Write(data);
                writer.Close();
            }

            return fileName;
        }
    }
}