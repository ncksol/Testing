using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace MetroLepraLib
{
    public class Lepra
    {
        private readonly List<LepraPost> _latestPosts = new List<LepraPost>();
        private int _cacheTime = 30 * 60000;
        private Stream _captchaImageDataStream;
        private string _chatWtf;
        private CookieContainer _cookieContainer;
        private int _karma;
        private DateTime? _lastPostFetchTime;
        private string _loginCode;
        private string _logoutWTF;
        private int _myNewComments;
        private int _myNewPosts;
        private string _myStuffWTF;
        private int _newCommentsCount;
        private int _newPostsCount;
        private String _postVoteWTF;
        private int _rating;
        private string _sId;
        private List<SubLepra> _subLepras;
        private string _uId;
        private string _userName;
        private int _voteWeight;
        private int _postCount;
        private int _screenBiggestMeasure = 1280;

        public event EventHandler LoginSuccessful;
        public event EventHandler<String> LoginFailed;

        public void Login(string captcha, string login = "dobroe-zlo", string password = "")
        {
            var postString = String.Format("user={0}&pass={1}&captcha={2}&logincode={3}&save=1", login, password, captcha, _loginCode);
            var byteArray = Encoding.UTF8.GetBytes(postString);

            var client = (HttpWebRequest) WebRequest.Create("http://leprosorium.ru/login/");
            client.AllowAutoRedirect = false;
            client.Method = "POST";
            client.ContentType = "application/x-www-form-urlencoded";
            client.ContentLength = byteArray.Length;

            client.BeginGetRequestStream(LoginRequestStreamCallback, new KeyValuePair<HttpWebRequest, byte[]>(client, byteArray));
        }

        private void LoginRequestStreamCallback(IAsyncResult callbackResult)
        {
            var data = (KeyValuePair<HttpWebRequest, byte[]>) callbackResult.AsyncState;

            var myRequest = data.Key;
            // End the stream request operation
            var postStream = myRequest.EndGetRequestStream(callbackResult);

            // Create the post data
            var byteArray = data.Value;

            // Add the post data to the web request
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            myRequest.BeginGetResponse(LoginResponsetStreamCallback, myRequest);
        }

        private async void LoginResponsetStreamCallback(IAsyncResult callbackResult)
        {
            var request = (HttpWebRequest) callbackResult.AsyncState;
            var response = (HttpWebResponse) request.EndGetResponse(callbackResult);

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

            if (sIdParts[1] == String.Empty)
            {
                using (var responseContentStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseContentStream);
                    var responseContent = await reader.ReadToEndAsync();
                    var error = GetResponseError(responseContent);
                    OnLoginFailed(error);
                    return;
                }
            }

            _sId = sIdParts[1];
            _uId = uidParts[1];

            _cookieContainer = new CookieContainer();
            _cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(sIdParts[0], sIdParts[1]));
            _cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(uidParts[0], uidParts[1]));
            _cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(rnbumParts[0], rnbumParts[1]));
            _cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(gstqcsaahbv20Parts[0], gstqcsaahbv20Parts[1]));
            _cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie(saveParts[0], saveParts[1]));

            var handler = new HttpClientHandler {CookieContainer = _cookieContainer};

            var client = new HttpClient(handler);
            var message = new HttpRequestMessage(HttpMethod.Get, "http://leprosorium.ru/");

            var response2 = await client.SendAsync(message);

            var content = await response2.Content.ReadAsStringAsync();
            OnLoginSuccessful();

            ProcessMain(content);

            GetLatestPosts(true);
        }

        private string GetResponseError(string htmlData)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlData);

            var errorDiv = htmlDocument.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("class", String.Empty) == "error" && x.Id != "js-noJsError");
            if (errorDiv == null)
                return String.Empty;

            return errorDiv.InnerText;
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

        protected void OnLoginSuccessful()
        {
            if (LoginSuccessful != null)
                LoginSuccessful(this, EventArgs.Empty);
        }

        protected void OnLoginFailed(string error)
        {
            if (LoginFailed != null)
                LoginFailed(this, error);
        }

        private void ProcessMain(string htmlData)
        {
            htmlData = Regex.Replace(htmlData, "\n+", "");
            htmlData = Regex.Replace(htmlData, "\r+", "");
            htmlData = Regex.Replace(htmlData, "\t+", "");

            _postVoteWTF = Regex.Match(htmlData, "wtf_vote = '(.+?)'").Groups[1].Value;
            _myStuffWTF = Regex.Match(htmlData, "mythingsHandler.wtf = '(.+?)'").Groups[1].Value;
            _chatWtf = Regex.Match(htmlData, "chatHandler.wtf = '(.+?)'").Groups[1].Value;
            _userName = Regex.Match(htmlData, "<div id=\"greetings\" class=\"columns_wrapper\">.+?<a href=\".+?\">(.+?)</a>").Groups[1].Value;
            _logoutWTF = Regex.Match(htmlData, "name=\"wtf\" value=\"(.+?)\"").Groups[1].Value;

            // sub lepra regex
            var subReg =
                "<div class=\"sub\"><strong class=\"logo\"><a href=\"(.+?)\" title=\"(.*?)\"><img src=\"(.+?)\" alt=\".+?\" />.+?<div class=\"creator\">.+?<a href=\".*?/users/.+?\">(.+?)</a>";

            var subLepraMatches = Regex.Matches(htmlData, subReg);
            _subLepras = new List<SubLepra>();

            foreach (Match match in subLepraMatches)
            {
                var subLepra = new SubLepra();
                subLepra.Name = match.Groups[2].Value;
                subLepra.Creator = match.Groups[4].Value;
                subLepra.Link = match.Groups[1].Value;
                subLepra.Logo = match.Groups[3].Value;

                _subLepras.Add(subLepra);
            }
        }

        private async void GetNewsCounters()
        {
            var handler = new HttpClientHandler {CookieContainer = _cookieContainer};
            var client = new HttpClient(handler);
            var message = new HttpRequestMessage(HttpMethod.Get, new Uri("http://leprosorium.ru/api/lepropanel"));
            var response = await client.SendAsync(message);

            var leproPanelJsonString = await response.Content.ReadAsStringAsync();

            var leproPanelJObject = JObject.Parse(leproPanelJsonString);
            _newPostsCount = leproPanelJObject["inboxunreadposts"].Value<int>();
            _newCommentsCount = leproPanelJObject["inboxunreadcomms"].Value<int>();
            _karma = leproPanelJObject["karma"].Value<int>();
            _rating = leproPanelJObject["rating"].Value<int>();
            _voteWeight = leproPanelJObject["voteweight"].Value<int>();
            _myNewComments = leproPanelJObject["myunreadcomms"].Value<int>();
            _myNewPosts = leproPanelJObject["myunreadposts"].Value<int>();
        }

        public async Task<List<LepraPost>> GetMyPosts(bool forseRefresh)
        {
            if (_cookieContainer == null)
                FillCookies();

            var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
            var client = new HttpClient(handler);

            var responseString = await client.GetStringAsync("http://leprosorium.ru/my/");

            var myPosts = ProcessHtmlPosts(responseString);

            return myPosts;
        }

        public async Task<List<LepraPost>> GetLatestPosts(bool forseRefresh)
        {
            if (_lastPostFetchTime != null && Math.Abs((DateTime.Now - _lastPostFetchTime.Value).TotalMilliseconds) < _cacheTime && _latestPosts.Count > 0 && !forseRefresh)
            {
                return null;
            }

            _postCount = 0;

            var latestPostsJObject = await RequestPosts();

            ProcessJsonPosts(latestPostsJObject["posts"], _latestPosts);

            return _latestPosts;
        }

        private async Task<JObject> RequestPosts()
        {
            if (_cookieContainer == null)
                FillCookies();

            var handler = new HttpClientHandler {CookieContainer = _cookieContainer};
            var client = new HttpClient(handler);
            var message = new HttpRequestMessage(HttpMethod.Post, new Uri("http://leprosorium.ru/idxctl/"));
            message.Content = new StringContent("from=" + _postCount);
            var response = await client.SendAsync(message);

            var responseContent = await response.Content.ReadAsStringAsync();
            _lastPostFetchTime = DateTime.Now;

            var latestPostsJObject = JObject.Parse(responseContent);
            return latestPostsJObject;
        }

        public async void GetMorePosts()
        {
            _postCount += 42;

            var latestPostsJObject = await RequestPosts();

            ProcessJsonPosts(latestPostsJObject["posts"], _latestPosts);
        }

        private List<LepraPost> ProcessHtmlPosts(String htmlData)
        {
            var myPosts = new List<LepraPost>();

            htmlData = Regex.Replace(htmlData, "\n+", "");
            htmlData = Regex.Replace(htmlData, "\r+", "");
            htmlData = Regex.Replace(htmlData, "\t+", "");

            var postReg ="<div class=\"post.+?id=\"(.+?)\".+?class=\"dt\">(.+?)</div>.+?<a href=\".*?/users/.+?\".*?>(.+?)</a>,(.+?)<span>.+?<a href=\".*?/(comments|inbox)/.+?\">(.+?)</span>.+?.+?(<div class=\"vote\".+?><em>(.+?)</em></span>|</div>)(<a href=\"#\".+?class=\"plus(.*?)\">.+?<a href=\"#\".+?class=\"minus(.*?)\">|</div>)";
            var matches = Regex.Matches(htmlData, postReg);
            foreach (Match match in matches)
            {
                var postBody = match.Groups[2].Value;
                var imgReg = "img src=\"(.+?)\"";
                var res = Regex.Matches(postBody, imgReg);
                var img = "";

                foreach (Match imgMatch in res)
                {
                    if (String.IsNullOrEmpty(img))
                        img = "http://src.sencha.io/80/80/" + imgMatch.Groups[1].Value;

                    postBody = postBody.Replace(imgMatch.Groups[1].Value, "http://src.sencha.io/" + _screenBiggestMeasure + "/" + imgMatch.Groups[1].Value);
                }

                var text = Regex.Replace(postBody, "(<([^>]+)>)", " ");
                if (text.Length > 140)
                {
                    text = text.Substring(0, 140);
                    text += "...";
                }

                var user = match.Groups[3].Value;
                
                var vote = 0;
                if (match.Groups[9].Success && match.Groups[9].Value.Length > 0)
                    vote = 1;
                else if (match.Groups[10].Success && match.Groups[10].Value.Length > 0)
                    vote = -1;

                var post = new LepraPost();
                post.Id = match.Groups[1].Value.Replace("p", "");
                post.Body = postBody;
                post.Rating = match.Groups[8].Value;
                post.User = user;
                post.Type = match.Groups[5].Value;
                post.Url = String.Format("/my/{0}/{1}", post.Type, post.Id);
                post.When = match.Groups[4].Value;

                var comments = Regex.Replace(match.Groups[6].Value, "(<([^>]+)>)", "");
                comments = Regex.Replace(comments, "коммента.+?(\\s|$)", "");
                comments = Regex.Replace(comments, " нов.+", "");
                post.Comments = comments;

                post.Image = img;
                post.Text = text;
                post.Vote = vote;

                myPosts.Add(post);
            }

            return myPosts;
        }

        private void ProcessJsonPosts(JToken parsedPostsJObject, List<LepraPost> latestPosts)
        {
            foreach (var postJObject in parsedPostsJObject)
            {
                var jToken = postJObject.First;

                var postBody = jToken["body"].Value<String>();

                var imgReg = "img src=\"(.+?)\"";
                var res = Regex.Matches(postBody, imgReg);
                var img = "";

                foreach (Match match in res)
                {
                    if(String.IsNullOrEmpty(img))
                        img = "http://src.sencha.io/80/80/" + match.Groups[1].Value;

                    postBody = postBody.Replace(match.Groups[1].Value, "http://src.sencha.io/" + _screenBiggestMeasure + "/" + match.Groups[1].Value);
                }

                var text = Regex.Replace(postBody, "(<([^>]+)>)", " ");
                if (text.Length > 140)
                {
                    text = text.Substring(0, 140);
                    text += "...";
                }

                var post = new LepraPost();
                post.Id = jToken["id"].Value<String>();
                post.Body = postBody;
                post.Rating = jToken["rating"].Value<String>();
                post.Url = jToken["domain_url"].Value<String>();
                post.Image = img;
                post.Text = text;
                post.User = jToken["login"].Value<String>();
                post.Comments = jToken["comm_count"].Value<String>() + " / " + jToken["unread"].Value<String>();
                post.Wrote = String.Format("{0} {1}{2}", jToken["gender"].Value<int>() == 1 ? "Написал " : "Написала ",
                                           (String.IsNullOrEmpty(jToken["user_rank"].Value<String>()) ? "" : jToken["user_rank"].Value<String>() + " "),
                                           jToken["login"].Value<String>());
                                   
                post.When = jToken["textdate"] + " в " + jToken["posttime"];
                post.Vote = jToken["user_vote"].Value<int>();

                latestPosts.Add(post);
            }
        }

        private void FillCookies()
        {
            _cookieContainer = new CookieContainer();
            _cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie("sid", "f7c7cc5378c67dcb66bd113dfbec114d"));
            _cookieContainer.Add(new Uri("http://leprosorium.ru/"), new Cookie("uid", "38633"));
        }
    }
}