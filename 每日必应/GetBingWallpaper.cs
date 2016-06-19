using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace 每日必应
{
    public class EverydayBingWallpaper
    {
        public string GetHttpData(string uri)
        {
            
            Uri url = new Uri(uri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            reader.Close();
            stream.Close();
            return result;
        }

        public string GetInputString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        public string GetBingData(int day, string mkt)
        {
            string url = "Http://test.dou.ms/bing/day/" + day + "/mkt/" + mkt;
            return GetHttpData(url);
        }

        public string GetBingImageUrl(string str)
        {
            string[] strArray = str.Split(new string[] { "地址：" }, StringSplitOptions.RemoveEmptyEntries);
            return strArray[1];
        }

        public void DownLoadImage(string url)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(url, Path.GetFileName(url));
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParm, int fuWinIni);
        public static void SetWallpaper(string path)
        {
            SystemParametersInfo(20, 0, path, 0x01 | 0x02);
        }

        public static string GetBingwallpaper(int day)
        {
            //string day = "1";
            string mkt = "zh-cn";

            EverydayBingWallpaper bingmallpaper = new EverydayBingWallpaper();

            string content = bingmallpaper.GetBingData(day, mkt);

            if (content == "日期不合法或超出范围")
            {
                return content;
            }

            string fileUrl = bingmallpaper.GetBingImageUrl(content);

            bingmallpaper.DownLoadImage(fileUrl);

            string path = Directory.GetCurrentDirectory() + "\\" + System.IO.Path.GetFileName(fileUrl);

            return path;
        }
    }
}
