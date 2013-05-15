using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using HtmlAgilityPack;

namespace MetroLepraLib
{
    public class Lepra
    {
        HttpClient client = new HttpClient();

        private string _captchaUrl;
        private string _loginCode;
        
        public async Task<String> Foo()
        {
            var data = await client.GetAsync("http://leprosorium.ru");
            var dataString = data.Content.ReadAsStringAsync().Result;

            ProcessCaptcha(dataString);

            return _captchaUrl;
        }

        private void ProcessCaptcha(string data)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(data);

            var captchaImage = htmlDocument.DocumentNode.Descendants().Where(x => x.GetAttributeValue("alt", String.Empty) == "captcha").ToList();
            if (captchaImage.Count > 0)
            {
                _captchaUrl = captchaImage.First().GetAttributeValue("src", String.Empty);
            }

            var loginCodeInput = htmlDocument.DocumentNode.Descendants().Where(x => x.GetAttributeValue("name", String.Empty) == "logincode").ToList();
            if (loginCodeInput.Count > 0)
            {
                _loginCode = loginCodeInput.First().GetAttributeValue("value", String.Empty);
            }

        }

        public async void TryLogin(string userName, string password, string captcha)
        {
            HttpContent content = new FormUrlEncodedContent(new[]
                                                                {
                                                                    new KeyValuePair<string,string>("user", userName),
                                                                    new KeyValuePair<string,string>("password", password),
                                                                    new KeyValuePair<string,string>("capture", captcha),
                                                                    new KeyValuePair<string,string>("logincode", _loginCode),
                                                                });
            


            var response = client.PostAsync("http://leprosorium.ru/login/", content);
            
        }
    }
}
