using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace Meteo
{
    
    public partial class MainWindow : Window
    {
        public string baseApi = "https://api.openweathermap.org/data/3.0/timemachine?appid=7fb4f4281688c1e0f2ac62294bbd115d&timezone=";
        public string newsApi = "https://newsapi.org/v2/everything?domains=wsj.com&pageSize=6&apiKey=e4095159d8fc42ce99412995661d64a8&fbclid=IwZXh0bgNhZW0CMTAAAR0cJnS8RBNL8WwEVrBxKI2Hv463t0yDPMN4BK5HgIQfcaXI_ujH9ONBk9E_aem_7IyZjpnrhNYaCdXUoIsFmg";
        public string city = "Paris";
        public string jsonString;
        public string newsJsonString;
        public string[] newUrls = new string[10];

        public MainWindow()
        {
            InitializeComponent();

            SetDay();
            //SetHeaderImg();
            GetApiResponse();
            SetUiInfos();
            LoadNews();

        }

        // API Functions
        public void GetApiResponse()
        {
            using (WebClient client = new WebClient())
            {
                //telecharge le json depuis l'api
                jsonString = client.DownloadString(baseApi + city);

                //encode UTF8 de la reponse de l'Api
                byte[] bytes = Encoding.Default.GetBytes(jsonString);   
                jsonString = Encoding.UTF8.GetString(bytes);
                SetUiInfos();
            
            }
        }

        public void SetUiInfos()
        {
            JObject jo = JObject.Parse(jsonString);
            // MessageBox.Show(jsonString);
            weatherDesc.Content = jo["description"];
            tempTxt.Content = jo["temperature"].ToString().Replace(" ","");
            windTxt.Content = jo["wind"];
            CheckWeatherDesc(jo);

            temp1.Content = jo["forecast"][0]["temperature"].ToString().Replace(" ", "");
            temp2.Content = jo["forecast"][1]["temperature"].ToString().Replace(" ", "");
            temp3.Content = jo["forecast"][2]["temperature"].ToString().Replace(" ", "");

        }

        public void CheckWeatherDesc(JObject jo)
        {
            // trouver un mot dans une phrase
            if (jo["description"].ToString().Contains("sun"))
            {
                MessageBox.Show("Sun");
            }
            if (jo["description"].ToString().Contains("cloud"))
            {
                MessageBox.Show("Cloud");
            }
            if (jo["description"].ToString().Contains("rain"))
            {
                SetHeaderImg();
            }
        }

        // Function Aticle
        public void LoadNews()
        {

            List<Article> articles = new List<Article>();

            using (WebClient client = new WebClient())
            {
                //telecharge le json depuis l'api
                newsJsonString = client.DownloadString(newsApi);

                //encode UTF8 de la reponse de l'Api
                byte[] bytes = Encoding.Default.GetBytes(newsJsonString);
                newsJsonString = Encoding.UTF8.GetString(bytes);
            }

            JObject jo = JObject.Parse(newsJsonString);

            for(int i=0; i < jo["articles"].Count(); i++)
            {
                string articleTitle = jo["articles"][i]["title"].ToString();
                articles.Add(new Article() { title = articleTitle });
                newUrls[i] = jo["articles"][i]["url"].ToString();
            }
            
            articlesList.ItemsSource = articles;

        }


        // Functions de base

        public void SetDay()
        {
            string dateFormated = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            dateTxt.Content = CapitalizeStr(dateFormated);

            forecast1.Content = CapitalizeStr(DateTime.Now.AddDays(1).ToString("ddd"));
            forecast2.Content = CapitalizeStr(DateTime.Now.AddDays(2).ToString("ddd"));
            forecast3.Content = CapitalizeStr(DateTime.Now.AddDays(3).ToString("ddd"));
        }

        public string CapitalizeStr(string str)
        {
            string strFormat = str;
            strFormat = char.ToUpper(str[0]) + str.Substring(1);
            return strFormat;
        
        }

        public void SetHeaderImg()
        {
            string meteo = "pluie";

            if (meteo == "pluie")
            {
                headerImg.Source = new BitmapImage(new Uri(@"/rain.png", UriKind.Relative));
            }
            else
            {
                headerImg.Source = new BitmapImage(new Uri(@"/sun.png", UriKind.Relative));
            }
        }


        // Gestion des events

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            city = cityText.Text;
            cityTitle.Content = city;
            GetApiResponse();
            SetUiInfos();
            LoadNews();
        }

        private void btnInfos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Developer par: @FarhaanSurfoodeen", "Credits", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void articlesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var index = articlesList.SelectedIndex;
            // MessageBox.Show("Index :" + index);
            System.Diagnostics.Process.Start(newUrls[index]);
        }
    }

    public class Article
    {
        public string title { get; set; }

        public override string ToString()
        {
            return this.title;
        }
    }
}


