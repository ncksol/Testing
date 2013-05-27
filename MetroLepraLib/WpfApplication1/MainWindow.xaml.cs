using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using MetroLepraLib;

namespace WpfApplication1
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Lepra _lepra = new Lepra();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnLoad_OnClick(object sender, RoutedEventArgs e)
        {
            await _lepra.LoadLoginPage();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = _lepra.GetCaptchaStream();
            bitmap.EndInit();

            imgCaptcha.Source = bitmap;
        }

        private async void BtnLogin_OnClick(object sender, RoutedEventArgs e)
        {
            //var headers = await _lepra.TryLogin(txtCaptcha.Text, txtLogin.Text, txtPassword.Text);
            var headers = await _lepra.TryLogin(txtCaptcha.Text, "dobroe-zlo", "d22msept85y");
            var sb = new StringBuilder();
            var setCookieHeader = headers.FirstOrDefault(x => x.Key == "Set-Cookie");

            foreach (var header in setCookieHeader.Value)
            {
                sb.AppendLine(header);
            }

            txtHeaders.Text = sb.ToString();

            txtError.Text = _lepra.Error;
        }
    }
}