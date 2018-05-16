using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.Web;
using Windows.Data.Json;
using System.Text.RegularExpressions;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace hw7
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(SearchBox.Text, @"[\u4E00-\u9FA5]+$"))
            {
                weatherSearch(SearchBox.Text);
            }
            else
            {
                IPSearch(SearchBox.Text);
            }
           
        }
        
        // using xml
        private async void weatherSearch(String str)
        {
            String url = String.Format("http://v.juhe.cn/weather/index?format=2&cityname={0}&dtype=xml&key=798e5e948edba42d631e480b7bbde4fb", str);

            HttpClient httpClient = new HttpClient();
            String result = await httpClient.GetStringAsync(new Uri(url));

            XmlDocument document = new XmlDocument();
            document.LoadXml(result);
            XmlNodeList resultList = document.GetElementsByTagName("resultcode");
            if (resultList.Item(0).InnerText == "200")
            {
                resultList = document.GetElementsByTagName("today");
                String info = "温度： " + resultList[0].ChildNodes[0].InnerText + "\n\n";
                info += "天气：" + resultList[0].ChildNodes[1].InnerText + "\n\n";
                info += "风向：" + resultList[0].ChildNodes[3].InnerText + "\n\n";
                info += "城市：" + resultList[0].ChildNodes[5].InnerText + "\n\n";
                info += "日期：" + resultList[0].ChildNodes[6].InnerText + "\n\n";
                weatherDetail.Text = info;
            }
            else
            {
                weatherDetail.Text = "查找不到该城市!";
            }
        }

        // using json
        private async void IPSearch(String str)
        {
            String url = String.Format("http://api.avatardata.cn/IpLookUp/LookUp?key=6ab80a3a071041b1b21d1132e19d34d2&ip={0}", str);

            HttpClient httpClient = new HttpClient();
            String jsonResult = await httpClient.GetStringAsync(new Uri(url));

            JsonObject document = JsonObject.Parse(jsonResult);
            if (document["error_code"].ToString() != "o")
            {
                String info = "";
                JsonObject node = document.GetNamedObject("result");
                info += "IP所在区域: " + node["area"].ToString() + "\n\n";
                info += "IP所在位置: " + node["location"].ToString() + "\n\n";
                locationDetail.Text = info;
            }
            else
            {
                locationDetail.Text = "查找不到该IP地址!";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            weatherDetail.Text = "";
            locationDetail.Text = "";
        }
    }
}
